using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Input;
using System.Runtime.InteropServices.WindowsRuntime;

#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
using Windows.Graphics.Imaging;
using Windows.Perception.Spatial;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#endif

public class Cameras : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
    OpenCVRuntimeComponent.CvUtils CvUtils;
    OpenCVRuntimeComponent.CameraCalibrationParams calibParamsLL;
    OpenCVRuntimeComponent.CameraCalibrationParams calibParamsLF;
    OpenCVRuntimeComponent.CameraCalibrationParams calibParamsRF;
    OpenCVRuntimeComponent.CameraCalibrationParams calibParamsRR;
#endif

    public ArUcoUtils.ArUcoDictionaryName ArUcoDictionaryName = ArUcoUtils.ArUcoDictionaryName.DICT_6X6_50;
    public ArUcoUtils.ArUcoTrackingType ArUcoTrackingType = ArUcoUtils.ArUcoTrackingType.Markers;
    public ArUcoUtils.CameraCalibrationParameterType CalibrationParameterType = ArUcoUtils.CameraCalibrationParameterType.UserDefined;
    public ArUcoBoardPositions ArUcoBoardPositions;
    public UnityEngine.UI.Text textLL, textLF, textRF, textRR;
    public Canvas canvas;
    public Camera MainCamera;
    private Vector3 mCameraPos;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsLL;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsLF;    
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsRF;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsRR;

    // private Matrix4x4 transformUnityCamera, transformUnityWorld;
    public Radar3D radar3D;
    public Arrows3D arrows3D;
    public EyeSee3D eyeSee3D;
    public VirtualButton buttons;
    public DataCollection dataCollect;
    public FrameCapture frameCapture;

    // Dictionary<string, bool> mapsIsActive = new Dictionary<string, bool>() {{"Radar3D", false}, {"Arrows3D", false}};
    bool radar3DActive = false;
    bool arrows3DActive = false;
    bool eyeSee3DActive = false;
    

    public int NumArUcoMarkers = 10;
    private Vector3[] ArUcoPos = new Vector3[10];
    private Quaternion[] ArUcoRot = new Quaternion[10];

    private static Matrix4x4 floatArrayToUnityMatrix(float[] cameraPose) {
        return new Matrix4x4()
        {
            m00 = cameraPose[0],  m10 = cameraPose[1],  m20 = cameraPose[2],  m30 = cameraPose[3],
            m01 = cameraPose[4],  m11 = cameraPose[5],  m21 = cameraPose[6],  m31 = cameraPose[7],
            m02 = cameraPose[8],  m12 = cameraPose[9],  m22 = cameraPose[10], m32 = cameraPose[11],
            m03 = cameraPose[12], m13 = cameraPose[13], m23 = cameraPose[14], m33 = cameraPose[15],
        };
    }

    void Start() {   

        radar3D.mapStart();
        radar3D.mapActivate(false);
        arrows3D.mapStart();
        arrows3D.mapActivate(false);
        eyeSee3D.mapStart();
        eyeSee3D.mapActivate(false);

        buttons.buttonStart();

        for (int i = 0; i < NumArUcoMarkers; i++) {
            ArUcoPos[i] = new Vector3(0.0f, 0.0f, 0.0f);
            ArUcoRot[i] = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        }

#if ENABLE_WINMD_SUPPORT
        
        try {
            CvUtils = new OpenCVRuntimeComponent.CvUtils(
                                ArUcoBoardPositions.ComputeMarkerSizeForTrackingType(ArUcoTrackingType, 
                                                                                     ArUcoBoardPositions.markerSizeForSingle,
                                                                                     ArUcoBoardPositions.markerSizeForBoard),
                                ArUcoBoardPositions.numMarkers, (int)ArUcoDictionaryName,
                                ArUcoBoardPositions.FillCustomObjectPointsFromUnity());
            Debug.Log("cvutil finished");
        }
        catch (System.Exception e) {
        }

        // Create calibration parameters for all cameras
        calibParamsLL = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsLL.focalLength.x, UserDefinedCalibParamsLL.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsLL.principalPoint.x, UserDefinedCalibParamsLL.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsLL.radialDistortion.x, UserDefinedCalibParamsLL.radialDistortion.y, UserDefinedCalibParamsLL.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsLL.tangentialDistortion.x, UserDefinedCalibParamsLL.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsLL.imageWidth, // Image width
                            UserDefinedCalibParamsLL.imageHeight); // Image height
        calibParamsLF = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsLF.focalLength.x, UserDefinedCalibParamsLF.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsLF.principalPoint.x, UserDefinedCalibParamsLF.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsLF.radialDistortion.x, UserDefinedCalibParamsLF.radialDistortion.y, UserDefinedCalibParamsLF.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsLF.tangentialDistortion.x, UserDefinedCalibParamsLF.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsLF.imageWidth, // Image width
                            UserDefinedCalibParamsLF.imageHeight); // Image height
        calibParamsRF = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsRF.focalLength.x, UserDefinedCalibParamsRF.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsRF.principalPoint.x, UserDefinedCalibParamsRF.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsRF.radialDistortion.x, UserDefinedCalibParamsRF.radialDistortion.y, UserDefinedCalibParamsRF.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsRF.tangentialDistortion.x, UserDefinedCalibParamsRF.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsRF.imageWidth, // Image width
                            UserDefinedCalibParamsRF.imageHeight); // Image height
        calibParamsRR = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsRR.focalLength.x, UserDefinedCalibParamsRR.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsRR.principalPoint.x, UserDefinedCalibParamsRR.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsRR.radialDistortion.x, UserDefinedCalibParamsRR.radialDistortion.y, UserDefinedCalibParamsRR.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsRR.tangentialDistortion.x, UserDefinedCalibParamsRR.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsRR.imageWidth, // Image width
                            UserDefinedCalibParamsRR.imageHeight); // Image height
#endif
    }

    bool showRealtimeFeed = false;
    bool startRealtimePreview = true;

    void LateUpdate() {
        mCameraPos = MainCamera.transform.position;
        textLL.text = $"MainCamera Pos: {mCameraPos.x}, {mCameraPos.y}, {mCameraPos.z}";
        // Vector3[] test = {new Vector3(0.0f, 0.0f, 1.0f), new Vector3(2.0f, 1.0f, 2.0f), new Vector3(1.0f, -2.0f, 2.0f), new Vector3(-1.0f, -2.0f, 2.0f)};
        // radar3D.plotMarkers(0, test[0]);
        // radar3D.plotMarkers(1, test[1]);
        // radar3D.plotMarkers(2, test[2]);
        // radar3D.plotMarkers(3, test[3]);

#if ENABLE_WINMD_SUPPORT
        if(LLbitmap != null) {  DetectMarkers(LLbitmap, calibParamsLL, 0); }
        // if(LFbitmap != null) {  DetectMarkers(LFbitmap, calibParamsLF, 1); }
        if(RFbitmap != null) {  DetectMarkers(RFbitmap, calibParamsRF, 2); }
        if(RRbitmap != null) {  DetectMarkers(RRbitmap, calibParamsRR, 3); }
#endif
    }

#region Button Event Functions

    bool renderPointCloud = true;

    public void StopSensorsEvent() {
#if ENABLE_WINMD_SUPPORT
        researchMode.StopAllSensorDevice();
#endif
        startRealtimePreview = false;
    }

    public void Radar3DActiveEvent() {
        radar3DActive = !radar3DActive;
        if(radar3DActive) {
            arrows3DActive = false;
            arrows3D.mapActivate(arrows3DActive);
            eyeSee3DActive = false;
            eyeSee3D.mapActivate(eyeSee3DActive);
        }
        radar3D.mapActivate(radar3DActive);
    }
    
    public void Arrows3DActiveEvent() {
        arrows3DActive = !arrows3DActive;
        if(arrows3DActive) {
            radar3DActive = false;
            radar3D.mapActivate(radar3DActive);
            eyeSee3DActive = false;
            eyeSee3D.mapActivate(eyeSee3DActive);
        }
        arrows3D.mapActivate(arrows3DActive);
    }

    public void EyeSee3DActiveEvent() {
        eyeSee3DActive = !eyeSee3DActive;
        if(eyeSee3DActive) {
            radar3DActive = false;
            radar3D.mapActivate(radar3DActive);
            arrows3DActive = false;
            arrows3D.mapActivate(arrows3DActive);
        }
        eyeSee3D.mapActivate(eyeSee3DActive);
    }
    public void DataCollectStartEvent() {
        // dataCollect.dataCollectionStart();
        StartCoroutine(dataCollect.dataCollectionStart());
    }
#endregion
    
    public bool Radar3DIsActive() {
        return radar3DActive;
    }
    public bool Arrows3DIsActive() {
        return arrows3DActive;
    }
    public bool EyeSee3DIsActive() {
        return eyeSee3DActive;
    }

    public int MapActiveType() {
        int type = 0;

        if(radar3DActive) {
            type = 1;
        } else if (arrows3DActive) {
            type = 2;
        } else if (eyeSee3DActive) {
            type = 3;
        }

        return type;
    }

    private void OnApplicationFocus(bool focus) {
        if (!focus) StopSensorsEvent();
    }

#if ENABLE_WINMD_SUPPORT
    private SoftwareBitmap LLbitmap, LFbitmap, RFbitmap, RRbitmap;

    public void SetFrameBitmap(SoftwareBitmap bitmap, int frameId) {
        
        switch(frameId) {
            case 0:
                LLbitmap = bitmap;
                break;
            case 1:
                LFbitmap = bitmap;
                break;
            case 2:
                RFbitmap = bitmap;
                break;
            case 3:
                RRbitmap = bitmap;
                break;
            default:
                break;
        }

    }
#endif

#if WINDOWS_UWP
    private long GetCurrentTimestampUnix() {
        // Get the current time, in order to create a PerceptionTimestamp. 
        Windows.Globalization.Calendar c = new Windows.Globalization.Calendar();
        Windows.Perception.PerceptionTimestamp ts = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(c.GetDateTime());
        return ts.TargetTime.ToUnixTimeMilliseconds();
        //return ts.SystemRelativeTargetTime.Ticks;
    }
    private Windows.Perception.PerceptionTimestamp GetCurrentTimestamp() {
        // Get the current time, in order to create a PerceptionTimestamp. 
        Windows.Globalization.Calendar c = new Windows.Globalization.Calendar();
        return Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(c.GetDateTime());
    }
#endif

#if ENABLE_WINMD_SUPPORT

    private void DetectMarkers(SoftwareBitmap softwareBitmap, OpenCVRuntimeComponent.CameraCalibrationParams calibParams, int ArUcoOnCamID) {
        // Get marker detections from opencv component
        var detected_markers = CvUtils.DetectMarkers(softwareBitmap, calibParams);
        // Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion rot = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        if (ArUcoOnCamID == 0) {
            radar3D.markersRenderDisabled();
            arrows3D.markersRenderDisabled();
            eyeSee3D.markersRenderDisabled();
            buttons.buttonsRenderDisabled();

            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                ArUcoPos[id] = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // ArUcoPos[id].x = ArUcoPos[id].x - 0.1f;
                // ArUcoPos[id].y = ArUcoPos[id].y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {ArUcoPos[id].y}, {ArUcoPos[id].x}, {ArUcoPos[id].z}";
                ArUcoRot[id] = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));
                ArUcoPos[id] = frameCapture.transformToLFCameraFrame(ArUcoPos[id], ArUcoOnCamID);
                ArUcoPos[id].y = ArUcoPos[id].y * (-1);
                ArUcoPos[id].x = ArUcoPos[id].x * (-1);
                // transformUnityCamera = ArUcoUtils.TransformInUnitySpace(ArUcoPos[id], ArUcoRot[id]);
                // transformUnityWorld = LLCameraPose * transformUnityCamera;

                // ArUcoPos[id].y -= 1.1f;
                // ArUcoPos[id].y -= 0.3f;         
                if(radar3DActive) {radar3D.plotMarkers(id, ArUcoPos[id]);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, ArUcoPos[id]);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, ArUcoPos[id]);}
                // buttons.plotButton(id, mCameraPos, ArUcoPos[id], ArUcoUtils.GetQuatFromMatrix(transformUnityWorld));
                buttons.plotButton(id, mCameraPos, ArUcoPos[id], rot);
            }
        }
        else if (ArUcoOnCamID == 1) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                ArUcoPos[id] = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // ArUcoPos[id].x = ArUcoPos[id].x - 0.1f;
                // ArUcoPos[id].y = ArUcoPos[id].y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {ArUcoPos[id].y}, {ArUcoPos[id].x}, {ArUcoPos[id].z}";
                ArUcoRot[id] = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));
                ArUcoPos[id] = frameCapture.transformToLFCameraFrame(ArUcoPos[id], ArUcoOnCamID);
                ArUcoPos[id].y = ArUcoPos[id].y * (-1);
                ArUcoPos[id].x = ArUcoPos[id].x * (-1);
                // transformUnityCamera = ArUcoUtils.TransformInUnitySpace(ArUcoPos[id], ArUcoRot[id]);
                // transformUnityWorld = LFCameraPose * transformUnityCamera;

                // ArUcoPos[id].y -= 0.02f;
                if(radar3DActive) {radar3D.plotMarkers(id, ArUcoPos[id]);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, ArUcoPos[id]);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, ArUcoPos[id]);}
                buttons.plotButton(id, mCameraPos, ArUcoPos[id], rot);
            }
        }
        else if (ArUcoOnCamID == 2) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                ArUcoPos[id] = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // ArUcoPos[id].x = ArUcoPos[id].x - 0.1f;
                // ArUcoPos[id].y = ArUcoPos[id].y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {ArUcoPos[id].y}, {ArUcoPos[id].x}, {ArUcoPos[id].z}";
                ArUcoRot[id] = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));
                ArUcoPos[id] = frameCapture.transformToLFCameraFrame(ArUcoPos[id], ArUcoOnCamID);
                ArUcoPos[id].y = ArUcoPos[id].y * (-1);
                ArUcoPos[id].x = ArUcoPos[id].x * (-1);
                // transformUnityCamera = ArUcoUtils.TransformInUnitySpace(ArUcoPos[id], ArUcoRot[id]);
                // transformUnityWorld = RFCameraPose * transformUnityCamera;

                // ArUcoPos[id].y += 0.15f; 
                if(radar3DActive) {radar3D.plotMarkers(id, ArUcoPos[id]);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, ArUcoPos[id]);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, ArUcoPos[id]);}
                buttons.plotButton(id, mCameraPos, ArUcoPos[id], rot);
            }
        }
        else if (ArUcoOnCamID == 3) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                ArUcoPos[id] = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // ArUcoPos[id].x = ArUcoPos[id].x - 0.1f;
                // ArUcoPos[id].y = ArUcoPos[id].y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {ArUcoPos[id].y}, {ArUcoPos[id].x}, {ArUcoPos[id].z}";
                ArUcoRot[id] = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));
                ArUcoPos[id] = frameCapture.transformToLFCameraFrame(ArUcoPos[id], ArUcoOnCamID);
                ArUcoPos[id].y = ArUcoPos[id].y * (-1);
                ArUcoPos[id].x = ArUcoPos[id].x * (-1);
                // transformUnityCamera = ArUcoUtils.TransformInUnitySpace(ArUcoPos[id], ArUcoRot[id]);
                // transformUnityWorld = RRCameraPose * transformUnityCamera;

                // ArUcoPos[id].y += 0.95f; 
                if(radar3DActive) {radar3D.plotMarkers(id, ArUcoPos[id]);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, ArUcoPos[id]);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, ArUcoPos[id]);}
                buttons.plotButton(id, mCameraPos, ArUcoPos[id], rot);
            }
        }
    }
#endif
}