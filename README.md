# HapticSeer: A Multi-channel, Black-box, Platform-agnostic Approach to Detecting Video Game Events for Real-time Haptic Feedback

## Notice
**This project is undergoing a major code refactoring for a better developer experience.** For instance, we are making feature extractors get rid of integrated raw capturers one by one. 

More details will be announced in next few months.
(If you're interested, the "WIP" repo could be found [HERE](https://github.com/eKL016/HapticSeerNeo))

## System Overview
![](https://i.imgur.com/hZGMbd3.png)
## Steps
### First, initiate HapticSeer to get information from games
1. ```git clone https://github.com/ntu-hci-lab/HapticSeer```
2. Open ```GameSolution.sln``` with Visual Studio
3. Restore NuGet Packages (if needed)![](https://i.imgur.com/bSrOnCX.png)


4. Rebuild solution (Rebuild All)![](https://i.imgur.com/0F0xZju.png)


5. Create a new config file inside ```HapticSeerDashboard```. (e.g. myApp.json)
6. Copy the [skeleton](##Config-Skeleton) into config file. Fill in the blank section with [components](./components.md).
7. Import config file into project ```HapticSeerDashboard``` and make it copy if newer![](https://i.imgur.com/xsXknQF.png)

8. Build again in order to copy the config file into execution folder
9. Start a Windows Command Prompt, type in ```cd {YourSolutionDirHere}\HapticSeerDashboard\bin\Debug\netcoreapp3.1```
10. Start the game. then execute HapticSeer by ```.\HapticSeerDashboard.exe {ConfigFileName}``` for listening to game events
### Next, connect your device to listen event triggers
11. Follow [this section](###Prerequisite) to connect your device to HapticSeer
## Config Skeleton
**Recommended: Pick ```EventDetectors``` first and traceback needed ```ExtractorSets``` and ```RawCaptures``` by inlets/outlets**
```json=
{
  "EventDetectors": [
    // Add Components Here
  ],
  "ExtractorSets": [
    // Add Componests Here
  ],
  "RawCapturers": [
    // Add Components Here
  ]
}
```
---

# How to listen to components with your device?
### Prerequisite
1. Clone project from https://github.com/eKL016/RedisEndpoint
2. Import project into **YOUR SOLUTION**
    * ![](https://i.imgur.com/Te1AzcB.png)



    * ```RedisEndpoint_dotnetCore.csproj``` if using .NET Core
    * ```RedisEndpoint_dotnetFramework.csproj```  if using .NET Framework
3. NuGet install ```StackExchange.Redis```
    1. ![](https://i.imgur.com/jUNRUyL.png)

    2. ![](https://i.imgur.com/SMzXXdq.png)


### Sample Usage
```csharp=
using RedisEndpoint;
using StackExchange.Redis;

namespace Example
{
    class Program
    {
        //Constants, DO NOT MODIFY
        const string URL = "localhost";
        const ushort PORT = 6380;
        
        //Declare a subscriber for SINGLE outlet
        //If you want to subscribe to an another outlet, INSTANTIATE MORE
        private static Subscriber mySubscriber = new Subscriber(URL, PORT);

        static int Main()
        {
            // Listen to a certain TARGET_OUTLET
            mySubscriber.SubscribeTo("TARGET_OUTLET");
            
            //"handler" will be invoked when 
            //the system receives a message from TARGET_CHANNEL
            mySubscriber.msgQueue.OnMessage(handler);
            _ = Console.ReadKey();
            return 0;
        }
        
        // Implement a message Handler
        static void handler(ChannelMessage msg)
        {
            // This struct will be passed into the function
            // ChannelMessage msg 
            // {
            //    string Channel;      => Which channel did this message come from?
            //    string Messages;     => What is the message?
            // }
            // 
            // Example:
            //   Listen to outlet "ACCELY"
            //     Console.WriteLine(msg.Channel);     => "ACCELY"
            //     COnsole.WriteLine(msg.Messages);    => "15.7"
            
            // Invoke your haptic controller here
        }
    }
}
```


