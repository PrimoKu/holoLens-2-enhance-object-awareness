# Enhance Human Object Awareness with HoloLens2 using Radar Map

## Overview
In this application, we repurpose HoloLens' built-in sensors to enhance object awareness. In order to do so, we not only unlocked the Research Mode of HoloLens 2 to access all four environmental gray-scale cameras and performed ArUco Marker detection functionality, but also developed a radar mini-map UI to display the detected objects of interest in real-time both for objects within and outside the users' FoV(field of view).

This Unity project shows 
- The utilization of HoloLens 2 Research Mode.
- The functionality of ArUco Marker detection.
- The proof of concept for increasing the object awareness by intuitive mini-map UI that shows the relative orientations and heights with respect to users.

## Compatibility
- Unity 2019.4*
- Visual Studio 2019

## Build
1. Open this folder in Unity.
2. Go to Build Settings, switch target platform to `Universal Windows Platform`, select `HoloLens` for target device, and `ARM64` as the target platform.
3. In the `Scenes in Build`, select `MainScene`.
4. Hopefully, there is no error in the console. Build the Unity project in a new folder (e.g. App folder).
5. To enable Research Mode capability,in yout build directory, open `App/XtraRadarEye/Package.appxmanifest` with a text editor. Add `xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"` before the IgnorableNamespaces in Package tag (line 2). Add `<rescap:Capability Name="perceptionSensorsExperimental" />` in the Capabilities tag between `<uap2:Capability ... >` and `<DeviceCapability ... >`.
6. Save the changes. Open `App/XtraRadarEye.sln`.
7. In Visual Studio 2019, change the configuration to `Release` and change the build platform to `ARM64`. Also select Remote Machine if you want to wirelessly deploy the application.
8. Go to `Project > Properties > Configuration Properties > Debugging > Machine Name`. Enter the IP address of your Hololens 2.
9. Then go to `Debug > Start Without Debugging` to deploy the application.
10. Done!

## Note
- The app may not function properly the first time you open the deployed app when there are pop-up windows asking for permissions. You can simply grant the permissions, close the app and reopen it. Then everything should be fine.
- You need to restart the device (hold the power button for several seconds) each time the device hiberates after you opened an app that uses research mode functions. So if your app suddenly cannot get any sensor data, try restarting your device. Please let me know if you know how to solve this issue.

## Result
<img src="/Demo/demo.gif" width="720">
