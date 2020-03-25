using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace OpenVRInputTest
{
    public class Button_B_Event : DigitalControllerEvent
    {
        public override string EventName() { return "B"; }
    }
    public class Button_A_Event : DigitalControllerEvent
    {
        public override string EventName() { return "A"; }
    }
    public class Button_System_Event : DigitalControllerEvent
    {
        public override string EventName() { return "System"; }
    }
    public class Button_Trigger_Event : DigitalControllerEvent
    {
        public override string EventName() { return "Trigger"; }
    }
    public class Button_TriggerVector1_Event : AnalogControllerEvent
    {
        public override string EventName() { return "TriggerVector1"; }
    }
    public class Button_Touchpad_Event : DigitalControllerEvent
    {
        public override string EventName() { return "Touchpad"; }
    }
    public class Button_TouchpadVector2_Event : AnalogControllerEvent
    {
        public override string EventName() { return "TouchpadVector2"; }
    }
    public class Button_ThumbStick_Event : DigitalControllerEvent
    {
        public override string EventName() { return "ThumbStick"; }
    }
    public class Button_ThumbStickVector2_Event : AnalogControllerEvent
    {
        public override string EventName() { return "ThumbStickVector2"; }
    }
    public class Button_Grip_Event : DigitalControllerEvent
    {
        public override string EventName() { return "Grip"; }
    }
    public class Button_GripVector1_Event : AnalogControllerEvent
    {
        public override string EventName() { return "GripVector1"; }
    }
    public abstract class AnalogControllerEvent : ControllerEvent
    {
        public override EventTypeEmun EventType() { return EventTypeEmun.Analog; }
    }
    public abstract class DigitalControllerEvent : ControllerEvent
    {
        public override EventTypeEmun EventType() { return EventTypeEmun.Digital; }
    }
    public abstract class PoseControllerEvent : ControllerEvent
    {
        public override EventTypeEmun EventType() { return EventTypeEmun.Pose; }
    }
    public abstract class ControllerEvent
    {
        public enum EventTypeEmun
        {
            Digital,
            Analog,
            Pose
        }
        public abstract string EventName();
        public abstract EventTypeEmun EventType();

        Controller controller;
        ulong ActionHandle;
        InputDigitalActionData_t Digital;
        InputAnalogActionData_t Analog;
        InputPoseActionData_t Pose;
        HmdVector3_t Position;
        public ControllerEvent()
        {
            switch (EventType())
            {
                case EventTypeEmun.Digital:
                    Digital = new InputDigitalActionData_t();
                    break;
                case EventTypeEmun.Analog:
                    Analog = new InputAnalogActionData_t();
                    break;
                case EventTypeEmun.Pose:
                    Pose = new InputPoseActionData_t();
                    Position = new HmdVector3_t();
                    break;

            }
        }
        public void AttachToController(Controller controller)
        {
            if (controller == null)
                throw new Exception("Parameter in ControllerEvent.AttachToController: controller is Null");
            this.controller = controller;
            var errorID = OpenVR.Input.GetActionHandle(controller.ActionHandleBasePath + EventName(), ref ActionHandle);
            if (errorID != EVRInputError.None)
                Utils.PrintError($"GetActionHandle {controller.ControllerName} {EventName()} Error: {Enum.GetName(typeof(EVRInputError), errorID)}");
            Utils.PrintDebug($"Action Handle {controller.ControllerName} {EventName()}: {ActionHandle}");
        }
        public bool DigitalFetchEventResult()
        {
            var size = (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t));
            OpenVR.Input.GetDigitalActionData(ActionHandle, ref Digital, size, controller.ControllerHandle);
            // Result
            if (Digital.bChanged)
            {
                VREventCallback.NewDigitalEvent(controller.ControllerType, (DigitalControllerEvent)this, Digital.bState);
            }
            return Digital.bState;
        }
        public InputAnalogActionData_t AnalogFetchEventResult()
        {
            var size = (uint)Marshal.SizeOf(typeof(InputAnalogActionData_t));
            OpenVR.Input.GetAnalogActionData(ActionHandle, ref Analog, size, controller.ControllerHandle);
            // Result
            if (Analog.deltaX != 0 || Analog.deltaY != 0 || Analog.deltaZ != 0)
            {
                VREventCallback.NewAnalogEvent(controller.ControllerType, (AnalogControllerEvent)this, Analog);
            }
            return Analog;
        }
        public InputPoseActionData_t PoseFetchEventResult()
        {
            var size = (uint)Marshal.SizeOf(typeof(InputPoseActionData_t));
            OpenVR.Input.GetPoseActionData(ActionHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, 1 / Program.DataFrameRate, ref Pose, size, controller.ControllerHandle);
            if (Pose.pose.bDeviceIsConnected == true && Pose.pose.bPoseIsValid == true)
            {
                HmdVector3_t position = new HmdVector3_t();
                HmdQuaternion_t quaternion = new HmdQuaternion_t();
                TrackableDeviceInfo.GetPosition(Pose.pose.mDeviceToAbsoluteTracking, ref position);
                TrackableDeviceInfo.GetRotation(Pose.pose.mDeviceToAbsoluteTracking, ref quaternion);
                VREventCallback.NewPoseEvent(controller.ControllerType, (PoseControllerEvent)this, position, quaternion);
            }
            return Pose;
        }
    }
}
