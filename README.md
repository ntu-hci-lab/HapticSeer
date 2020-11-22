# HapticSeer Tutorial
[HackMD (w/ directory)](https://hackmd.io/@yuhsinlin/HkWxCyWmw)
## Setup
1. Download whole solution
2. Build all
3. Make the config file
4. Run ```{solutionDir}\HapticSeerDashboard\bin\Debug\netcoreapp3.1\HapticSeerDashboard.exe [Config file]```
## Config Structure
```json=
{
  "ExtractorSets": [
    // Add Componests Here
  ],
  "RawCapturers": [
    // Add Components Here
  ],
  "EventDetectors": [
    // Add Components Here
  ]
}
```
## How to listen?
```csharp=
Using RedisEndpoint;

namespace Example
{
    class Program
    {
        const string URL = "localhost";
        const ushort PORT = 6380;
        private static Subscriber mySubscriber = new Subscriber(URL, PORT);

        static int Main()
        {
            mySubscriber.SubscribeTo("TARGET_CHANNEL");
            mySubscriber.msgQueue.OnMessage(msg => {
                //msg.Channel = Channel Name
                //msg.Message = Input Message
            });
            _ = Console.ReadKey();
            return 0;
        }
    }
}
```
---
# Components
## Extractor Sets

### Battlefield 1
```json=
{
      "Type": "BF1",
      "Outlets": [ "[BULLET]", "[BLOOD]", "[HIT]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### Half-Life: Alyx
```json=
{
      "Type": "HLA",
      "Outlets": [ "[BULLET]", "[BLOOD]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### Project Cars 2
```json=
{
  "Type": "PC2",
  "Outlets": [ "[SPEED]" ],
  "Options": {
    "UseShellExecute": "False"
  }
}
```
## Raw Capturers
### OpenVR
```json=
{
      "Type": "OpenVR",
      "Outlets": [ "[OPENVR]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### XInputCap
```json=
{
      "Type": "XInputCap",
      "Outlets": [ "[XINPUT]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### PulseCap
```json=
{
      "Type": "PulseCap",
      "Outlets": [ "[PULSE]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
## Event Detectors
### InertiaDetect
```json=
{
      "Type": "InertiaDetect",
      "Inlets": [ "[SPEED]", "[XINPUT]" ],
      "Outlets": [ "[ACCELX]", "[ACCELY]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### FireDetect (HLA)
```json=
{
      "Type": "FireDetectHLA",
      "Inlets": [ "[BULLET]", "[OPENVR]" ],
      "Outlets": [ "[FIRE]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### FireDetect (BF1)
```json=
{
      "Type": "FireDetectBF1",
      "Inlets": [ "[BULLET]", "[XINPUT]", "[PULSE]" ],
      "Outlets": [ "[FIRE]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### HitDetect (HLA)
```json=
{
      "Type": "HitDetectHLA",
      "Inlets": [ "[BLOOD]" ],
      "Outlets": [ "[HURT]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### HitDetect (BF1)
```json=
{
      "Type": "HitDetectBF1",
      "Inlets": [ "[BLOOD]", "[HIT]" ],
      "Outlets": [ "[IMCOMING]" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
---
# Type of Signals
### BULLET
```
"BulletInGun(Int32)"
```
### BLOOD
```
"HPState(Double)"
```
### HIT
```
"HitAngle(Double)"
```
### SPEED
```
"Speed(Int32)"
```
### OPENVR
```
"SourceTypeName(String)|EventType(String)|EventName(String)|StateInfo(Any)"
```
### XINPUT
```
"SourceEvent(String)|EventInfo(Any)"
```
### PULSE
```
"MonoDetected(Bool)|LFEDetected(Bool)|Angle(Double)"
```
### ACCELX, ACCELY
```
"Accel(Double)"
```
### FIRE
```
"FIRE(Const String)"
```
### HURT
```
"BloodLoss(Double)"
```
### IMCOMING
```
"BloodLoss(Double),HitAngle(Double)"
```
