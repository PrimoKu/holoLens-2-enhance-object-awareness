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
    
    public GameObject LLPreviewPlane = null;
    private Material LLMediaMaterial = null;
    private Texture2D LLMediaTexture = null;
    private byte[] LLFrameData = null;
    private Matrix4x4 LLCameraPose;

    public GameObject LFPreviewPlane = null;
    private Material LFMediaMaterial = null;
    private Texture2D LFMediaTexture = null;
    private byte[] LFFrameData = null;
    private Matrix4x4 LFCameraPose;

    public GameObject RFPreviewPlane = null;
    private Material RFMediaMaterial = null;
    private Texture2D RFMediaTexture = null;
    private byte[] RFFrameData = null;
    private Matrix4x4 RFCameraPose;

    public GameObject RRPreviewPlane = null;
    private Material RRMediaMaterial = null;
    private Texture2D RRMediaTexture = null;
    private byte[] RRFrameData = null;
    private Matrix4x4 RRCameraPose;

    private Matrix4x4 transformUnityCamera, transformUnityWorld;

    public Radar3D radar3D;
    public Arrows3D arrows3D;
    public EyeSee3D eyeSee3D;
    public VirtualButton buttons;
    public DataCollection dataCollect;

    // Dictionary<string, bool> mapsIsActive = new Dictionary<string, bool>() {{"Radar3D", false}, {"Arrows3D", false}};
    bool radar3DActive = false;
    bool arrows3DActive = false;
    bool eyeSee3DActive = false;
    

    public int NumArUcoMarkers = 10;
    private List<Vector3> ArUcoPos = new List<Vector3>();
    private List<Quaternion> ArUcoRot = new List<Quaternion>();

#if ENABLE_WINMD_SUPPORT
    Windows.Perception.Spatial.SpatialCoordinateSystem unityWorldOrigin;
#endif

    private void Awake() {
#if ENABLE_WINMD_SUPPORT
#if UNITY_2020_1_OR_NEWER // note: Unity 2021.2 and later not supported
        IntPtr WorldOriginPtr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;
        unityWorldOrigin = Marshal.GetObjectForIUnknown(WorldOriginPtr) as Windows.Perception.Spatial.SpatialCoordinateSystem;
        //unityWorldOrigin = Windows.Perception.Spatial.SpatialLocator.GetDefault().CreateStationaryFrameOfReferenceAtCurrentLocation().CoordinateSystem;
#else
        IntPtr WorldOriginPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
        unityWorldOrigin = Marshal.GetObjectForIUnknown(WorldOriginPtr) as Windows.Perception.Spatial.SpatialCoordinateSystem;
#endif
#endif
    }

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
        // canvas.transform.SetParent(MainCamera.transform);
        if (LLPreviewPlane != null) {
            LLMediaMaterial = LLPreviewPlane.GetComponent<MeshRenderer>().material;
            LLMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            LLPreviewPlane.transform.GetComponent<Renderer>().enabled = false;
        }

        if (LFPreviewPlane != null) {
            LFMediaMaterial = LFPreviewPlane.GetComponent<MeshRenderer>().material;
            LFMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            LFPreviewPlane.transform.GetComponent<Renderer>().enabled = false;
        }

        if (RFPreviewPlane != null) {
            RFMediaMaterial = RFPreviewPlane.GetComponent<MeshRenderer>().material;
            RFMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            RFPreviewPlane.transform.GetComponent<Renderer>().enabled = false;
        }

        if (RRPreviewPlane != null) {
            RRMediaMaterial = RRPreviewPlane.GetComponent<MeshRenderer>().material;
            RRMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            RRPreviewPlane.transform.GetComponent<Renderer>().enabled = false;
        }

        radar3D.mapStart();
        radar3D.mapActivate(false);
        arrows3D.mapStart();
        arrows3D.mapActivate(false);
        eyeSee3D.mapStart();
        eyeSee3D.mapActivate(false);

        buttons.buttonStart();

        for (int i = 0; i < NumArUcoMarkers; i++) {
            ArUcoPos.Add(new Vector3(0.0f, 0.0f, 0.0f));
            ArUcoRot.Add(new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
        }

#if ENABLE_WINMD_SUPPORT
        researchMode = new HL2ResearchMode();

        researchMode.InitializeSpatialCamerasFront();
        researchMode.InitializeSpatialCamerasSide();

        researchMode.SetReferenceCoordinateSystem(unityWorldOrigin);

        researchMode.StartSpatialCamerasFrontLoop();
        researchMode.StartSpatialCamerasSideLoop();

        LLCameraPose = floatArrayToUnityMatrix(researchMode.GetLLCameraPose());
        LFCameraPose = floatArrayToUnityMatrix(researchMode.GetLFCameraPose());
        RFCameraPose = floatArrayToUnityMatrix(researchMode.GetRFCameraPose());
        RRCameraPose = floatArrayToUnityMatrix(researchMode.GetRRCameraPose());
        
        try {
            CvUtils = new OpenCVRuntimeComponent.CvUtils(
                                ArUcoBoardPositions.ComputeMarkerSizeForTrackingType(ArUcoTrackingType, 
                                                                                     ArUcoBoardPositions.markerSizeForSingle,
                                                                                     ArUcoBoardPositions.markerSizeForBoard),
                                ArUcoBoardPositions.numMarkers, (int)ArUcoDictionaryName,
                                ArUcoBoardPositions.FillCustomObjectPointsFromUnity());
            // textRR.text = "cvutil finished";
            Debug.Log("cvutil finished");
        }
        catch (System.Exception e) {
            // textRR.text = "cvutil failed";
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
        
        // textLL.text = $"LLCameraPose[0]: {LLCameraPose.m00}, {LLCameraPose.m01}, {LLCameraPose.m02}, {LLCameraPose.m03}";
        // textLF.text = $"LLCameraPose[1]: {LLCameraPose.m10}, {LLCameraPose.m11}, {LLCameraPose.m12}, {LLCameraPose.m13}";
        // textRF.text = $"LLCameraPose[2]: {LLCameraPose.m20}, {LLCameraPose.m21}, {LLCameraPose.m22}, {LLCameraPose.m23}";
        // textRR.text = $"LLCameraPose[3]: {LLCameraPose.m30}, {LLCameraPose.m31}, {LLCameraPose.m32}, {LLCameraPose.m33}";

        // textLL.text = $"LFCameraPose[0]: {LFCameraPose.m00}, {LFCameraPose.m01}, {LFCameraPose.m02}, {LFCameraPose.m03}";
        // textLF.text = $"LFCameraPose[1]: {LFCameraPose.m10}, {LFCameraPose.m11}, {LFCameraPose.m12}, {LFCameraPose.m13}";
        // textRF.text = $"LFCameraPose[2]: {LFCameraPose.m20}, {LFCameraPose.m21}, {LFCameraPose.m22}, {LFCameraPose.m23}";
        // textRR.text = $"LFCameraPose[3]: {LFCameraPose.m30}, {LFCameraPose.m31}, {LFCameraPose.m32}, {LFCameraPose.m33}";

        // textLL.text = $"RFCameraPose[0]: {RFCameraPose.m00}, {RFCameraPose.m01}, {RFCameraPose.m02}, {RFCameraPose.m03}";
        // textLF.text = $"RFCameraPose[1]: {RFCameraPose.m10}, {RFCameraPose.m11}, {RFCameraPose.m12}, {RFCameraPose.m13}";
        // textRF.text = $"RFCameraPose[2]: {RFCameraPose.m20}, {RFCameraPose.m21}, {RFCameraPose.m22}, {RFCameraPose.m23}";
        // textRR.text = $"RFCameraPose[3]: {RFCameraPose.m30}, {RFCameraPose.m31}, {RFCameraPose.m32}, {RFCameraPose.m33}";

        // textLL.text = $"RRCameraPose[0]: {RRCameraPose.m00}, {RRCameraPose.m01}, {RRCameraPose.m02}, {RRCameraPose.m03}";
        // textLF.text = $"RRCameraPose[1]: {RRCameraPose.m10}, {RRCameraPose.m11}, {RRCameraPose.m12}, {RRCameraPose.m13}";
        // textRF.text = $"RRCameraPose[2]: {RRCameraPose.m20}, {RRCameraPose.m21}, {RRCameraPose.m22}, {RRCameraPose.m23}";
        // textRR.text = $"RRCameraPose[3]: {RRCameraPose.m30}, {RRCameraPose.m31}, {RRCameraPose.m32}, {RRCameraPose.m33}";
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
        // update LL camera texture
        if (startRealtimePreview && LLPreviewPlane != null && researchMode.LLImageUpdated()) {
            long ts;
            byte[] frameTexture = researchMode.GetLLCameraBuffer(out ts);
            if (frameTexture.Length > 0) {
                if (LLFrameData == null) {LLFrameData = frameTexture;}
                else {System.Buffer.BlockCopy(frameTexture, 0, LLFrameData, 0, LLFrameData.Length);}
                if (showRealtimeFeed) {
                    LLMediaMaterial.mainTexture = LLMediaTexture;   
                    LLMediaTexture.LoadRawTextureData(LLFrameData);
                    LLMediaTexture.Apply();   
                }
                else {LLMediaMaterial.mainTexture = null;}
                IBuffer buffer = LLFrameData.AsBuffer();
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                HandleArUcoTracking(bitmap, 0);
            }
        }
        // update LF camera texture
        if (startRealtimePreview && LFPreviewPlane != null && researchMode.LFImageUpdated()) {
            long ts;
            byte[] frameTexture = researchMode.GetLFCameraBuffer(out ts);
            if (frameTexture.Length > 0) {
                if (LFFrameData == null) {LFFrameData = frameTexture;}
                else {System.Buffer.BlockCopy(frameTexture, 0, LFFrameData, 0, LFFrameData.Length);}
                if (showRealtimeFeed) {
                    LFMediaMaterial.mainTexture = LFMediaTexture;   
                    LFMediaTexture.LoadRawTextureData(LFFrameData);
                    LFMediaTexture.Apply();   
                }
                else {LFMediaMaterial.mainTexture = null;}
                IBuffer buffer = LFFrameData.AsBuffer();
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                HandleArUcoTracking(bitmap, 1);
            }
        }
        // update RF camera texture
        if (startRealtimePreview && RFPreviewPlane != null && researchMode.RFImageUpdated()) {
            long ts;
            byte[] frameTexture = researchMode.GetRFCameraBuffer(out ts);
            if (frameTexture.Length > 0) {
                if (RFFrameData == null) {RFFrameData = frameTexture;}
                else {System.Buffer.BlockCopy(frameTexture, 0, RFFrameData, 0, RFFrameData.Length);}
                if (showRealtimeFeed) {
                    RFMediaMaterial.mainTexture = RFMediaTexture;   
                    RFMediaTexture.LoadRawTextureData(RFFrameData);
                    RFMediaTexture.Apply();   
                }
                else {RFMediaMaterial.mainTexture = null;}
                IBuffer buffer = RFFrameData.AsBuffer();
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                HandleArUcoTracking(bitmap, 2);
            }
        }
        // update RR camera texture
        if (startRealtimePreview && RRPreviewPlane != null && researchMode.RRImageUpdated()) {
            long ts;
            byte[] frameTexture = researchMode.GetRRCameraBuffer(out ts);
            if (frameTexture.Length > 0) {
                if (RRFrameData == null) {RRFrameData = frameTexture;}
                else {System.Buffer.BlockCopy(frameTexture, 0, RRFrameData, 0, RRFrameData.Length);}
                if (showRealtimeFeed) {
                    RRMediaMaterial.mainTexture = RRMediaTexture;   
                    RRMediaTexture.LoadRawTextureData(RRFrameData);
                    RRMediaTexture.Apply();   
                }
                else {RRMediaMaterial.mainTexture = null;}
                IBuffer buffer = RRFrameData.AsBuffer();
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                HandleArUcoTracking(bitmap, 3);
            }
        } 
#endif
    }

#region Button Event Functions
    public void ToggleFeedEvent() {
        showRealtimeFeed = !showRealtimeFeed;
        LFPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
        RFPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
        LLPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
        RRPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
    }

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
    private void HandleArUcoTracking(SoftwareBitmap bitmap, int ArUcoOnCamID) {
        if (bitmap != null) {
            //System.Threading.Thread.Sleep(1000);
            switch (ArUcoTrackingType) {
                case ArUcoUtils.ArUcoTrackingType.Markers:
                    //text.text = "start detect marker";
                    //System.Threading.Thread.Sleep(1000);
                    switch (CalibrationParameterType) {
                        // Cache from user-defined parameters 
                        case ArUcoUtils.CameraCalibrationParameterType.UserDefined:
                            //System.Threading.Thread.Sleep(1000);
                            if (ArUcoOnCamID == 0) { // LL
                                // System.Threading.Thread.Sleep(1000);
                                DetectMarkers(bitmap, calibParamsLL, ArUcoOnCamID)
                            }
                            else if (ArUcoOnCamID == 1) { // LF
                                // System.Threading.Thread.Sleep(1000);
                                DetectMarkers(bitmap, calibParamsLF, ArUcoOnCamID);
                            }
                            else if (ArUcoOnCamID == 2) { // RF
                                // System.Threading.Thread.Sleep(1000);
                                DetectMarkers(bitmap, calibParamsRF, ArUcoOnCamID);
                            }
                            else if (ArUcoOnCamID == 3) { // RR
                                // System.Threading.Thread.Sleep(1000);
                                DetectMarkers(bitmap, calibParamsRR, ArUcoOnCamID);
                            }
                            break;
                        default:
                            Debug.Log("user defined param not found");
                            break;
                    }

                case ArUcoUtils.ArUcoTrackingType.None:
                    //text.text = $"Not running tracking...";
                    break;

                default:
                    //text.text = $"No option selected for tracking...";
                    break;
            }
        }
        bitmap?.Dispose();
    }

    private void DetectMarkers(SoftwareBitmap softwareBitmap, OpenCVRuntimeComponent.CameraCalibrationParams calibParams, int ArUcoOnCamID) {
        // Get marker detections from opencv component
        var detected_markers = CvUtils.DetectMarkers(softwareBitmap, calibParams);
        Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion rot = new Quaternion(0.0f, 0.0f, 0.0f, 0.0f);
        if (ArUcoOnCamID == 0)
        {
            radar3D.markersRenderDisabled();
            arrows3D.markersRenderDisabled();
            eyeSee3D.markersRenderDisabled();
            buttons.buttonsRenderDisabled();

            foreach (var det_marker in detected_markers)
            {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // pos.x = pos.x - 0.1f;
                // pos.y = pos.y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {pos.y}, {pos.x}, {pos.z}";
                rot = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));
                
                transformUnityCamera = ArUcoUtils.TransformInUnitySpace(pos, rot);
                transformUnityWorld = LLCameraPose * transformUnityCamera;

                pos.y -= 1.1f;
                // pos.y -= 0.3f;   
                // textLF.text = $"ArUco Pos: {pos}, Rot: {rot}";
                // textRF.text = $"Transform Pos: {ArUcoUtils.GetVectorFromMatrix(transformUnityWorld)}, Rot: {ArUcoUtils.GetQuatFromMatrix(transformUnityWorld)}";       
                if(radar3DActive) {radar3D.plotMarkers(id, pos);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, pos);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, pos);}
                buttons.plotButton(id, mCameraPos, pos, ArUcoUtils.GetQuatFromMatrix(transformUnityWorld));
            }
        }
        else if (ArUcoOnCamID == 1) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // pos.x = pos.x - 0.1f;
                // pos.y = pos.y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {pos.y}, {pos.x}, {pos.z}";
                rot = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));

                transformUnityCamera = ArUcoUtils.TransformInUnitySpace(pos, rot);
                transformUnityWorld = LFCameraPose * transformUnityCamera;
                pos.y = pos.y * (-1);
                pos.x = pos.x * (-1);

                pos.y -= 0.02f;
                // textLF.text = $"ArUco Pos: {pos}, Rot: {rot}";
                // textRF.text = $"Transform Pos: {ArUcoUtils.GetVectorFromMatrix(transformUnityWorld)}, Rot: {ArUcoUtils.GetQuatFromMatrix(transformUnityWorld)}"; 
                if(radar3DActive) {radar3D.plotMarkers(id, pos);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, pos);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, pos);}
                buttons.plotButton(id, mCameraPos, pos, ArUcoUtils.GetQuatFromMatrix(transformUnityWorld));
            }
        }
        else if (ArUcoOnCamID == 2) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // pos.x = pos.x - 0.1f;
                // pos.y = pos.y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {pos.y}, {pos.x}, {pos.z}";
                rot = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));

                transformUnityCamera = ArUcoUtils.TransformInUnitySpace(pos, rot);
                transformUnityWorld = RFCameraPose * transformUnityCamera;

                // pos.y += 0.15f;
                // textLF.text = $"ArUco Pos: {pos}, Rot: {rot}";
                // textRF.text = $"Transform Pos: {ArUcoUtils.GetVectorFromMatrix(transformUnityWorld)}, Rot: {ArUcoUtils.GetQuatFromMatrix(transformUnityWorld)}"; 
                if(radar3DActive) {radar3D.plotMarkers(id, pos);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, pos);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, pos);}
                buttons.plotButton(id, mCameraPos, pos, ArUcoUtils.GetQuatFromMatrix(transformUnityWorld));
            }
        }
        else if (ArUcoOnCamID == 3) {
            foreach (var det_marker in detected_markers) {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                // pos.x = pos.x - 0.1f;
                // pos.y = pos.y + 0.1f;
                textLF.text = $"Orig ArUco Pos: {pos.y}, {pos.x}, {pos.z}";
                rot = ArUcoUtils.RotationQuatFromRodrigues(ArUcoUtils.Vec3FromFloat3(det_marker.Rotation));

                transformUnityCamera = ArUcoUtils.TransformInUnitySpace(pos, rot);
                transformUnityWorld = RRCameraPose * transformUnityCamera;

                pos.y = pos.y * (-1);
                pos.x = pos.x * (-1);
                pos.y += 0.95f;
                // textLF.text = $"ArUco Pos: {pos}, Rot: {rot}";
                // textRF.text = $"Transform Pos: {ArUcoUtils.GetVectorFromMatrix(transformUnityWorld)}, Rot: {ArUcoUtils.GetQuatFromMatrix(transformUnityWorld)}";  
                if(radar3DActive) {radar3D.plotMarkers(id, pos);}
                if(arrows3DActive) {arrows3D.arrowPoint(id, pos);}
                if(eyeSee3DActive) {eyeSee3D.plotMarkers(id, pos);}
                buttons.plotButton(id, mCameraPos, pos, ArUcoUtils.GetQuatFromMatrix(transformUnityWorld));
            }
        }
    }
#endif
}