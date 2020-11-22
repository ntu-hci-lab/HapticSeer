#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Valve.VR;
using RedisEndpoint;
using static OpenVRInputTest.VREventCallback;
//Sources: https://github.com/BOLL7708/OpenVRInputTest
namespace OpenVRInputTest
{
    class Program
    {
        public static Publisher publisher = new Publisher("localhost", 6380);
        public static string outletChannelName;
        public static float DataFrameRate = 90f;
        static ulong mActionSetHandle;
        //static ulong mActionHandleLeftB, mActionHandleRightB, mActionHandleLeftA, mActionHandleRightA, mActionHandleChord1, mActionHandleChord2;
        static VRActiveActionSet_t[] mActionSetArray;
        //static InputDigitalActionData_t[] mActionArray;
        static Controller rightController, leftController;
        // # items are referencing this list of actions: https://github.com/ValveSoftware/openvr/wiki/SteamVR-Input#getting-started
        static void Main(string[] args)
        {
            outletChannelName = args[0];
            // Initializing connection to OpenVR
            var error = EVRInitError.None;
            OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background); // Had this as overlay before to get it working, but changing it back is now fine?
            var workerThread = new Thread(Worker);
            if (error != EVRInitError.None)
                Utils.PrintError($"OpenVR initialization errored: {Enum.GetName(typeof(EVRInitError), error)}");
            else
            {
                Utils.PrintInfo("OpenVR initialized successfully.");

                // Load app manifest, I think this is needed for the application to show up in the input bindings at all
                Utils.PrintVerbose("Loading app.vrmanifest");
                Console.WriteLine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFullPath("app.vrmanifest")));
                var appError = OpenVR.Applications.AddApplicationManifest(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFullPath("app.vrmanifest")), false);
                if (appError != EVRApplicationError.None)
                    Utils.PrintError($"Failed to load Application Manifest: {Enum.GetName(typeof(EVRApplicationError), appError)}");
                else 
                    Utils.PrintInfo("Application manifest loaded successfully.");

                // #3 Load action manifest
                Utils.PrintVerbose("Loading actions.json");
                var ioErr = OpenVR.Input.SetActionManifestPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFullPath("actions.json")));
                if (ioErr != EVRInputError.None) 
                    Utils.PrintError($"Failed to load Action Manifest: {Enum.GetName(typeof(EVRInputError), ioErr)}");
                else 
                    Utils.PrintInfo("Action Manifest loaded successfully.");

                // #4 Get action handles
                Utils.PrintVerbose("Getting action handles");
                rightController = 
                    new Controller(DeviceType.RightController, "RightController", "/user/hand/right", "/actions/default/in/right_")
                    .AttachNewEvent(new Button_B_Event())
                    .AttachNewEvent(new Button_A_Event())
                    .AttachNewEvent(new Button_Trigger_Event())
                    .AttachNewEvent(new Button_TriggerVector1_Event())
                    .AttachNewEvent(new Button_Touchpad_Event())
                    .AttachNewEvent(new Button_TouchpadVector2_Event())
                    .AttachNewEvent(new Button_System_Event())
                    .AttachNewEvent(new Button_ThumbStick_Event())
                    .AttachNewEvent(new Button_ThumbStickVector2_Event())
                    .AttachNewEvent(new Button_Grip_Event())
                    .AttachNewEvent(new Button_GripVector1_Event());

                leftController = 
                    new Controller(DeviceType.LeftController, "LeftController", "/user/hand/left", "/actions/default/in/left_")
                    .AttachNewEvent(new Button_B_Event())
                    .AttachNewEvent(new Button_A_Event())
                    .AttachNewEvent(new Button_Trigger_Event())
                    .AttachNewEvent(new Button_TriggerVector1_Event())
                    .AttachNewEvent(new Button_Touchpad_Event())
                    .AttachNewEvent(new Button_TouchpadVector2_Event())
                    .AttachNewEvent(new Button_System_Event())
                    .AttachNewEvent(new Button_ThumbStick_Event())
                    .AttachNewEvent(new Button_ThumbStickVector2_Event())
                    .AttachNewEvent(new Button_Grip_Event())
                    .AttachNewEvent(new Button_GripVector1_Event());
                
                // #5 Get action set handle
                Utils.PrintVerbose("Getting action set handle");
                var errorAS = OpenVR.Input.GetActionSetHandle("/actions/default", ref mActionSetHandle);
                if (errorAS != EVRInputError.None) 
                    Utils.PrintError($"GetActionSetHandle Error: {Enum.GetName(typeof(EVRInputError), errorAS)}");
                Utils.PrintDebug($"Action Set Handle default: {mActionSetHandle}");

                // Starting worker
                Utils.PrintDebug("Starting worker thread.");
                if (!workerThread.IsAlive) 
                    workerThread.Start();
                else 
                    Utils.PrintError("Could not start worker thread.");
            }
            Console.ReadLine();
            workerThread.Abort();
            OpenVR.Shutdown();
        }

        private static void Worker()
        {
            Thread.CurrentThread.IsBackground = true;
            while (true)
            {
                TrackableDeviceInfo.UpdateTrackableDevicePosition();

                // Getting events
                var vrEvents = new List<VREvent_t>();
                var vrEvent = new VREvent_t();
                try
                {
                    while (OpenVR.System.PollNextEvent(ref vrEvent, Utils.SizeOf(vrEvent)))
                    {
                        vrEvents.Add(vrEvent);
                    }
                }
                catch (Exception e)
                {
                    Utils.PrintWarning($"Could not get events: {e.Message}");
                }

                // Printing events
                foreach (var e in vrEvents)
                {
                    var pid = e.data.process.pid;
                    if (e.eventType == (uint)EVREventType.VREvent_Input_HapticVibration)
                    {
                        ETrackedControllerRole DeviceType = OpenVR.System.GetControllerRoleForTrackedDeviceIndex(e.data.process.pid);
                        if (DeviceType != ETrackedControllerRole.LeftHand && DeviceType != ETrackedControllerRole.RightHand)
                            continue;
                        NewVibrationEvent(DeviceType, e.data.hapticVibration);
                    }
#if DEBUG
                    if ((EVREventType)vrEvent.eventType != EVREventType.VREvent_None)
                    {
                        var name = Enum.GetName(typeof(EVREventType), e.eventType);
                        var message = $"[{pid}] {name}";
                        if (pid == 0) 
                            Utils.PrintVerbose(message);
                        else if (name == null) 
                            Utils.PrintVerbose(message);
                        else if (name.ToLower().Contains("fail")) 
                            Utils.PrintWarning(message);
                        else if (name.ToLower().Contains("error"))
                            Utils.PrintError(message);
                        else if (name.ToLower().Contains("success"))
                            Utils.PrintInfo(message);
                        else 
                            Utils.Print(message);
                    }
#endif
                }

                // #6 Update action set
                if (mActionSetArray == null)
                {
                    var actionSet = new VRActiveActionSet_t
                    {
                        ulActionSet = mActionSetHandle,
                        ulRestrictedToDevice = OpenVR.k_ulInvalidActionSetHandle,
                        nPriority = 0
                    };
                    mActionSetArray = new VRActiveActionSet_t[] { actionSet };
                }

                var errorUAS = OpenVR.Input.UpdateActionState(mActionSetArray, (uint)Marshal.SizeOf(typeof(VRActiveActionSet_t)));
                if (errorUAS != EVRInputError.None) Utils.PrintError($"UpdateActionState Error: {Enum.GetName(typeof(EVRInputError), errorUAS)}");

                // #7 Load input action data
                leftController.UpdateAllState();
                rightController.UpdateAllState();

                // Restrict rate
                Thread.Sleep((int)(1000 / DataFrameRate));
            }
        }
    }
}

