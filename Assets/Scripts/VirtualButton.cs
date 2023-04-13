using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualButton : MonoBehaviour
{
    public Cameras Cameras;
    public Camera MainCamera;
    public GameObject buttonObject;
    public GameObject button0;
    private GameObject[] buttons;
    public UnityEngine.UI.Text textRR;
    // Start is called before the first frame update
    public void buttonStart()
    {
        buttons = new GameObject[] { button0 };
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i].gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    public void plotButton(int markerId, Vector3 cPos, Vector3 pos, Quaternion rot) 
    {
        // Debug.Log(pos); 
        var button = buttons[markerId].transform;
        pos = new Vector3(pos.x + cPos.x, pos.y + cPos.y, pos.z + cPos.z);
        textRR.text = $"Button Pos: {pos}";
        button.SetPositionAndRotation(pos, rot);
        button.gameObject.SetActive(true);
    }
}
