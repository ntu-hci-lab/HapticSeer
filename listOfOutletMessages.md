# Type of Outlet Messages
## Event Detectors
**Most of the time, you only have to worry about messages IN THIS SECTION**
### ACCELX, ACCELY
```
"Accel(Double)"
```
#### Attributes
* Refresh rate: 60Hz
* Axis: 
    * X: Right
    * Y: Front
* Range:
    * X: -20~20
    * Y: -5~5
### FIRE
```
"FIRE(Const String)"
```
#### Attributes
* Refresh rate: 100Hz (Pulse On), 60Hz (Pulse Off) 
### HURT
```
"BloodLoss(Double)"
```
#### Attributes
* Refresh rate: 10Hz
### IMCOMING
```
"BloodLoss(Double)|HitAngle(Double)"
```
#### Attributes
* Refresh rate: 60Hz
* Range: 0~360
---
## Feature Extractors
### BULLET
```
"BulletInGun(Int32)"
```
#### Attributes
* Refresh Rate: 60Hz
### BLOOD
```
"HPState(Double)"
```
#### Attributes
* Refresh Rate: 10Hz (HLA) 60Hz (BF1)
### HIT
```
"HitAngle(Double)"
```
#### Attributes
* Refresh rate: 60Hz
### SPEED
```
"Speed(Int32)"
```
#### Attributes
* Refresh rate: 60Hz
### PULSE
```
"MonoDetected(Bool)|LFEDetected(Bool)|Angle(Double)"
```
#### Attributes
* Refresh rate: 100Hz
---

## Raw Capturer
### OPENVR
```
"SourceTypeName(String)|EventType(String)|EventName(String)|StateInfo(Any)"
```
#### Attributes
* Refresh rate: 1000Hz
### XINPUT
```
"SourceEvent(String)|EventInfo(Any)"
```
#### Attributes
* Refresh rate: 1000Hz
