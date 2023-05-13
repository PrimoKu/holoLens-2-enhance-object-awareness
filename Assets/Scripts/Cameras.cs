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
    // public UnityEngine.UI.Text textLL, textLF, textRF, textRR;
    public Canvas canvas;
    public Camera MainCamera;
    private Vector3 mCameraPos;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsLL;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsLF;    
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsRF;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsRR;
    public Matrix4x4 TransformUnityWorld {get; set;}
    public Radar3D radar3D;
    public Arrows3D arrows3D;
    public EyeSee3D eyeSee3D;
    public VirtualButton buttons;
    public DataCollection dataCollect;
    public FrameCapture frameCapture;
    bool radar3DActive = false;
    bool arrows3DActive = false;
    bool eyeSee3DActive = false;

    public int NumArUcoMarkers = 4;
    private bool[] markerDetected; 
    private Vector3[] ArUcoPos;
    private Quaternion[] ArUcoRot;
    private Vector3[] ButtonPos;
    private Quaternion[] ButtonRot;
    private Matrix4x4 RotM_c;
    private Quaternion r_c, r_cc;

    void Start() {   

        radar3D.mapStart();
        radar3D.mapActivate(false);
        arrows3D.mapStart();
        arrows3D.mapActivate(false);
        eyeSee3D.mapStart();
        eyeSee3D.mapActivate(false);

        buttons.buttonStart();
        buttons.buttonsRenderDisabled();

        ArUcoPos = new Vector3[NumArUcoMarkers];
        ArUcoRot = new Quaternion[NumArUcoMarkers];
        ButtonPos = new Vector3[NumArUcoMarkers];
        ButtonRot = new Quaternion[NumArUcoMarkers];

        for (int i = 0; i < NumArUcoMarkers; i++) {
            ArUcoPos[i] = new Vector3(0.0f, 0.0f, 0.0f);
            ArUcoRot[i] = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
            ButtonPos[i] = new Vector3(0.0f, 0.0f, 0.0f);
            ButtonRot[i] = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        }

        Vector3 t = Vector3.zero;
        Vector3 s = Vector3.one;
        r_c = Quaternion.Euler(0, 0, 90);
        r_cc = Quaternion.Euler(0, 0, -90);
        RotM_c = Matrix4x4.TRS(t, r_c, s);

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
        // textLL.text = $"MainCamera Pos: {mCameraPos.x}, {mCameraPos.y}, {mCameraPos.z}";

#if ENABLE_WINMD_SUPPORT
        if(LLbitmap != null) {  DetectMarkers(LLbitmap, calibParamsLL, 0); }
        if(LFbitmap != null) {  DetectMarkers(LFbitmap, calibParamsLF, 1); }
        // if(RFbitmap != null) {  DetectMarkers(RFbitmap, calibParamsRF, 2); }
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

    public string MapActiveType() {
        string type = "NULL";

        if(radar3DActive) {
            type = "Radar";
        } else if (arrows3DActive) {
            type = "Arrow";
        } else if (eyeSee3DActive) {
            type = "EyeSee360";
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
        var detected_markers = CvUtils.DetectMarkers(softwareBitmap, calibParams);
        markerDetected = new bool[] {false, false, false, false, false, false, false, false, false, false};
        foreach (var det_marker in detected_markers) { markerDetected[det_marker.Id] = true; }

        List<int> falseIndexes = new List<int>();
        for (int i = 0; i < markerDetected.Length; i++) {
            if (!markerDetected[i]) {
                falseIndexes.Add(i);
            }
        }

        if (ArUcoOnCamID == 0) {
            radar3D.markersRenderDisabled();
            arrows3D.markersRenderDisabled();
            eyeSee3D.markersRenderDisabled();
            buttons.buttonsRenderDisabled();

            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                PlotMarkers(ArUcoOnCamID, id, det_marker.Position, det_marker.Rotation);
            }
        }
        else if (ArUcoOnCamID == 1) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                PlotMarkers(ArUcoOnCamID, id, det_marker.Position, det_marker.Rotation);
            }
        }
        else if (ArUcoOnCamID == 2) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                PlotMarkers(ArUcoOnCamID, id, det_marker.Position, det_marker.Rotation);
            }
        }
        else if (ArUcoOnCamID == 3) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                PlotMarkers(ArUcoOnCamID, id, det_marker.Position, det_marker.Rotation);
            }
        }
    }

    private void PlotMarkers(int CamId, int markerId, System.Numerics.Vector3 position, System.Numerics.Vector3 rotation) {
        ArUcoPos[markerId] = ArUcoUtils.Vec3FromFloat3(position);
        ArUcoRot[markerId] = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(rotation));
        ArUcoPos[markerId] = frameCapture.transformToLFCameraFrame(ArUcoPos[markerId], CamId);

        TransformUnityWorld = frameCapture.GetViewToUnityTransform(CamId, RotM_c * ArUcoPos[markerId], r_cc * ArUcoRot[markerId]);

        ButtonPos[markerId] = ArUcoUtils.GetVectorFromMatrix(TransformUnityWorld);
        ButtonRot[markerId] = ArUcoUtils.GetQuatFromMatrix(TransformUnityWorld);

        ArUcoPos[markerId].y = ArUcoPos[markerId].y * (-1);
        ArUcoPos[markerId].x = ArUcoPos[markerId].x * (-1);

        if(radar3DActive) {radar3D.plotMarkers(markerId, ArUcoPos[markerId]);}
        if(arrows3DActive) {arrows3D.arrowPoint(markerId, ButtonPos[markerId]);}
        if(eyeSee3DActive) {eyeSee3D.plotMarkers(markerId, ArUcoPos[markerId]);}
        buttons.plotButton(markerId, ButtonPos[markerId], ButtonRot[markerId]);
    }
#endif
}