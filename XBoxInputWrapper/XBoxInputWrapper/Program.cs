using System;
using System.Diagnostics;
using System.Threading;
using XBoxInputWrapper.API_Hook;
using XBoxInputWrapper.API_Hook.FunctionInfo;

namespace XBoxInputWrapper
{
    partial class Program
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
        static bool IsProcessKeepRunning = true;
        static Thread ControllerInputThread = new Thread(BackgroundGetControllerInput);
        static Thread ControllerOutputThread;
        static void Main(string[] args)
        {
            Console.CancelKeyPress += (s, e) => { IsProcessKeepRunning = false; };
            if (args.Length == 1)
            {
                int RemotePID = int.Parse(args[0]);
                Process process = Process.GetProcessById(RemotePID);
                bool IsRemoteProcessOnWoW64,
                    IsSelfProcessOnWoW64 = (IntPtr.Size == 4);
                IsWow64Process(process.Handle, out IsRemoteProcessOnWoW64);
                if (IsRemoteProcessOnWoW64 != IsSelfProcessOnWoW64)
                    throw new NotImplementedException();
                ControllerOutputThread = new Thread(BackgroundGetControllerOutput);
                ControllerOutputThread.Start(process);
            }
            else if (args.Length > 1)
                throw new Exception("Args more than 1!");

            ControllerInputThread.Start();
            Thread.Sleep(-1);
        }
        static void EventSender(EventType SourceEvent, string EventInfo)
        {
            Console.WriteLine(SourceEvent.ToString() + "\t" + EventInfo);
        }
        static void BackgroundGetControllerOutput(object process)
        {
            Process _process = process as Process;
            RemoteAPIHook remoteAPIHook = new RemoteAPIHook(_process);
            ControllerOutputFunctionSet ControllerOutputHooker = new ControllerOutputFunctionSet(_process);
            if (!remoteAPIHook.Hook(ControllerOutputHooker))
                throw new Exception($"Cannot attach hook on process {_process.ProcessName}");
            var XInputSetObj = ControllerOutputHooker.AccessXInputSetState();
            ushort OldLeftMotor = 0, OldRightMotor = 0;
            while (IsProcessKeepRunning)
            {
                Thread.Sleep(1);
                ushort NewLeftMotor = 0, NewRightMotor = 0;
                XInputSetObj.FetchStateFromRemoteProcess(remoteAPIHook);
                ControllerOutputHooker.AccessXInputSetState().GetData(out NewLeftMotor, out NewRightMotor);
                if (NewLeftMotor != OldLeftMotor)
                {
                    EventSender(EventType.LeftMotor, $"{OldLeftMotor.ToString()} -> {NewLeftMotor.ToString()}");
                    OldLeftMotor = NewLeftMotor;
                }
                if (NewRightMotor != OldRightMotor)
                {
                    EventSender(EventType.RightMotor, $"{OldRightMotor.ToString()} -> {NewRightMotor.ToString()}");
                    OldRightMotor = NewRightMotor;
                }
            }
        }

        static void BackgroundGetControllerInput()
        {
            var values = Enum.GetValues(typeof(EventType));
            XInputState OldState = new XInputState();
            XInputState NewState = new XInputState();
            XInputGetState(0, out OldState);

            while (IsProcessKeepRunning)
            {
                Thread.Sleep(1);
                XInputGetState(0, out NewState);
                if (OldState.dwPacketNumber == NewState.dwPacketNumber)
                    continue;   //No new data

                foreach (EventType ControllerEvent in values)
                {
                    if (ControllerEvent == EventType.LeftMotor || ControllerEvent == EventType.RightMotor)
                        continue;

                    if (ControllerEvent == EventType.Buttons)
                    {
                        int OldVal = OldState.Gamepad.Buttons,
                            NewVal = NewState.Gamepad.Buttons;
                        int StateChangedButton = OldVal ^ NewVal;
                        if (StateChangedButton == 0)
                            continue;
                        for (int i = 0; i < wButtonsName.Length; ++i)
                        {
                            int Mask = 1 << i;
                            if ((StateChangedButton & Mask) == 0)
                                continue;
                            bool IsNowBtnPressed = (NewVal & Mask) != 0;
                            EventSender(ControllerEvent, $"{wButtonsName[i]} {(IsNowBtnPressed ? "Pressed" : "Release")}");
                        }
                    }
                    else
                    {
                        object OldVal = GetElementFromXInputState(OldState, ControllerEvent),
                            NewVal = GetElementFromXInputState(NewState, ControllerEvent);

                        if (!OldVal.Equals(NewVal))
                            EventSender(ControllerEvent, $"{OldVal.ToString()} -> {NewVal.ToString()}");
                    }
                }

                Swap(ref OldState, ref NewState);
            }
        }
    }
}
