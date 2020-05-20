using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WPFCaptureSample.API_Hook.FunctionInfo.XInputBaseHooker
{
    public class Base_XInputSetState : FunctionHookBase, IComparable<Base_XInputSetState>
    {
        protected long RemoteDataAddress;
        private byte[] FetchDataStream = new byte[4];
        private ushort LeftMotor, RightMotor;
        public virtual int GetPriority()
        {
            return 0;
        }
        /// <summary>
        /// Fetch Results from remote process.
        /// </summary>
        /// <param name="remoteAPIHook">The parameters that saves the hooked process.</param>
        /// <param name="OnDiffCallBack">A function to process result when data is not equal. OnDiffCallBack(Left, Right) </param>
        public void GetData(out ushort LeftMotor, out ushort RightMotor)
        {
            LeftMotor = this.LeftMotor;
            RightMotor = this.RightMotor;
        }
        public bool FetchStateFromRemoteProcess(RemoteAPIHook remoteAPIHook)
        {
            remoteAPIHook.RemoteAddressRead((IntPtr)RemoteDataAddress, ref FetchDataStream);
            ushort NewLeftMotor = BitConverter.ToUInt16(FetchDataStream, 0),
                NewRightMotor = BitConverter.ToUInt16(FetchDataStream, 2);
            bool IsDiff = (LeftMotor != NewLeftMotor || RightMotor != NewRightMotor);
            LeftMotor = NewLeftMotor;
            RightMotor = NewRightMotor;
            return IsDiff;
        }
        public static List<Base_XInputSetState> FetchAllChild()
        {
            List<Base_XInputSetState> objects = new List<Base_XInputSetState>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(Base_XInputSetState)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Base_XInputSetState))))
            {
                objects.Add((Base_XInputSetState)Activator.CreateInstance(type));
            }
            objects.Sort();
            return objects;
        }
        public int CompareTo(Base_XInputSetState other)
        {
            // A null value means that this object is greater.
            if (other == null)
                return 1;

            else
                return this.GetPriority().CompareTo(other.GetPriority());
        }
    }
}
