# HapticSeer Tutorial

## System Overview
![](https://i.imgur.com/hZGMbd3.png)
## Steps
### First, use HapticSeer to get information from games
1. [Clone whole solution](https://github.com/eKL016/GameAgent)
2. Open ```GameSolution.sln``` with Visual Studio
3. Restore NuGet Packages (if needed)![](https://i.imgur.com/o73V9Ng.png)

4. Rebuild solution (Rebuild All)![](https://i.imgur.com/CmspTYL.png)

5. Create a new config file inside ```HapticSeerDashboard```. (e.g. myApp.json)
6. Copy the [skeleton](##Config-Skeleton) into config file. Fill in the blank section with [components](./components.md).
7. Import config file into project ```HapticSeerDashboard``` and make it copied every build![](https://i.imgur.com/jfKompE.png)
8. Build again in order to copy the config file into execution folder
9. Open cmd ![](https://i.imgur.com/INJYRUb.png), type in ```cd {YourSolutionDirHere}\HapticSeerDashboard\bin\Debug\netcoreapp3.1```
10. Open the game. Type in ```.\HapticSeerDashboard.exe {ConfigFileName}``` to start listening to game events
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
    * ![](https://i.imgur.com/kMuhJt7.png)


    * ```RedisEndpoint_dotnetCore.csproj``` if using .NET Core
    * ```RedisEndpoint_dotnetFramework.csproj```  if using .NET Framework
3. NuGet install ```StackExchange.Redis```
    1. ![](https://i.imgur.com/XDwd129.png)
    2. ![](https://i.imgur.com/zQgPrCS.png)


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


