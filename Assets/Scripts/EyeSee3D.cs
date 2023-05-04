using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Input;

public class EyeSee3D : MonoBehaviour
{
    public Cameras Cameras;
    public Camera MainCamera;
    public GameObject mapObject;
    public GameObject marker0, marker1, marker2, marker3;
    public DataCollection dataCollection;

    private float a, b;
    private Vector3 mapCenter;
    private Canvas mapCanvas;
    private GameObject[] markers;
    bool isActive;
    
    public void mapStart()
    {
        mapCanvas = mapObject.transform.GetChild(0).GetComponent<Canvas>();
        mapCenter = mapObject.GetComponent<Transform>().GetChild(0).GetComponent<RectTransform>().localPosition;
        a = mapCanvas.GetComponent<RectTransform>().rect.xMax * 0.4f;
        b = mapCanvas.GetComponent<RectTransform>().rect.yMax * 0.4f;
        markers = new GameObject[] {marker0, marker1, marker2, marker3};

        isActive = Cameras.EyeSee3DIsActive();
        mapCanvas.gameObject.SetActive(false);
        for(int i = 0; i < markers.Length; i++) {
            markers[i].gameObject.SetActive(false);
            markers[i].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        }
    }

    // Update is called once per frame
    public void mapActivate(bool isActive) {
        mapCanvas.gameObject.SetActive(isActive);
        for(int i = 0; i < markers.Length; i++) {
            markers[i].gameObject.SetActive(isActive);
        }
    }

    public void markersRenderDisabled() {
        for(int i = 0; i < markers.Length; i++) {
            markers[i].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        }
    }

    public void plotMarkers(int markerId, Vector3 vec) {
        var marker = markers[markerId].transform;
        var sphere = marker.GetChild(0).GetComponent<Renderer>();

        float theta = Mathf.Atan2(vec.x, vec.z);
        if (theta >= Mathf.PI){
            theta -= Mathf.PI;
        } else if(theta <= -Mathf.PI) {
            theta += Mathf.PI;
        }
        float psi = Mathf.Atan2(vec.y, vec.z);

        float x = (a * psi) / Mathf.PI;
        float y = (2 * b * theta) / Mathf.PI;
        float z = 0.0f;

        marker.localPosition = new Vector3(mapCenter.x, mapCenter.y, mapCenter.z);
        sphere.transform.localPosition = new Vector3(x/0.4f, y/0.4f, z/0.4f);

        if(!dataCollection.getTestStart()) {
            sphere.enabled = true;
        } else {
            if(markerId == dataCollection.getCurrId()) {
                sphere.enabled = true;
            }
        }
    }

    public void enableMarkerById(int markerId, bool enabled) {
        var marker = markers[markerId].transform;
        var sphere = marker.GetChild(0).GetComponent<Renderer>();

        sphere.enabled = enabled;
    }
}
