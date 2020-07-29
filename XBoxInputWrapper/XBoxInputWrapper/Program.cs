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
            XInputState OldState = new XInputState();
            XInputState NewState = new XInputState();
            XInputGetState(0, out OldState);

            while (IsProcessKeepRunning)
            {
                Thread.Sleep(UpdateInterval_Millisecond);
                XInputGetState(0, out NewState);
                if (OldState.dwPacketNumber == NewState.dwPacketNumber)
                    continue;   //No new data

                foreach (EventType ControllerEvent in values)
                {
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
                            EventSender(ControllerEvent, $"{wButtonsName[i]}|{(IsNowBtnPressed ? "Pressed" : "Release")}");
                        }
                    }
                    else
                    {
                        object OldVal = GetElementFromXInputState(OldState, ControllerEvent),
                            NewVal = GetElementFromXInputState(NewState, ControllerEvent);

                        if (!OldVal.Equals(NewVal))
                            EventSender(ControllerEvent, $"{OldVal.ToString()}|{NewVal.ToString()}");
                    }
                }
                Swap(ref OldState, ref NewState);
            }
        }
    }
}
