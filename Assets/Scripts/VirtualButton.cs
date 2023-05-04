using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualButton : MonoBehaviour
{
    public Cameras Cameras;
    public Camera MainCamera;
    public GameObject buttonObject;
    public Button button0, button1, button2, button3;
    private Button[] buttons;
    public UnityEngine.UI.Text textRF;
    // Start is called before the first frame update
    public void buttonStart()
    {
        buttons = new Button[] { button0, button1, button2, button3 };
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i].gameObject.SetActive(true);
        }
    }

    public void buttonsRenderDisabled() {
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i].transform.GetComponent<Renderer>().enabled = false;
        }
    }
    // Update is called once per frame
    public void plotButton(int markerId, Vector3 cPos, Vector3 pos, Quaternion rot) 
    {
        // Debug.Log(pos); 
        var button = buttons[markerId].transform;
        // pos = new Vector3(pos.x - cPos.x, pos.y - cPos.y, pos.z - cPos.z);
        // pos = new Vector3(pos.x + 0.1f, pos.y, pos.z + 0.3f);
        textRF.text = $"Trans ArUco Pos: {pos.y}, {pos.x}, {pos.z}";
        button.SetPositionAndRotation(pos, rot);
        // button.gameObject.SetActive(true);
        // button.localPosition =  new Vector3(pos.y - cPos.x, pos.x - cPos.y, pos.z - cPos.z);
        // button.localPosition =  new Vector3(pos.y, pos.x, pos.z);
        button.GetComponent<Renderer>().enabled = true;
    }

    public void allColorReset() {
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i].ColorReset();
        }
    }
}
