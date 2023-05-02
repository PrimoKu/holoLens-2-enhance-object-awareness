using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    public int id;
    public DataCollection dataCollection;
    public void OnClick()
    {
        GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        
        if(dataCollection.getTestStart()) {
            dataCollection.setMarkerClicked(id);
        }
    }

    public void ColorReset() {
        Debug.Log("Color reset");
        GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }

    public IEnumerator ColorLerp() {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = Color.Lerp(renderer.material.color, Color.red, Time.deltaTime * 0.5f);
        yield return new WaitUntil(() => renderer.material.color == Color.red);
    }
}
