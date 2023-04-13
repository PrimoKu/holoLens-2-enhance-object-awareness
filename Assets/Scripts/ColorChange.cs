using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChange : MonoBehaviour
{
    public void PointerClick()
    {
        GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        Debug.Log("OK");
    }
}
