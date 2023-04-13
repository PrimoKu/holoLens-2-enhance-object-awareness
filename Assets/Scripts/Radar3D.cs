using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Input;
using TMPro;

public class Radar3D : MonoBehaviour
{
    public Cameras Cameras;
    public Camera MainCamera;
    public GameObject mapObject;
    public GameObject marker0, marker1, marker2, marker3;

    private float radius;
    private Vector3 mapCenter;
    private Canvas mapCanvas;
    private float rotateX = 75.0f;
    private GameObject[] markers;
    private float leftmost = -1.6f;
    private float rightmost = 1.6f;
    private float depthmost = 2f;

    bool isActive;

    public void mapStart() {
        mapCanvas = mapObject.transform.GetChild(0).GetComponent<Canvas>();
        mapCanvas.transform.GetChild(0).GetComponent<RawImage>().transform.Rotate(rotateX, 0.0f, 0.0f);
        mapCenter = mapObject.GetComponent<Transform>().GetChild(0).GetComponent<RectTransform>().localPosition;
        radius = mapCanvas.GetComponent<RectTransform>().rect.yMax;
        markers = new GameObject[] {marker0, marker1, marker2, marker3};

        isActive = Cameras.Radar3DIsActive();
        mapCanvas.gameObject.SetActive(false);
        for(int i = 0; i < markers.Length; i++) {
            markers[i].gameObject.SetActive(false);
            markers[i].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            markers[i].transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
            markers[i].transform.GetChild(2).GetComponent<Canvas>().enabled = false;
        }
    }

    public void mapActivate(bool isActive) {
        mapCanvas.gameObject.SetActive(isActive);
        for(int i = 0; i < markers.Length; i++) {
            markers[i].gameObject.SetActive(isActive);
        }
    }

    public void markersRenderDisabled() {
        for(int i = 0; i < markers.Length; i++) {
            markers[i].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            markers[i].transform.GetChild(1).GetComponent<LineRenderer>().enabled = false;
            markers[i].transform.GetChild(2).GetComponent<Canvas>().enabled = false;
        }
    }

    // vec.x: Real vertical coordinate of detected marker
    // vec.y: Real horizontal coordinate of detected marker
    // vec.z: Real depth coordinate of detected marker
    public void plotMarkers(int markerId, Vector3 vec) {
        var marker = markers[markerId].transform;
        var sphere = marker.GetChild(0).GetComponent<Renderer>();
        var line = marker.GetChild(1).GetComponent<LineRenderer>();
        var markerBase = marker.GetChild(2).GetComponent<Canvas>();

        // TMP_text text = markerBase.transform.GetChild(0).GetComponent<TextMeshPro>();

        float thetaDegree = 180 * (vec.y - rightmost) / (leftmost - rightmost);
        float x, y, z;
        if(vec.z > depthmost) {
            x = radius * Mathf.Cos((thetaDegree * Mathf.PI)/180);
            y = radius * Mathf.Sin((thetaDegree * Mathf.PI)/180) * Mathf.Cos((rotateX * Mathf.PI)/180);
            z = radius * Mathf.Sin((thetaDegree * Mathf.PI)/180) * Mathf.Sin((rotateX * Mathf.PI)/180);
        } else {
            x = radius * (vec.z/depthmost) * Mathf.Cos((thetaDegree * Mathf.PI)/180);
            y = radius * (vec.z/depthmost) * Mathf.Sin((thetaDegree * Mathf.PI)/180) * Mathf.Cos((rotateX * Mathf.PI)/180);
            z = radius * (vec.z/depthmost) * Mathf.Sin((thetaDegree * Mathf.PI)/180) * Mathf.Sin((rotateX * Mathf.PI)/180);
        }
        float yOff = (vec.x/depthmost)*radius*1.5f;

        marker.localPosition = new Vector3(mapCenter.x, mapCenter.y, mapCenter.z);
        sphere.transform.localPosition = new Vector3(x, y + yOff, z);
        line.useWorldSpace = false;
        line.SetPosition(0, new Vector3(x, y, z));
        line.SetPosition(1, new Vector3(x, y + yOff, z));
        line.startColor = Color.white;
        markerBase.GetComponent<RectTransform>().localPosition = new Vector3(x, y, z);
        // markerBase.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Marker Id: {markerId}";
        // markerBase.transform.GetChild(0).GetComponent<TextMeshProUGUI>().transform.position = new Vector3(mapCenter.x + x + 0.05f, mapCenter.y + y + yOff, mapCenter.z + z);

        if(vec.x < 0) {
            sphere.material.SetColor("_Color", Color.blue);
            line.endColor = Color.blue;
            markerBase.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.blue;
            markerBase.transform.GetChild(1).GetComponent<RawImage>().color = Color.blue;
        } else {
            sphere.material.SetColor("_Color", Color.red);
            line.endColor = Color.red;
            markerBase.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
            markerBase.transform.GetChild(1).GetComponent<RawImage>().color = Color.red;
        }

        sphere.enabled = true;
        line.enabled = true;
        markerBase.enabled = true;
        
    }

}