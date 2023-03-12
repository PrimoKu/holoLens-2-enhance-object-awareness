using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

#endif
    public ArUcoUtils.ArUcoDictionaryName ArUcoDictionaryName = ArUcoUtils.ArUcoDictionaryName.DICT_6X6_50;
    public ArUcoUtils.ArUcoTrackingType ArUcoTrackingType = ArUcoUtils.ArUcoTrackingType.Markers;
    public ArUcoUtils.CameraCalibrationParameterType CalibrationParameterType = ArUcoUtils.CameraCalibrationParameterType.UserDefined;
    public ArUcoBoardPositions ArUcoBoardPositions;
    public UnityEngine.UI.Text textRR, textRF, textLF, textLL;
    public Canvas canvas;
    public Camera MainCamera;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsRR;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsRF;    
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsLF;
    public UserDefinedCameraCalibrationParams UserDefinedCalibParamsLL;
    public GameObject LFPreviewPlane = null;
    private Material LFMediaMaterial = null;
    private Texture2D LFMediaTexture = null;
    // private byte[] LFFrameData = null;

    public GameObject RFPreviewPlane = null;
    private Material RFMediaMaterial = null;
    private Texture2D RFMediaTexture = null;
    // private byte[] RFFrameData = null;

    public GameObject LLPreviewPlane = null;
    private Material LLMediaMaterial = null;
    private Texture2D LLMediaTexture = null;
    // private byte[] LLFrameData = null;

    public GameObject RRPreviewPlane = null;
    private Material RRMediaMaterial = null;
    private Texture2D RRMediaTexture = null;
    // private byte[] RRFrameData = null;

    public Radar3D radar3D;
    public Arrows3D arrows3D;

    // Dictionary<string, bool> mapsIsActive = new Dictionary<string, bool>() {{"Radar3D", false}, {"Arrows3D", false}};
    bool radar3DActive = false;
    bool arrows3DActive = false;

#if ENABLE_WINMD_SUPPORT
    Windows.Perception.Spatial.SpatialCoordinateSystem unityWorldOrigin;
#endif

    private void Awake()
    {
#if ENABLE_WINMD_SUPPORT
#if UNITY_2020_1_OR_NEWER // note: Unity 2021.2 and later not supported
        IntPtr WorldOriginPtr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;
        unityWorldOrigin = Marshal.GetObIjectForUnknown(WorldOriginPtr) as Windows.Perception.Spatial.SpatialCoordinateSystem;
        //unityWorldOrigin = Windows.Perception.Spatial.SpatialLocator.GetDefault().CreateStationaryFrameOfReferenceAtCurrentLocation().CoordinateSystem;
#else
        IntPtr WorldOriginPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
        unityWorldOrigin = Marshal.GetObjectForIUnknown(WorldOriginPtr) as Windows.Perception.Spatial.SpatialCoordinateSystem;
#endif
#endif
    }
    void Start()
    {   
        // canvas.transform.SetParent(MainCamera.transform);
        if (LFPreviewPlane != null)
        {
            LFMediaMaterial = LFPreviewPlane.GetComponent<MeshRenderer>().material;
            LFMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            //LFMediaMaterial.mainTexture = LFMediaTexture;
        }

        if (RFPreviewPlane != null)
        {
            RFMediaMaterial = RFPreviewPlane.GetComponent<MeshRenderer>().material;
            RFMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            //RFMediaMaterial.mainTexture = RFMediaTexture;
        }

        if (LLPreviewPlane != null)
        {
            LLMediaMaterial = LLPreviewPlane.GetComponent<MeshRenderer>().material;
            LLMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            //LLMediaMaterial.mainTexture = LLMediaTexture;
        }

        if (RRPreviewPlane != null)
        {
            RRMediaMaterial = RRPreviewPlane.GetComponent<MeshRenderer>().material;
            RRMediaTexture = new Texture2D(640, 480, TextureFormat.Alpha8, false);
            //RRMediaMaterial.mainTexture = RRMediaTexture;
        }

        radar3D.mapStart();
        radar3D.mapActivate(false);
        arrows3D.mapStart();
        // mapsIsActive["Arrows3D"] = true;
        arrows3D.mapActivate(true);

#if ENABLE_WINMD_SUPPORT
        researchMode = new HL2ResearchMode();

        researchMode.InitializeSpatialCamerasFront();
        researchMode.InitializeSpatialCamerasSide();

        researchMode.SetReferenceCoordinateSystem(unityWorldOrigin);

        researchMode.StartSpatialCamerasFrontLoop();
        researchMode.StartSpatialCamerasSideLoop();

    try
    {
        CvUtils = new OpenCVRuntimeComponent.CvUtils(
                    ArUcoBoardPositions.ComputeMarkerSizeForTrackingType(
                        ArUcoTrackingType, 
                        ArUcoBoardPositions.markerSizeForSingle,
                        ArUcoBoardPositions.markerSizeForBoard),
                    ArUcoBoardPositions.numMarkers,
                    (int)ArUcoDictionaryName,
                    ArUcoBoardPositions.FillCustomObjectPointsFromUnity());
        // textRR.text = "cvutil finished";
        Debug.Log("cvutil finished");
    }
    catch (System.Exception e) 
    {
        // textRR.text = "cvutil failed";
    }
#endif
    }

    bool showRealtimeFeed = false;
    bool startRealtimePreview = true;

    void LateUpdate()
    {
        radar3D.markersRenderDisabled();
        arrows3D.markersRenderDisabled();
        
        Vector3[] test = {new Vector3(5.0f, 1.0f, 2.0f), new Vector3(-1.0f, -1.5f, 1.2f), new Vector3(1.0f, 0.0f, 2.0f), new Vector3(0.0f, -2.0f, 2.0f)};
        radar3D.plotMarkers(0, test[0]);
        radar3D.plotMarkers(1, test[1]);
        radar3D.plotMarkers(2, test[2]);

        arrows3D.arrowPoint(0, test[0]);
        arrows3D.arrowPoint(1, test[1]);
        arrows3D.arrowPoint(2, test[2]);

// chaging feed
#if ENABLE_WINMD_SUPPORT
        if (LLPreviewPlane != null && researchMode.LLImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetLLCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (LLFrameData == null)
                {
                    LLFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, LLFrameData, 0, LLFrameData.Length);
                }

                LLMediaTexture.LoadRawTextureData(LLFrameData);
                LLMediaTexture.Apply();
                IBuffer buffer = LLFrameData.AsBuffer();
                Debug.Log($"buffer length: {buffer.Length}");
                //text.text = "Build bit map";
                //SoftwareBitmap bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 640, 480);
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                Debug.Log("Start handle track");
                HandleArUcoTracking(bitmap, 0);
                //text.text = "end handle track";
            }
        }
        if (LFPreviewPlane != null && researchMode.LFImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetLFCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (LFFrameData == null)
                {
                    LFFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, LFFrameData, 0, LFFrameData.Length);
                }

                LFMediaTexture.LoadRawTextureData(LFFrameData);
                LFMediaTexture.Apply();
                IBuffer buffer = LFFrameData.AsBuffer();
                Debug.Log($"buffer length: {buffer.Length}");
                //text.text = "Build bit map";
                //SoftwareBitmap bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 640, 480);
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                Debug.Log("Start handle track");
                // HandleArUcoTracking(bitmap, 1);
                //text.text = "end handle track";
            }
        }

        if (RFPreviewPlane != null && researchMode.RFImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetRFCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (RFFrameData == null)
                {
                    RFFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, RFFrameData, 0, RFFrameData.Length);
                }

                RFMediaTexture.LoadRawTextureData(RFFrameData);
                RFMediaTexture.Apply();
                IBuffer buffer = RFFrameData.AsBuffer();
                Debug.Log($"buffer length: {buffer.Length}");
                //text.text = "Build bit map";
                //SoftwareBitmap bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 640, 480);
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                Debug.Log("Start handle track");
                HandleArUcoTracking(bitmap, 2);
                //text.text = "end handle track";
            }
        }
       
        if (RRPreviewPlane != null && researchMode.RRImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetRRCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (RRFrameData == null)
                {
                    RRFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, RRFrameData, 0, RRFrameData.Length);
                }

                RRMediaTexture.LoadRawTextureData(RRFrameData);
                RRMediaTexture.Apply();
                IBuffer buffer = RRFrameData.AsBuffer();
                Debug.Log($"buffer length: {buffer.Length}");
                //text.text = "Build bit map";
                //SoftwareBitmap bitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, 640, 480);
                SoftwareBitmap bitmap_gray = SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Gray8, 640, 480);
                bitmap_gray.CopyFromBuffer(buffer);
                SoftwareBitmap bitmap = SoftwareBitmap.Convert(bitmap_gray, BitmapPixelFormat.Bgra8);
                Debug.Log("Start handle track");
                HandleArUcoTracking(bitmap, 3);
                //text.text = "end handle track";
            }
        } 
#endif

#if ENABLE_WINMD_SUPPORT
        // update LF camera texture
        if (startRealtimePreview && LFPreviewPlane != null && researchMode.LFImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetLFCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (LFFrameData == null)
                {
                    LFFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, LFFrameData, 0, LFFrameData.Length);
                }

                if (showRealtimeFeed)
                {
                    LFMediaMaterial.mainTexture = LFMediaTexture;   
                    LFMediaTexture.LoadRawTextureData(LFFrameData);
                    LFMediaTexture.Apply();   
                }
                else
                {
                    LFMediaMaterial.mainTexture = null;
                }
            }
        }
        // update RF camera texture
        if (startRealtimePreview && RFPreviewPlane != null && researchMode.RFImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetRFCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (RFFrameData == null)
                {
                    RFFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, RFFrameData, 0, RFFrameData.Length);
                }

                if (showRealtimeFeed)
                {
                    RFMediaMaterial.mainTexture = RFMediaTexture;   
                    RFMediaTexture.LoadRawTextureData(RFFrameData);
                    RFMediaTexture.Apply();   
                }
                else
                {
                    RFMediaMaterial.mainTexture = null;
                }
            }
        }
        // update LL camera texture
        if (startRealtimePreview && LLPreviewPlane != null && researchMode.LLImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetLLCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (LLFrameData == null)
                {
                    LLFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, LLFrameData, 0, LLFrameData.Length);
                }

                if (showRealtimeFeed)
                {
                    LLMediaMaterial.mainTexture = LLMediaTexture;   
                    LLMediaTexture.LoadRawTextureData(LLFrameData);
                    LLMediaTexture.Apply();   
                }
                else
                {
                    LLMediaMaterial.mainTexture = null;
                }
            }
        }
        // update RR camera texture
        if (startRealtimePreview && RRPreviewPlane != null && researchMode.RRImageUpdated())
        {
            long ts;
            byte[] frameTexture = researchMode.GetRRCameraBuffer(out ts);
            if (frameTexture.Length > 0)
            {
                if (RRFrameData == null)
                {
                    RRFrameData = frameTexture;
                }
                else
                {
                    System.Buffer.BlockCopy(frameTexture, 0, RRFrameData, 0, RRFrameData.Length);
                }

                if (showRealtimeFeed)
                {
                    RRMediaMaterial.mainTexture = RRMediaTexture;   
                    RRMediaTexture.LoadRawTextureData(RRFrameData);
                    RRMediaTexture.Apply();   
                }
                else
                {
                    RRMediaMaterial.mainTexture = null;
                }
            }
        }
#endif
    }

#region Button Event Functions
    public void ToggleFeedEvent()
    {
        showRealtimeFeed = !showRealtimeFeed;
    }

    bool renderPointCloud = true;

    public void StopSensorsEvent()
    {
#if ENABLE_WINMD_SUPPORT
        researchMode.StopAllSensorDevice();
#endif
        startRealtimePreview = false;
    }

    public void Radar3DActiveEvent()
    {
        radar3DActive = !radar3DActive;
        if(radar3DActive) {
            arrows3DActive = false;
            arrows3D.mapActivate(arrows3DActive);
        }
        radar3D.mapActivate(radar3DActive);
    }
    public void Arrows3DActiveEvent()
    {
        arrows3DActive = !arrows3DActive;
        if(arrows3DActive) {
            radar3DActive = false;
            radar3D.mapActivate(radar3DActive);
        }
        arrows3D.mapActivate(arrows3DActive);
    }

#endregion

    public void MapsDeactive() {

    }
    
    public bool Radar3DIsActive() {
        return radar3DActive;
    }
    public bool Arrows3DIsActive() {
        return arrows3DActive;
    }


    private void OnApplicationFocus(bool focus)
    {
        if (!focus) StopSensorsEvent();
    }

#if WINDOWS_UWP
    private long GetCurrentTimestampUnix()
    {
        // Get the current time, in order to create a PerceptionTimestamp. 
        Windows.Globalization.Calendar c = new Windows.Globalization.Calendar();
        Windows.Perception.PerceptionTimestamp ts = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(c.GetDateTime());
        return ts.TargetTime.ToUnixTimeMilliseconds();
        //return ts.SystemRelativeTargetTime.Ticks;
    }
    private Windows.Perception.PerceptionTimestamp GetCurrentTimestamp()
    {
        // Get the current time, in order to create a PerceptionTimestamp. 
        Windows.Globalization.Calendar c = new Windows.Globalization.Calendar();
        return Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(c.GetDateTime());
    }
#endif

#if ENABLE_WINMD_SUPPORT
    private void HandleArUcoTracking(SoftwareBitmap bitmap, int ArUcoOnCamID)
    {
        OpenCVRuntimeComponent.CameraCalibrationParams calibParams = 
              new OpenCVRuntimeComponent.CameraCalibrationParams(System.Numerics.Vector2.Zero, System.Numerics.Vector2.Zero, System.Numerics.Vector3.Zero, System.Numerics.Vector2.Zero, 0, 0);
        if (bitmap != null)
        {
            //text.text = "bitmap is not null";
            //System.Threading.Thread.Sleep(1000);
             switch (CalibrationParameterType)
            {
                // Cache from user-defined parameters 
                case ArUcoUtils.CameraCalibrationParameterType.UserDefined:
                    //text.text = "user defined param found";
                    Debug.Log("user defined param found");
                    //System.Threading.Thread.Sleep(1000);
                    if (ArUcoOnCamID == 0) //LL
                    {
                        calibParams = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsLL.focalLength.x, UserDefinedCalibParamsLL.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsLL.principalPoint.x, UserDefinedCalibParamsLL.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsLL.radialDistortion.x, UserDefinedCalibParamsLL.radialDistortion.y, UserDefinedCalibParamsLL.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsLL.tangentialDistortion.x, UserDefinedCalibParamsLL.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsLL.imageWidth, // Image width
                            UserDefinedCalibParamsLL.imageHeight); // Image height
                            Debug.Log($"User-defined calibParams: [{calibParams}]");
                    }
                    else if (ArUcoOnCamID == 1)
                    {
                        calibParams = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsLF.focalLength.x, UserDefinedCalibParamsLF.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsLF.principalPoint.x, UserDefinedCalibParamsLF.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsLF.radialDistortion.x, UserDefinedCalibParamsLF.radialDistortion.y, UserDefinedCalibParamsLF.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsLF.tangentialDistortion.x, UserDefinedCalibParamsLF.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsLF.imageWidth, // Image width
                            UserDefinedCalibParamsLF.imageHeight); // Image height
                            Debug.Log($"User-defined calibParams: [{calibParams}]");
                    }
                    else if (ArUcoOnCamID == 2)
                    {
                        calibParams = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsRF.focalLength.x, UserDefinedCalibParamsRF.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsRF.principalPoint.x, UserDefinedCalibParamsRF.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsRF.radialDistortion.x, UserDefinedCalibParamsRF.radialDistortion.y, UserDefinedCalibParamsRF.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsRF.tangentialDistortion.x, UserDefinedCalibParamsRF.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsRF.imageWidth, // Image width
                            UserDefinedCalibParamsRF.imageHeight); // Image height
                            Debug.Log($"User-defined calibParams: [{calibParams}]");
                    }
                    else if (ArUcoOnCamID == 3)
                    {
                        calibParams = new OpenCVRuntimeComponent.CameraCalibrationParams(
                            new System.Numerics.Vector2(UserDefinedCalibParamsRR.focalLength.x, UserDefinedCalibParamsRR.focalLength.y), // Focal length
                            new System.Numerics.Vector2(UserDefinedCalibParamsRR.principalPoint.x, UserDefinedCalibParamsRR.principalPoint.y), // Principal point
                            new System.Numerics.Vector3(UserDefinedCalibParamsRR.radialDistortion.x, UserDefinedCalibParamsRR.radialDistortion.y, UserDefinedCalibParamsRR.radialDistortion.z), // Radial distortion
                            new System.Numerics.Vector2(UserDefinedCalibParamsRR.tangentialDistortion.x, UserDefinedCalibParamsRR.tangentialDistortion.y), // Tangential distortion
                            UserDefinedCalibParamsRR.imageWidth, // Image width
                            UserDefinedCalibParamsRR.imageHeight); // Image height
                            Debug.Log($"User-defined calibParams: [{calibParams}]");
                    }
                    break;
                default:
                    //text.text = "user defined param not found";
                    Debug.Log("user defined param not found");
                    break;
            }

            switch (ArUcoTrackingType)
            {
                case ArUcoUtils.ArUcoTrackingType.Markers:
                    //text.text = "start detect marker";
                    //System.Threading.Thread.Sleep(1000);
                    Debug.Log("start detect marker");
                    DetectMarkers(bitmap, calibParams, ArUcoOnCamID);
                    break;

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

    private void DetectMarkers(SoftwareBitmap softwareBitmap, OpenCVRuntimeComponent.CameraCalibrationParams calibParams, int ArUcoOnCamID)
    {
        // Get marker detections from opencv component
        var detected_markers = CvUtils.DetectMarkers(softwareBitmap, calibParams);
        Vector3 pos = new Vector3(0.0f, 0.0f, 0.0f);
        if (ArUcoOnCamID == 0)
        {
            // foreach (var marker in markers)
            // {
            //     marker.GetComponent<Renderer>().enabled = false;
            // }
            foreach (var det_marker in detected_markers)
            {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                pos.y -= 1.1f;               
                // radar3D.plotMarkers(id, pos);
                arrows3D.arrowPoint(id, pos);
            }
            // textLL.text = $"LLDetected: {detected_markers.Count} markers, Pos : {pos.y}, {pos.x}, {pos.z}.";// Pos : {pos.x}, {pos.y}, {pos.z}";
            
        }
        else if (ArUcoOnCamID == 1)
        {
            foreach (var det_marker in detected_markers)
            {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                pos.y = pos.y * (-1);
                pos.x = pos.x * (-1);
                pos.y -= 0.02f;
                // radar3D.plotMarkers(id, pos);
                arrows3D.arrowPoint(id, pos);
            }
            // textLF.text = $"LFDetected: {detected_markers.Count} markers, Pos : {pos.y}, {pos.x}, {pos.z}.";// Pos : {pos.x}, {pos.y}, {pos.z}";

        }
        else if (ArUcoOnCamID == 2)
        {
            foreach (var det_marker in detected_markers)
            {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                pos.y += 0.3f;
                // radar3D.plotMarkers(id, pos);
                arrows3D.arrowPoint(id, pos);
            }
            // textRF.text = $"RFDetected: {detected_markers.Count} markers, Pos : {pos.y}, {pos.x}, {pos.z}.";// Pos : {pos.x}, {pos.y}, {pos.z}";
        }
        else if (ArUcoOnCamID == 3)
        {
            foreach (var det_marker in detected_markers)
            {
                int id = det_marker.Id;
                pos = ArUcoUtils.Vec3FromFloat3(det_marker.Position);
                pos.y = pos.y * (-1);
                pos.x = pos.x * (-1);
                pos.y += 0.95f;
                // radar3D.plotMarkers(id, pos);
                arrows3D.arrowPoint(id, pos);
            }
            // textRR.text = $"RRDetected: {detected_markers.Count} markers, Pos : {pos.y}, {pos.x}, {pos.z}.";// Pos : {pos.x}, {pos.y}, {pos.z}";
        }

    }
#endif
}