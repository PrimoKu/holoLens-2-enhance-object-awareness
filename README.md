# Repurposing HMD’s Built-in Sensors to Increase User Awareness
This project aims to explore the effectiveness of different visualization techniques for ArUco marker detection in a HoloLens 2 augmented reality environment. We developed a HoloLens app using Unity 3D and utilized the HoloLens 2 Research Mode API and OpenCV library to implement three different visualization methods: 3D radar, 3D arrows, and EyeSee360.

## Overview 
The application is a augmented reality tool that utilizes the Microsoft HoloLens 2 headset to enhance the user's ability to navigate and interact with their environment. The application uses the HoloLens 2's Research Mode API to access data from the device's four visible light cameras, which is then processed using the OpenCV computer vision library to detect and track ArUco markers in the user's surroundings. <br />
To provide the user with a more intuitive and engaging way to interact with the detected markers, we have designed and implemented three different visualization techniques: a 3D radar, a 3D arrow, and EyeSee360. Each of these visualization techniques provides a unique perspective on the location and orientation of the markers, and allows the user to quickly and accurately identify and interact with them. <br />

This project showcases the following features:
- Utilization of HoloLens 2 Research Mode for obtaining information from the device's visible light cameras.
- Functionality of ArUco Marker detection through OpenCV.
- Proof of concept for increasing object awareness through a holographic spatial map that displays relative orientations and heights with respect to the user.

Each of these features was implemented in Unity 3D using C# scripts and required the integration of various technologies. The HoloLens 2 Research Mode API and OpenCV were written in C++, and thus wrappers were created to allow for integration with Unity. The combination of these technologies allows for the creation of a aumented reality application with increased object awareness and detection capabilities.

## Compatibility
- Unity 2019.4*
- Visual Studio 2019

## Structure

    ├── Assets                                      
    │   ├── Scripts                                 # Folder for all the scripts
    │   │   ├── Radar3D.cs                          # Class for 3D Radar
    │   │   ├── Arrows3D.cs                         # Class for 3D Arrows
    │   │   ├── EyeSee360.cs                        # Class for EyeSee360
    │   │   ├── VirtualButtons.cs                   # Class for control the behavior of virtual buttons
    │   │   ├── Cameras.cs                          # Mainly utilizing OpenCV to perform ArUco marker detect functionality
    │   │   ├── FrameCapture.cs                     # Utilizing HL2 Research Mode to obtain raw streams for all VLC cameras 
    │   │   ├── DataCollection.cs                   # Script for performing object searching task and recording the data
    │   │   ├── ArUcoUtils.cs                       # Functions for perform calculations with OpenCV
    │   │   └── ...
    │   ├── Plugins
    │   │   ├── OpenCvRuntimeComponent.winmd        # OpenCV Runtime Wrapper
    │   │   ├── HL2UnityPlugin.dll                  # HL2 Research Mode Wrapper
    │   │   └── ...
    │   └── ...
    └── ...

## Build
1. Open this folder in Unity.
2. Go to Build Settings, switch target platform to `Universal Windows Platform`, select `HoloLens` for target device, and `ARM64` as the target platform.
3. In the `Scenes in Build`, select `MainScene`.
4. Hopefully, there is no error in the console. Build the Unity project in a new folder (e.g. App folder).
5. To enable Research Mode capability,in yout build directory, open `App/AwareceptionAR/Package.appxmanifest` with a text editor. 
    1. Add `xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"` before the IgnorableNamespaces in Package tag (line 2). 
    2. Add `<rescap:Capability Name="perceptionSensorsExperimental" />` in the Capabilities tag between `<uap2:Capability ... >` and `<DeviceCapability ... >`.
6. Save the changes. Open `App/AwareceptionAR.sln`.
7. In Visual Studio 2019, change the configuration to `Release` and change the build platform to `ARM64`. 
    1. If you are deploying the app with cable plugged, select `Device` with a green start icon next the `ARM64`.
    2. If you want to deploy the app wirelessly, choose `Remote Machine` for the selection. Then, go to `Project > Properties > Configuration Properties > Debugging > Machine Name`, and enter the IP address of your Hololens 2.
8. Then go to `Debug > Start Without Debugging` to deploy the application.
9. Done!

## Note
- The app may not function properly the first time you open the deployed app when there are pop-up windows asking for permissions. You can simply grant the permissions, close the app and reopen it. Then everything should be fine.
- You need to restart the device (hold the power button for several seconds) each time the device hiberates after you opened an app that uses research mode functions. So if your app suddenly cannot get any sensor data, try restarting your device. Please let me know if you know how to solve this issue.

## Demo
<img src="/Demo/DataCollectVideo.gif" width="720">
