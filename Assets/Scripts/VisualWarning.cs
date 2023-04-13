using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualWarning : MonoBehaviour
{
    public Camera MainCamera;
    public GameObject Visuals;

    private RawImage Lside, Rside;
    void Start()
    {
        Lside = Visuals.transform.GetComponent<Canvas>().transform.GetChild(0).GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
