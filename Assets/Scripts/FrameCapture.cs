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

public class FrameCapture : MonoBehaviour
{
#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
#endif
    public Cameras cameras;
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
    bool showRealtimeFeed = false;

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
    void Start()
    {
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

        RRCameraPose[1,2] = RRCameraPose[1,2]*(-1.0f);
        RRCameraPose[2,1] = RRCameraPose[2,1]*(-1.0f);
        RRCameraPose[0,3] = -0.006604989f;
        RRCameraPose[1,3] = -0.109371658f;
        RRCameraPose[2,3] = -0.005209249f;
#endif
    }

    void FixedUpdate() {
#if ENABLE_WINMD_SUPPORT
        // update LL camera texture
        if (LLPreviewPlane != null && researchMode.LLImageUpdated()) {
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
                cameras.SetFrameBitmap(bitmap, 0);
                // HandleArUcoTracking(bitmap, 0);
                // DetectMarkers(bitmap, calibParamsLL, 0);
            }
        }
        // update LF camera texture
        if (LFPreviewPlane != null && researchMode.LFImageUpdated()) {
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
                cameras.SetFrameBitmap(bitmap, 1);
                // HandleArUcoTracking(bitmap, 1);
                // DetectMarkers(bitmap, calibParamsLF, 1);
            }
        }
        // update RF camera texture
        if (RFPreviewPlane != null && researchMode.RFImageUpdated()) {
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
                cameras.SetFrameBitmap(bitmap, 2);
                // HandleArUcoTracking(bitmap, 2);
                // DetectMarkers(bitmap, calibParamsRF, 2);
            }
        }
        // update RR camera texture
        if (RRPreviewPlane != null && researchMode.RRImageUpdated()) {
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
                cameras.SetFrameBitmap(bitmap, 3);
                // HandleArUcoTracking(bitmap, 3);
                // DetectMarkers(bitmap, calibParamsRR, 3);
            }
        } 
#endif
    }

    public void ToggleFeedEvent() {
        showRealtimeFeed = !showRealtimeFeed;
        LFPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
        RFPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
        LLPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
        RRPreviewPlane.transform.GetComponent<Renderer>().enabled = showRealtimeFeed;
    }

    public Vector3 transformToLFCameraFrame(Vector3 pose, int cameraId) {
        Vector3 poseWRTLF = new Vector3();

        switch(cameraId) {
            case 0:
                poseWRTLF = LLCameraPose.MultiplyPoint(pose);
                break;
            case 1:
                poseWRTLF = LFCameraPose.MultiplyPoint(pose);
                break;
            case 2:
                poseWRTLF = RFCameraPose.MultiplyPoint(pose);
                break;
            case 3:
                poseWRTLF = RRCameraPose.MultiplyPoint(pose);
                break;
            default:
                poseWRTLF = pose;
                break;
        }

        return poseWRTLF;
    }
}
