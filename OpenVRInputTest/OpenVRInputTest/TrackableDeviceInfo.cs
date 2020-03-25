using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace OpenVRInputTest
{
    public class TrackableDeviceInfo
    {
        public static TrackedDevicePose_t HmdDevicePose
        {
            get
            {
                return TrackedDevicePose_t[0];
            }
        }
        public static TrackedDevicePose_t LeftControllerPose = new TrackedDevicePose_t();
        public static TrackedDevicePose_t RightControllerPose = new TrackedDevicePose_t();
        private static TrackedDevicePose_t[] TrackedDevicePose_t = { new TrackedDevicePose_t() };

        public static void UpdateTrackableDevicePosition()
        {
            VRControllerState_t controllerState = new VRControllerState_t();
            var size = (uint)Marshal.SizeOf(typeof(VRControllerState_t));
            HmdVector3_t position = new HmdVector3_t();
            HmdQuaternion_t quaternion = new HmdQuaternion_t();
            uint LeftControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand),
                RightControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);

            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 1 / Program.DataFrameRate, TrackedDevicePose_t);
            if (TrackedDevicePose_t[0].bPoseIsValid)
            {
                GetPosition(TrackedDevicePose_t[0].mDeviceToAbsoluteTracking, ref position);
                GetRotation(TrackedDevicePose_t[0].mDeviceToAbsoluteTracking, ref quaternion);
                VREventCallback.NewPoseEvent(VREventCallback.DeviceType.HMD, position, quaternion);
            }

            OpenVR.System.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding, LeftControllerIndex, ref controllerState, size, ref LeftControllerPose);
            if (LeftControllerPose.bPoseIsValid)
            {
                GetPosition(LeftControllerPose.mDeviceToAbsoluteTracking, ref position);
                GetRotation(LeftControllerPose.mDeviceToAbsoluteTracking, ref quaternion);
                VREventCallback.NewPoseEvent(VREventCallback.DeviceType.LeftController, position, quaternion);
            }
            OpenVR.System.GetControllerStateWithPose(ETrackingUniverseOrigin.TrackingUniverseStanding, RightControllerIndex, ref controllerState, size, ref RightControllerPose);
            if (RightControllerPose.bPoseIsValid)
            {
                GetPosition(RightControllerPose.mDeviceToAbsoluteTracking, ref position);
                GetRotation(RightControllerPose.mDeviceToAbsoluteTracking, ref quaternion);
                VREventCallback.NewPoseEvent(VREventCallback.DeviceType.RightController, position, quaternion);
            }

        }
        public static double CSharpCopySign(in double x, in double y)
        {
            bool IsXPositive = x >= 0,
                IsYPositive = y >= 0;
            if (IsXPositive != IsYPositive)
                return -1 * x;
            return x;
        }
        //-----------------------------------------------------------------------------
        // Purpose: Calculates quaternion (qw,qx,qy,qz) representing the rotation
        // from: https://github.com/Omnifinity/OpenVR-Tracking-Example/blob/master/HTC%20Lighthouse%20Tracking%20Example/LighthouseTracking.cpp
        //-----------------------------------------------------------------------------
        public static void GetRotation(HmdMatrix34_t matrix, ref HmdQuaternion_t q)
        {
            q.w = Math.Sqrt(Math.Max(0, 1 + matrix.m0 + matrix.m5 + matrix.m10)) / 2;
            q.x = Math.Sqrt(Math.Max(0, 1 + matrix.m0 - matrix.m5 - matrix.m10)) / 2;
            q.y = Math.Sqrt(Math.Max(0, 1 - matrix.m0 + matrix.m5 - matrix.m10)) / 2;
            q.z = Math.Sqrt(Math.Max(0, 1 - matrix.m0 - matrix.m5 + matrix.m10)) / 2;
            q.x = CSharpCopySign(q.x, matrix.m9 - matrix.m6);
            q.y = CSharpCopySign(q.y, matrix.m2 - matrix.m8);
            q.z = CSharpCopySign(q.z, matrix.m4 - matrix.m1);
        }
        //-----------------------------------------------------------------------------
        // Purpose: Extracts position (x,y,z).
        // from: https://github.com/Omnifinity/OpenVR-Tracking-Example/blob/master/HTC%20Lighthouse%20Tracking%20Example/LighthouseTracking.cpp
        //-----------------------------------------------------------------------------
        public static void GetPosition(HmdMatrix34_t matrix, ref HmdVector3_t vector)
        {
            vector.v0 = matrix.m3;
            vector.v1 = matrix.m7;
            vector.v2 = matrix.m11;
        }
    }
}
