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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Windows.Foundation.Metadata;
using Windows.Graphics.Capture;
using Windows.UI.Composition;
using WPFCaptureSample.ScreenCapture;
using WPFCaptureSample.ScreenCapture.ImageProcess;

namespace WPFCaptureSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr interopWindowHwnd;
        private Compositor compositor;
        private CompositionTarget target;
        private ContainerVisual root;

        private BasicSampleApplication sample;
        private GraphicsCaptureItem captureItem;
        private OpticalFlow opticalFlow = new OpticalFlow();
        private ScreenClipped screenClipped = new ScreenClipped();
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            // Force graphicscapture.dll to load.
            var picker = new GraphicsCapturePicker();
#endif
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var interopWindow = new WindowInteropHelper(this);
            interopWindowHwnd = interopWindow.Handle;

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

            Task WaitSelectWindowCaptureTask = StartPickerCaptureAsync();
            WaitSelectWindowCaptureTask.Wait();

            if (captureItem == null)
                Environment.Exit(-1);
            else if (!FindHwndByTitle(captureItem.DisplayName, out OnFrameArrivedEvent.TargetHwnd))
                Environment.Exit(-1);
            FrameDispatcher.StartDispatcher();
            sample.basicCapture.BindOnFrameAction(OnFrameArrivedEvent.OnFrameArrived);
        }
        private void InitComposition(float controlsWidth)
        {
            // Create the compositor.
            compositor = new Compositor();

            // Create a target for the window.
            target = compositor.CreateDesktopWindowTarget(interopWindowHwnd, true);

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
        private async Task StartPickerCaptureAsync()
        {
            var picker = new GraphicsCapturePicker();
            picker.SetWindow(interopWindowHwnd);
            captureItem = await picker.PickSingleItemAsync();

            if (captureItem != null)
            {
                sample.StartCaptureFromItem(captureItem);
            }
        }

        private bool FindHwndByTitle(string title, out IntPtr Hwnd)
        {
            var processesWithTitle = from p in Process.GetProcesses()
                                       where !string.IsNullOrWhiteSpace(p.MainWindowTitle) && string.Equals(p.MainWindowTitle, title, StringComparison.Ordinal)
                                       select p;
            if (processesWithTitle.Count<Process>() >= 1)
            {
                Hwnd = processesWithTitle.ElementAt(0).MainWindowHandle;
                return true;
            }else
            {
                Hwnd = IntPtr.Zero;
                return false;
            }
        }
        private void StartHwndCapture(IntPtr Hwnd)
        {
            captureItem = CaptureHelper.CreateItemForWindow(Hwnd);
            if (captureItem != null)
            {
                sample.StartCaptureFromItem(captureItem);
            }
        }
    }
}
