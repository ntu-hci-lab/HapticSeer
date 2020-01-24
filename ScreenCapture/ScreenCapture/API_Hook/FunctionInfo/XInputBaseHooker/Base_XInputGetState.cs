using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker
{
    public class XINPUT_GAMEPAD
    {
        public static readonly string[] wButtonsName = new string[]
           {
            "DPAD_Up",
            "DPAD_Down",
            "DPAD_Left",
            "DPAD_Right",
            "Start",
            "Back",
            "Left_Thumb",
            "Right_Thumb",
            "Left_Shoulder",
            "Right_Shoulder",
            "DummyField",
            "DummyField",
            "A",
            "B",
            "X",
            "Y"
           };
        public bool[] wButtonsIsPressed = new bool[wButtonsName.Length];
        public uint Index;
        public ushort wButtons
        {
            get
            {
                return (ushort)(RawData[0] | RawData[1] << 8);
            }
        }
        public byte bLeftTrigger
        {
            get
            {
                return RawData[2];
            }
        }
        public byte bRightTrigger
        {
            get
            {
                return RawData[3];
            }
        }
        public short sThumbLX
        {
            get
            {
                return BitConverter.ToInt16(RawData, 4);
            }
        }
        public short sThumbLY
        {
            get
            {
                return BitConverter.ToInt16(RawData, 6);
            }
        }
        public short sThumbRX
        {
            get
            {
                return BitConverter.ToInt16(RawData, 8);
            }
        }
        public short sThumbRY
        {
            get
            {
                return BitConverter.ToInt16(RawData, 10);
            }
        }
        public void FetchFromRawMemory(byte[] MemData)
        {
            Index = BitConverter.ToUInt32(MemData, 0);
            Array.Copy(MemData, 4, RawData, 0, 12); 
            for (int i = 0; i < wButtonsName.Length; ++i)
            {
                int Mask = 1 << i;
                wButtonsIsPressed[i] = (wButtons & Mask) != 0;
            }
        }
        public bool IsDiff(XINPUT_GAMEPAD Target)
        {
            return Target.Index != Index;
        }
        private byte[] RawData = new byte[12];

        public string ToJSON(ushort LeftMotor, ushort RightMotor, long Timestamp)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .Append("{").AppendLine()
                .Append("\"TimeStamp\": ").Append(Timestamp).Append(",").AppendLine()
                .Append("\"Left Motor\": ").Append(LeftMotor).Append(",").AppendLine()
                .Append("\"Right Motor\": ").Append(RightMotor);
            for (int i = 0; i < wButtonsIsPressed.Length; ++i)
            {
                if (wButtonsName[i].Equals("DummyField"))
                    continue;
                stringBuilder.Append(",");
                stringBuilder.AppendLine();
                stringBuilder.Append("\"").Append(wButtonsName[i]).Append("\": ").Append(wButtonsIsPressed[i] ? "true" : "false");
            }
            stringBuilder.Append(",").AppendLine()
                .Append("\"LeftTrigger\": ").Append(bLeftTrigger).Append(",").AppendLine()
                .Append("\"RightTrigger\": ").Append(bRightTrigger).Append(",").AppendLine()
                .Append("\"LeftThumbX\": ").Append(sThumbLX).Append(",").AppendLine()
                .Append("\"LeftThumbY\": ").Append(sThumbLY).Append(",").AppendLine()
                .Append("\"RightThumbX\": ").Append(sThumbRX).Append(",").AppendLine()
                .Append("\"RightThumbY\": ").Append(sThumbRY).AppendLine()
                .Append("}");
            return stringBuilder.ToString();
        }
    }
    public class Base_XInputGetState : FunctionHookBase, IComparable<Base_XInputGetState>
    {
        protected long RemoteDataAddress;
        private byte[] FetchDataStream = new byte[32];
        private XINPUT_GAMEPAD Old = new XINPUT_GAMEPAD(), New = new XINPUT_GAMEPAD();
        public virtual int GetPriority()
        {
            return 0;
        }
        /// <summary>
        /// Fetch Results from remote process.
        /// </summary>
        /// <param name="remoteAPIHook">The parameters that saves the hooked process.</param>
        /// <param name="OnDiffCallBack">A function to process result when data is not equal. OnDiffCallBack(New, Old) </param>
        public XINPUT_GAMEPAD GetData()
        {
            return Old;
        }
        public bool FetchStateFromRemoteProcess(RemoteAPIHook remoteAPIHook)
        {
            remoteAPIHook.RemoteAddressRead((IntPtr)RemoteDataAddress, ref FetchDataStream);
            New.FetchFromRawMemory(FetchDataStream);
            bool IsDiff = New.IsDiff(Old);
            XINPUT_GAMEPAD temp = New;
            New = Old;
            Old = temp;
            return IsDiff;
        }
        public static List<Base_XInputGetState> FetchAllChild()
        {
            List<Base_XInputGetState> objects = new List<Base_XInputGetState>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(Base_XInputGetState)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Base_XInputGetState))))
            {
                objects.Add((Base_XInputGetState)Activator.CreateInstance(type));
            }
            objects.Sort();
            return objects;
        }
        public int CompareTo(Base_XInputGetState other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            else
                return this.GetPriority().CompareTo(other.GetPriority());
        }
    }
}
