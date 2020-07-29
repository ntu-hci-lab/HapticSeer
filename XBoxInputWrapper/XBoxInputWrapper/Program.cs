using System;
using System.Diagnostics;
using System.Threading;
using RedisEndpoint;

namespace XBoxInputWrapper
{
    partial class Program
    {
        const int UpdateInterval_Millisecond = 1;
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
        static Publisher publisher = new Publisher("localhost", 6380);

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (s, e) => { IsProcessKeepRunning = false; };
            ControllerInputThread.Start();
            Thread.Sleep(-1);
        }
        static void EventSender(EventType SourceEvent, string EventInfo)
        {
            publisher.Publish("XINPUT", $"{SourceEvent.ToString()}|{EventInfo}");
            Console.WriteLine(SourceEvent.ToString() + "|" + EventInfo);
        }
        static void BackgroundGetControllerInput()
        {
            var values = Enum.GetValues(typeof(EventType));
            XInputState NewState = new XInputState();
            XInputGetState(0, out NewState);
            int LastPacketIndex = NewState.dwPacketNumber; 
            while (IsProcessKeepRunning)
            {
                Thread.Sleep(UpdateInterval_Millisecond);
                XInputGetState(0, out NewState);
                if (NewState.dwPacketNumber == LastPacketIndex)
                    continue;   //No new data

                foreach (EventType ControllerEvent in values)
                {
                    if (ControllerEvent == EventType.Buttons)
                    {
                        int NewVal = NewState.Gamepad.Buttons;
                        for (int i = 0; i < wButtonsName.Length; ++i)
                        {
                            int Mask = 1 << i;
                            if ((NewVal & Mask) == 0)
                                continue;
                            bool IsNowBtnPressed = (NewVal & Mask) != 0;
                            EventSender(ControllerEvent, $"{wButtonsName[i]}|{(IsNowBtnPressed ? "Pressed" : "Release")}");
                        }
                    }
                    else
                    {
                        object NewVal = GetElementFromXInputState(NewState, ControllerEvent);
                        EventSender(ControllerEvent, $" |{NewVal.ToString()}");
                    }
                }
            }
        }
    }
}
