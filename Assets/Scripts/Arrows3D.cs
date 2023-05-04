using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Input;

public class Arrows3D : MonoBehaviour
{
    public UnityEngine.UI.Text textRF, textLF, textLL;
    public Cameras Cameras;
    public Camera MainCamera;
    public GameObject mapObject;
    public GameObject arrow0, arrow1, arrow2, arrow3;
    public DataCollection dataCollection;

    private Vector3 head;
    private Vector3 mapCenter = new Vector3(0.0f, -0.2f, 1.0f);
    private GameObject[] arrows;

    bool isActive;

    public void mapStart() {
        arrows = new GameObject[] {arrow0, arrow1, arrow2, arrow3};
        isActive = Cameras.Arrows3DIsActive();
        mapObject.gameObject.SetActive(false);
        for(int i = 0; i < arrows.Length; i++) {
            arrows[i].gameObject.SetActive(false);
            arrows[i].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        }
    }

    public void mapActivate(bool isActive) {
        mapObject.gameObject.SetActive(isActive);
        for(int i = 0; i < arrows.Length; i++) {
            arrows[i].gameObject.SetActive(isActive);
        }
    }

    public void markersRenderDisabled() {
        for(int i = 0; i < arrows.Length; i++) {
            arrows[i].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        }
    }

    public void arrowPoint(int markerId, Vector3 pos) {
        // arrows[markerId].transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        head = MainCamera.transform.position;
        // textLL.text = $"CameraPos : {head.x}, {head.y}, {head.z}.";
        // textLF.text = $"ArUcoPos : {pos.y}, {pos.x}, {pos.z}.";
        // textRF.text = $"CameraPos : {head.x + pos.y}, {head.y + pos.x}, {head.z + pos.z}.";
        arrows[markerId].transform.position = new Vector3(head.x, head.y, head.z);
        arrows[markerId].transform.LookAt(new Vector3(pos.y, pos.x, pos.z), Vector3.up);
        arrows[markerId].transform.position = new Vector3(mapCenter.x, mapCenter.y, mapCenter.z);
        arrows[markerId].transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        
        if(!dataCollection.getTestStart()) {
            arrows[markerId].transform.GetChild(0).GetComponent<Renderer>().enabled = true;
        } else {
            if(markerId == dataCollection.getCurrId()) {
                arrows[markerId].transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            }
        }
    }

    public void enableMarkerById(int markerId, bool enabled) {
        arrows[markerId].transform.GetChild(0).GetComponent<Renderer>().enabled = enabled;
    }

}