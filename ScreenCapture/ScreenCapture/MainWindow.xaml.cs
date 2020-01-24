//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using CaptureSampleCore;
using Composition.WindowsRuntimeHelpers;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Windows.Foundation.Metadata;
using Windows.Graphics.Capture;
using Windows.UI.Composition;
using WPFCaptureSample.API_Hook;
using WPFCaptureSample.API_Hook.FunctionInfo;
using WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker;

namespace WPFCaptureSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr hwnd;
        private Compositor compositor;
        private CompositionTarget target;
        private ContainerVisual root;
        private BasicSampleApplication sample;
        private ObservableCollection<Process> processes;
        private ObservableCollection<MonitorInfo> monitors;
        //Add for API Hook
        Timer TimeUIRefresh;
        private RemoteAPIHook remoteAPIHook;
        private ControllerInputFunctionSet ControllerInputHooker;
        private ControllerOutputFunctionSet ControllerOutputHooker;
        private Stopwatch timestamp = new Stopwatch();
        //Main Thread Context
        SynchronizationContext syncContext;
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hwnd, out Rect lpRect);


        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            // Force graphicscapture.dll to load.
            var picker = new GraphicsCapturePicker();
#endif
        }

        private async void PickerButton_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            WindowComboBox.SelectedIndex = -1;
            MonitorComboBox.SelectedIndex = -1;
            await StartPickerCaptureAsync();
        }

        private void PrimaryMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            WindowComboBox.SelectedIndex = -1;
            MonitorComboBox.SelectedIndex = -1;
            StartPrimaryMonitorCapture();
        }
        private string DirectJSONOutput(XINPUT_GAMEPAD GamePadInfo, ushort LeftMotor, ushort RightMotor)
        {
            return GamePadInfo.ToJSON(LeftMotor, RightMotor, timestamp.ElapsedMilliseconds);
        }
        private void AttachHook(Process process)
        {
            Rect rect1, rect2;
            GetWindowRect(process.MainWindowHandle, out rect1);
            GetClientRect(process.MainWindowHandle, out rect2);
            timestamp.Restart();
            remoteAPIHook = new RemoteAPIHook(process);
            ControllerInputHooker = new ControllerInputFunctionSet(process);
            ControllerOutputHooker = new ControllerOutputFunctionSet(process);
            remoteAPIHook.Hook(ControllerInputHooker);
            remoteAPIHook.Hook(ControllerOutputHooker);
        }
        public void _Refresh()
        {
            Label_FPS_Text.Content = ((int)sample.FrameRate).ToString() + " FPS";
            /*
            bool IsUpdate = false;
            lock (controllerHooker.EventLock)
            {
                long TimeStampRec = 0;
                if (controllerHooker.EventsRec.Count != 0)
                {
                    IsUpdate = true;
                    for (int i = 0; i < controllerHooker.EventsRec.Count; ++i)
                    {
                        if (TimeStampRec != controllerHooker.EventsTimestamp[0])
                        {
                            TimeStampRec = controllerHooker.EventsTimestamp[0];
                            listView_EventRec.Items.Add("At time: " + TimeStampRec);
                        }
                        controllerHooker.EventsTimestamp.RemoveAt(0);
                        listView_EventRec.Items.Add(controllerHooker.EventsRec[0]);
                        controllerHooker.EventsRec.RemoveAt(0);
                    }
                }
            }
            */
            bool IsMotorDiff = ControllerOutputHooker.AccessXInputSetState().FetchStateFromRemoteProcess(remoteAPIHook);
            bool IsInputDiff = ControllerInputHooker.AccessXInputGetState().FetchStateFromRemoteProcess(remoteAPIHook);
            if (IsMotorDiff || IsInputDiff) //Not Output Yet
            {
                ushort Left, Right;
                ControllerOutputHooker.AccessXInputSetState().GetData(out Left, out Right);
                string Output = DirectJSONOutput(ControllerInputHooker.AccessXInputGetState().GetData(), Left, Right);
                Console.WriteLine(Output);
            }

            //if (IsUpdate)
            //    listView_EventRec.ScrollIntoView(listView_EventRec.Items[listView_EventRec.Items.Count - 1]);
        }
        public void Refresh(object state)
        {
            if (ControllerInputHooker == null)
                return;
            syncContext.Send((s) =>
            {
                _Refresh();
            }, null);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var interopWindow = new WindowInteropHelper(this);
            hwnd = interopWindow.Handle;

            var presentationSource = PresentationSource.FromVisual(this);
            double dpiX = 1.0;
            double dpiY = 1.0;
            if (presentationSource != null)
            {
                dpiX = presentationSource.CompositionTarget.TransformToDevice.M11;
                dpiY = presentationSource.CompositionTarget.TransformToDevice.M22;
            }
            var controlsWidth = (float)(ControlsGrid.ActualWidth * dpiX);

            InitComposition(controlsWidth);
            InitWindowList();
            InitMonitorList();
            TimeUIRefresh = new Timer(new TimerCallback(Refresh), null, 0, 1);
            syncContext = SynchronizationContext.Current;
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopCapture();
            WindowComboBox.SelectedIndex = -1;
            MonitorComboBox.SelectedIndex = -1;
        }

        private void WindowComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var process = (Process)comboBox.SelectedItem;

            if (process != null)
            {
                StopCapture();
                MonitorComboBox.SelectedIndex = -1;
                string c = process.Modules[0].ModuleName;
                AttachHook(process);
                var hwnd = process.MainWindowHandle;
                try
                {
                    StartHwndCapture(hwnd);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Hwnd 0x{hwnd.ToInt32():X8} is not valid for capture!");
                    processes.Remove(process);
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        private void MonitorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            var monitor = (MonitorInfo)comboBox.SelectedItem;

            if (monitor != null)
            {
                StopCapture();
                WindowComboBox.SelectedIndex = -1;
                var hmon = monitor.Hmon;
                try
                {
                    StartHmonCapture(hmon);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Hmon 0x{hmon.ToInt32():X8} is not valid for capture!");
                    monitors.Remove(monitor);
                    comboBox.SelectedIndex = -1;
                }
            }
        }

        private void InitComposition(float controlsWidth)
        {
            // Create the compositor.
            compositor = new Compositor();

            // Create a target for the window.
            target = compositor.CreateDesktopWindowTarget(hwnd, true);

            // Attach the root visual.
            root = compositor.CreateContainerVisual();
            root.RelativeSizeAdjustment = Vector2.One;
            root.Size = new Vector2(-controlsWidth, 0);
            root.Offset = new Vector3(controlsWidth, 0, 0);
            target.Root = root;

            // Setup the rest of the sample application.
            sample = new BasicSampleApplication(compositor);
            root.Children.InsertAtTop(sample.Visual);
        }

        private void InitWindowList()
        {
            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
            {
                var processesWithWindows = from p in Process.GetProcesses()
                                           where !string.IsNullOrWhiteSpace(p.MainWindowTitle) && WindowEnumerationHelper.IsWindowValidForCapture(p.MainWindowHandle)
                                           select p;
                processes = new ObservableCollection<Process>(processesWithWindows);
                WindowComboBox.ItemsSource = processes;
            }
            else
            {
                WindowComboBox.IsEnabled = false;
            }
        }

        private void InitMonitorList()
        {
            if (ApiInformation.IsApiContractPresent(typeof(Windows.Foundation.UniversalApiContract).FullName, 8))
            {
                monitors = new ObservableCollection<MonitorInfo>(MonitorEnumerationHelper.GetMonitors());
                MonitorComboBox.ItemsSource = monitors;
            }
            else
            {
                MonitorComboBox.IsEnabled = false;
                PrimaryMonitorButton.IsEnabled = false;
            }
        }

        private async Task StartPickerCaptureAsync()
        {
            var picker = new GraphicsCapturePicker();
            picker.SetWindow(hwnd);
            GraphicsCaptureItem item = await picker.PickSingleItemAsync();
            if (item != null)
            {
                InitWindowList();
                string title = item.DisplayName;
                foreach (Process process in WindowComboBox.ItemsSource)
                {
                    if (process.MainWindowTitle.Equals(title))
                    {
                        AttachHook(process);
                        break;
                    }
                }
                sample.StartCaptureFromItem(item);
            }
        }

        private void StartHwndCapture(IntPtr hwnd)
        {
            GraphicsCaptureItem item = CaptureHelper.CreateItemForWindow(hwnd);
            if (item != null)
            {
                sample.StartCaptureFromItem(item);
            }
        }

        private void StartHmonCapture(IntPtr hmon)
        {
            GraphicsCaptureItem item = CaptureHelper.CreateItemForMonitor(hmon);
            if (item != null)
            {
                sample.StartCaptureFromItem(item);
            }
        }

        private void StartPrimaryMonitorCapture()
        {
            MonitorInfo monitor = (from m in MonitorEnumerationHelper.GetMonitors()
                           where m.IsPrimary
                           select m).First();
            StartHmonCapture(monitor.Hmon);
        }

        private void StopCapture()
        {
            sample.StopCapture();
        }

        private void Button_RefreshEventList_Click(object sender, RoutedEventArgs e)
        {
            listView_EventRec.Items.Clear();
        }
    }
}
