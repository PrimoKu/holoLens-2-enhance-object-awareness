using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Newtonsoft.Json;
public class DataCollection : MonoBehaviour
{
    public Cameras cameras;
    public Radar3D radar3D;
    public Arrows3D arrows3D;
    public EyeSee3D eyeSee3D;
    public VirtualButton virtualButton;
    public Button button0, button1, button2, button3;
    private Button[] buttons;
    private int[] counts;
    private double[] accuracys;
    private double totalAcc;
    private List<int> randomMarkerList;
    private List<bool> markerClicked;
    private bool testStart = false;
    private bool anyButtonIsClicked = false;
    private int currId;

    public UnityEngine.UI.Text textRR, text;

    void Start() {
        buttons = new Button[] {button0, button1, button2, button3};
        counts = new int[] {0, 0, 0, 0, 0};
        accuracys = new double[] {0.0, 0.0, 0.0, 0.0, 0.0};
        text.text = $"Ready to collect data";
    }
    public IEnumerator dataCollectionStart()
    {
        testStart = true;
        int mapType = cameras.MapActiveType();
        // int count = 0;
        for(int j = 0; j < 5; j++) {
            text.text = $"Round {j+1}";
            randomMarkerList = randomList();
            markerClicked = new List<bool> {false, false, false, false};
            virtualButton.allColorReset();
            for(int i = 0; i < buttons.Length; i++) {
                currId = randomMarkerList[i];
                textRR.text = $"Current ID: {randomMarkerList[i]}";
                yield return new WaitUntil(() => anyButtonIsClicked);
                if(markerIsClicked(randomMarkerList[i])) {
                    counts[j]++;
                }
                anyButtonIsClicked = false;
            }
            accuracys[j] = (double)counts[j] / 4;
        }
        Array.ForEach(accuracys, i => totalAcc += i);
        text.text = $"Test accuracy: {totalAcc/5*100}";
        // if (mapType == 1) {
        //     for(int i = 0; i < buttons.Length; i++) {
        //         currId = randomMarkerList[i];
        //         textRR.text = $"Current ID: {randomMarkerList[i]}";
        //         yield return new WaitUntil(() => anyButtonIsClicked);
        //         if(markerIsClicked(randomMarkerList[i])) {
        //             count++;
        //         }
        //         anyButtonIsClicked = false;
        //     }
        // } 
        // else if (mapType == 2) {
        //     for(int i = 0; i < buttons.Length; i++) {
        //         currId = randomMarkerList[i];
        //         textRR.text = $"Current ID: {randomMarkerList[i]}";
        //         yield return new WaitUntil(() => anyButtonIsClicked);
        //         if(markerIsClicked(randomMarkerList[i])) {
        //             count++;
        //         }
        //         anyButtonIsClicked = false;
        //     }
        // }
        // else if (mapType == 3) {
        //     for(int i = 0; i < buttons.Length; i++) {
        //         currId = randomMarkerList[i];
        //         textRR.text = $"Current ID: {randomMarkerList[i]}";
        //         yield return new WaitUntil(() => anyButtonIsClicked);
        //         if(markerIsClicked(randomMarkerList[i])) {
        //             count++;
        //         }
        //         anyButtonIsClicked = false;
        //     }
        // }
        testStart = false;
        virtualButton.allColorReset();
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
