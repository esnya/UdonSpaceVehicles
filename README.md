# UdonSpaceVehicles

([日本語↓↓](#Japanese))

This is the Udon package for realizing a space plane with VRChat. Simulates orbital behavior such as 6-axis maneuvering and gravitational pull.

Inspired by [SaccFlightAndVehicles](https://github.com/Sacchan-VRC/SaccFlightAndVehicles.git)

## Requirements
* [UdonSharp](https://github.com/MerlinVR/UdonSharp/tree/v0.19.5) v0.19.5 or later
* [UdonToolKit](https://github.com/orels1/UdonToolkit/tree/develop) v1.0.0-rc or later
  * Download from **develop branch**. Do not from rerelases because it's not released yet.
  * Recommended to setup acording to [Installation](https://github.com/orels1/UdonToolkit/tree/develop#installation)

## Components
<!-- _USV_COMPONENTS_ -->
### ControllerInput
Simulates joystick (Pitch, Yaw, Roll) and slider (X, Y, Z) input.

### DamageManager
Manages vehicle damage.

### EngineDriver
Applies the power of the main engine and animates  The ControllerInput is required as a throttle.

### GlobalSettings
Put as a single GameObject named "_USV_Global_Settings_" to provide global setting values for other components.

### GravitySource
Adds an unprojected gravitational force to target objects. Time and length can be scaled.

### HUDTextDriver
Drives text for the instruments.

### KineticDriver
Add or set force or velocity when custom event "Trigger" received to attached Rigidbody.

### LaserGun
The gun.

### OrbitalMarkerDriver
Rotates the attached object according to the direction of the orbit.

### OrbitalObject
Simulates the gravitational force on an orbiting object.The xz plane will be projected as perpendicular to the orbital plane and y will be the altitude.

### ParticleDriver
Emits particle by custom event "Trigger".

### RCSController
Controls RCS (Reaction Control System) such as thrusters. Two Controller Inputs are required: joystick and throttle.

### SeatController
Controls the pilot seat. The vehicle is initially activated with this component.

### SyncManager
Provides synchronized values to USV components.

### ThrusterDriver
Applies thruster force to the vehicle, and animate them.

### UdonActivator
Activates and takes ownership activatable components. Targets can be retrieved from children and their children.

### UdonLogger
Formatted logger. Requires TMPro.TextMeshPro.

### VehicleRoot
Manages collisions, and respawns, vehicle power states. Attach to the root game object with a Rigidbody.
<!-- /_USV_COMPONENTS_ -->

# Japanese

VRChatで宇宙機を実現するためのUdonパッケージです。6軸機動、引力などの軌道上の挙動をシミュレートします。

## Requirements
* [UdonSharp](https://github.com/MerlinVR/UdonSharp/tree/v0.19.5) v0.19.5 以降
* [UdonToolKit](https://github.com/orels1/UdonToolkit/tree/develop) v1.0.0-rc 以降
  * **develop**ブランチからダウンロードしてください。 リリースはまだされていないのでGitHubにunitypackageはありません。
  * [Installation](https://github.com/orels1/UdonToolkit/tree/develop#installation)の設定をしておくことを推奨します。
