using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class DataCollection : MonoBehaviour
{
    public Cameras cameras;
    public Radar3D radar3D;
    public Arrows3D arrows3D;
    public EyeSee3D eyeSee3D;
    public VirtualButton virtualButton;
    public Button button0, button1, button2, button3;
    private Button[] buttons;
    private List<int> randomMarkerList;
    private List<bool> markerClicked;
    private bool testStart = false;
    private bool anyButtonIsClicked = false;
    private int currId;

    public UnityEngine.UI.Text textRR;

    void Start() {
        buttons = new Button[] {button0, button1, button2, button3};
    }
    public IEnumerator dataCollectionStart()
    {
        testStart = true;
        int mapType = cameras.MapActiveType();
        randomMarkerList = randomList();
        markerClicked = new List<bool> {false, false, false, false};
        virtualButton.allColorReset();
        int count = 0;
        if (mapType == 1) {
            // radar3D.markersRenderDisabled();
            for(int i = 0; i < buttons.Length; i++) {
                currId = randomMarkerList[i];
                textRR.text = $"Current ID: {randomMarkerList[i]}";
                // radar3D.enableMarkerById(randomMarkerList[i], true);
                yield return new WaitUntil(() => anyButtonIsClicked);
                if(markerIsClicked(randomMarkerList[i])) {
                    count++;
                }
                // radar3D.enableMarkerById(randomMarkerList[i], false);
                anyButtonIsClicked = false;
                // StartCoroutine(buttons[randomMarkerList[i]].ColorLerp());
            }
            Debug.Log("Accuracy: " + (double)count/4);
            testStart = false;
            virtualButton.allColorReset();
        }
    }

    private List<int> randomList() {
        List<int> markerList = new List<int>();
        System.Random random = new System.Random();
        int num = 0;

        for(int i = 0; i < 4; i++) {
            num = random.Next(0, 4);
            do{
                num = random.Next(0, 4);
            } while (markerList.Contains(num));

            markerList.Add(num);
        }

        return markerList;
    }

    bool markerIsClicked(int id) {
        return markerClicked[id];
    }
    public void setMarkerClicked(int id) {
        markerClicked[id] = true;
        anyButtonIsClicked = true;
    }

    public bool getTestStart() {
        return testStart;
    }

    public int getCurrId() {
        return currId;
    }
}
