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
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
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
using WPFCaptureSample.AudioRecorder;

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
        private Process TargetProcess;
        private RemoteAPIHook remoteAPIHook;
        private ControllerInputFunctionSet ControllerInputHooker;
        private ControllerOutputFunctionSet ControllerOutputHooker;
        private ulong StartRecordTime;
        private BasicCapture basicCapture;
        private BitmapHandler bitmapHandler;
        private PseudoXInput pseudoXInput;
        private uint JSONLastTimeStamp;
        private object JSON_Lock = new object();
        private JSONHandler json;
#if CV_TESTING_OUTPUT
        private string for_cv_logfile = "";
        private short last_x, last_y;
#endif
        AudioLoopback audioCapture;
        [DllImport("kernel32")]
        extern static UInt64 GetTickCount64();
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        //Main Thread Context
        SynchronizationContext syncContext;
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
            return GamePadInfo.ToJSON(LeftMotor, RightMotor, GetTickCount64() - StartRecordTime);
        }
        private void AttachHook(Process process)
        {
#if CV_TESTING_OUTPUT
            for_cv_logfile = "";
#endif
            TimeUIRefresh = new Timer(new TimerCallback(Refresh), null, 0, 1);
            StopButton.IsEnabled = true;
            DateTime date = DateTime.Now;   //Absolute Time
            string DateStr = date.ToString("yyyyMMdd_HHmmss");
            json = new JSONHandler(DateStr + @"\");
            Form_MainWindow.Title = "WPF Capture Sample " + DateStr;
            Directory.CreateDirectory(DateStr);
            bitmapHandler = new BitmapHandler(DateStr + @"\", basicCapture);
            try
            {
                remoteAPIHook = new RemoteAPIHook(process);
                ControllerInputHooker = new ControllerInputFunctionSet(process);
                remoteAPIHook.Hook(ControllerInputHooker);
                ControllerOutputHooker = new ControllerOutputFunctionSet(process);
                remoteAPIHook.Hook(ControllerOutputHooker);
            }catch (UnsupportedModuleException e)
            {
                MessageBox.Show("Cannot found native XINPUT module!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                pseudoXInput = new PseudoXInput(); 
                remoteAPIHook = new RemoteAPIHook(Process.GetCurrentProcess());
                ControllerInputHooker = new ControllerInputFunctionSet(Process.GetCurrentProcess());
                remoteAPIHook.Hook(ControllerInputHooker);
                ControllerOutputHooker = new ControllerOutputFunctionSet(Process.GetCurrentProcess());
                remoteAPIHook.Hook(ControllerOutputHooker);
            }
            basicCapture.proc = process;
            basicCapture.StartRecordTime = StartRecordTime = GetTickCount64();  //Relative Time
            basicCapture.OnBitmapCreate = bitmapHandler.PushBuffer;
            audioCapture.StartRecord(DateStr + @"\");
            SystemSounds.Asterisk.Play();
        }
        public void _Refresh()
        {
            if (StopButton.IsEnabled == false)
                return;
            Label_FPS_Text.Content = ((int)basicCapture.RecordFrameRate).ToString() + " FPS";
            if (ControllerOutputHooker == null)
                return;
            bool IsMotorDiff = ControllerOutputHooker.AccessXInputSetState().FetchStateFromRemoteProcess(remoteAPIHook);
            if (ControllerInputHooker == null)
                return;
            bool IsInputDiff = ControllerInputHooker.AccessXInputGetState().FetchStateFromRemoteProcess(remoteAPIHook);
            if (IsMotorDiff || IsInputDiff) //Not Output Yet
            {
                lock (JSON_Lock)
                {
                    ushort Left, Right;
                    ControllerOutputHooker.AccessXInputSetState().GetData(out Left, out Right);
                    var Data = ControllerInputHooker.AccessXInputGetState().GetData();
                    uint Data_index = Data.Index;
                    string Output = DirectJSONOutput(Data, Left, Right);
                    if (Data_index > JSONLastTimeStamp)
                    {
                        json.AddNew(Output);
                        JSONLastTimeStamp = Data_index;
                    }
#if CV_TESTING_OUTPUT
                    bool IsAccing = Data.bRightTrigger > 0,
                        IsRotating = (last_x != Data.sThumbLX) || (last_y != Data.sThumbLY);
                    IsAccing |= IsRotating;
                    last_x = Data.sThumbLX;
                    last_y = Data.sThumbLY;
                    for_cv_logfile = for_cv_logfile + (GetTickCount64() - StartRecordTime) + "\t" + (IsAccing ? "1" : "0") + "\t" + (IsRotating ? "1" : "0") + "\n";
#endif
                }
            }
        }
        public void Refresh(object state)
        {
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
            syncContext = SynchronizationContext.Current;
            if (GetTickCount64() >=  int.MaxValue * 0.9)
                MessageBox.Show("You had better restart your computer! Poor Computer!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            audioCapture = new AudioLoopback();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = false;
            SystemSounds.Asterisk.Play();
            TimeUIRefresh.Dispose();
            Form_MainWindow.Title = "WPF Capture Sample";
            json?.ToFile();
            json = null;
            ControllerInputHooker = null;
            ControllerOutputHooker = null;
            if (bitmapHandler != null)
                bitmapHandler.IsStart = false;
            if (pseudoXInput != null)
            {
                pseudoXInput.IsStart = false;
                pseudoXInput = null;
            }
            bitmapHandler = null;
            StopCapture();
            audioCapture?.StopRecord();
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
                TargetProcess = process;
                HookButton_Start.IsEnabled = true;
                basicCapture = sample.GetBasicCapture();
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
                sample.StartCaptureFromItem(item);
                string title = item.DisplayName;
                foreach (Process process in WindowComboBox.ItemsSource)
                {
                    if (process.MainWindowTitle.Equals(title))
                    {
                        TargetProcess = process;
                        HookButton_Start.IsEnabled = true;
                        basicCapture = sample.GetBasicCapture();
                        break;
                    }
                }
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

        private void Form_MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (StopButton.IsEnabled)
                StopButton_Click(StopButton, null);
            System.Environment.Exit(0);
        }

        private void HookButton_Stop_Click(object sender, RoutedEventArgs e)
        {
            SystemSounds.Asterisk.Play();
            TimeUIRefresh.Dispose();
            Form_MainWindow.Title = "WPF Capture Sample";
            json.ToFile();
            json = null;
            ControllerInputHooker = null;
            ControllerOutputHooker = null;
            basicCapture.StartRecordTime = 0;
            basicCapture.OnBitmapCreate = null;
            audioCapture.StopRecord();
            bitmapHandler.Done();
            bitmapHandler = null;
            HookButton_Stop.IsEnabled = false;
            HookButton_Start.IsEnabled = true;
#if CV_TESTING_OUTPUT
            File.WriteAllText(@"O:\CV_Log.txt", for_cv_logfile);
#endif
        }

        private void HookButton_Start_Click(object sender, RoutedEventArgs e)
        {
            AttachHook(TargetProcess);
            SetForegroundWindow(TargetProcess.MainWindowHandle);
            HookButton_Stop.IsEnabled = true;
            HookButton_Start.IsEnabled = false;
        }
    }
}
