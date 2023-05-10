using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualButton : MonoBehaviour
{
    public Cameras Cameras;
    public Camera MainCamera;
    public Button button0, button1, button2, button3, button4, button5, button6, button7, button8, button9;
    private Button[] buttons;
    // public UnityEngine.UI.Text textRF;
    // Start is called before the first frame update
    public void buttonStart()
    {
        buttons = new Button[] { button0, button1, button2, button3, button4, button5, button6, button7, button8, button9 };
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
        var button = buttons[markerId].transform;
        pos.y -= 0.05f;
        button.SetPositionAndRotation(pos, rot);
        // textRF.text = $"Trans ArUco Pos: {pos.x}, {pos.y}, {pos.z}";
        button.GetComponent<Renderer>().enabled = true;
    }

    public void allColorReset() {
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i].ColorReset();
        }
    }

    public void enableMarkerById(int markerId, bool enabled) {
        var button = buttons[markerId].transform;
        button.GetComponent<Renderer>().enabled = enabled;
    }
}
