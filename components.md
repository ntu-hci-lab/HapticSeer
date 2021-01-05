# HapticSeer Components
You may copy components here to the skeleton file directly.
***NOTICE: Comments are not supported in JSON files, please remove them before use!***

Three games to choose from:
* HLA(Half-Life: Alyx): a VR first-person shooter game.
* BF1(Battlefield 1): a first-person shooter PC video game.
* PC2(Project Cars 2): a motorsport racing simulator video game supports VR and PC.

### Rules
1. If you want to connect A component with B component, make sure they have pairs of inlet/outlet with the same name (i.e., they should connect to the same channel)
2. Naming of inlets/outlets **would not affect** contents passing through them, i.e., ```Outlets: ["A", "B", "C"]``` is equal to ```Outlets: ["X", "Y", "Z"]```.
3. Position of inlets/outlets **matters**. 
    * e.g., ```HUDExtractorSet``` with preset ```BF1``` will always send ```"BulletInGun(Int32)"``` messages by its first outlet.
4. You may found all of possible messages [HERE](./listOfOutletMessages.md), follow their format
5. If you want to see output of a component in a seperated console, enable ```UseShellExecute```
---
![](https://i.imgur.com/hZGMbd3.png)

## Event Detectors
Choose the event(Outlets) you want to detect from the following detectors.

We provide three kinds of detectors:
* InertiaDetector
* FiringDetector
* HitDetector
### 1. InertiaDetector
Get the range of acceleration magnitude. Triggered when car accelerates, deccelerates, or turns.
```json=
{
      "Name": "InertiaDetector",
      "Preset": "PC2",
      "Inlets": [ //Please copy corresponding inlets ],
      "Outlets": [ //Please copy corresponding outlets ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
#### **Required** Inlets

PC2: ```["SPEED", "XINPUT"]```

#### Default Outlets

PC2: ```["ACCELX", "ACCELY"]```

### 2. FiringDetector
Get the event of gun firing.
```json=
{
      "Name": "FiringDetector",
      "Preset": "BF1", "HLA", //Choose One
      "Inlets": [ "//Please copy corresponding inlets" ],
      "Outlets": [ "FIRE" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
#### **Required** Inlets

BF1: ```[ "BULLET", "XINPUT", "PULSE" ]```
HLA: ```[ "BULLET", "OPENVR" ]```

#### Default Outlets

BF1: ```[ "FIRE" ]```
HLA: ```[ "FIRE" ]```

### 3. HitDetector
Triggered when the player lose health.
```json=
{
      "Name": "HitDetector",
      "Preset": "BF1", "HLA", //Choose One
      "Inlets": [ "//Please copy corresponding inlets" ],
      "Outlets": [ "//Please copy corresponding outlets" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
#### **Required** Inlets
BF1: ```[ "BLOOD", "HIT" ]```
HLA: ```[ "BLOOD" ]```

#### Default Outlets 
BF1: ```[ "IMCOMING" ]```
HLA: ```[ "HURT" ]```


## Extractor Sets
According to the inlets you previously used, select the matching extractor outlets.
### 1. HUDExtractorSet
Get information from in-game Heads Up Display(HUD).
```json=
{
      "Name": "HUDExtractorSet",
      "Preset": "BF1", "HLA", "PC2", //Choose One
      "Outlets": [ "Please copy corresponding outlets" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
#### Default Outlets
BF1: ```[ "BULLET", "BLOOD", "HIT" ]```
HLA: ```[ "BULLET", "BLOOD"]```
PC2: ```[ "SPEED" ]```

### 2. GenericPulseExtractor
Get pulse data from audio stream.
```json=
{
      "Name": "GenericPulseExtractor",
      "Outlets": [ "PULSE" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
## Raw Capturers
According to the inlets you used in "event detectors" section, select the matching raw capture outlets.
### 1. GenericOpenVRInputCapturer
Get VR controller input.
```json=
{
      "Name": "GenericOpenVRInputCapturer",
      "Outlets": [ "OPENVR" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
### 2. GenericXboxInputCapturer
Get xbox controller input.
```json=
{
      "Name": "GenericXboxInputCapturer",
      "Outlets": [ "XINPUT" ],
      "Options": {
        "UseShellExecute": "False"
      }
}
```
