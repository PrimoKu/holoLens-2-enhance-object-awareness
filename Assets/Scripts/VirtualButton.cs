using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualButton : MonoBehaviour
{
    public Cameras Cameras;
    public Camera MainCamera;
    public Button button0, button1, button2, button3, button4, button5, button6, button7, button8, button9;
    private Button[] buttons;
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
    public void plotButton(int markerId, Vector3 pos, Quaternion rot) 
    {
        var button = buttons[markerId].transform;
        pos.x = pos.x - 0.03f;
        pos.y = pos.y - 0.05f;
        button.SetPositionAndRotation(pos, rot);
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
