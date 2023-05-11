using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
public class DataCollection : MonoBehaviour
{
    public Cameras cameras;
    public Radar3D radar3D;
    public Arrows3D arrows3D;
    public EyeSee3D eyeSee3D;
    public VirtualButton virtualButton;
    public Button button0, button1, button2, button3, button4, button5, button6, button7, button8, button9;
    private Button[] buttons;
    private int[] counts;
    private double[] accuracys;
    private double totalAcc;
    private float[] times;
    private float totalTime;
    private List<int> randomMarkerList;
    private List<bool> markerClicked;
    private bool testStart = false;
    private bool anyButtonIsClicked = false;
    private int currId;
    private float start_time, end_time;
    public UnityEngine.UI.Text text, testText;
    private string totalJsonData = "";
    private string fileName;

    void Start() {
        buttons = new Button[] {button0, button1, button2, button3, button4, button5, button6, button7, button8, button9};
        text.text = $"Ready to collect data";
        testText.gameObject.SetActive(false);
    }
    public IEnumerator dataCollectionStart()
    {
        counts = new int[] {0, 0, 0};
        accuracys = new double[] {0.0, 0.0, 0.0};
        times = new float[] {0.0f, 0.0f, 0.0f};
        
        testStart = true;
        testText.gameObject.SetActive(true);
        testText.text = $"Be prepare to find the markers!";
        yield return new WaitForSeconds(2);

        string mapType = cameras.MapActiveType();
        fileName = $"{mapType}.json";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        for(int j = 0; j < 3; j++) {
            radar3D.markersRenderDisabled();
            arrows3D.markersRenderDisabled();
            eyeSee3D.markersRenderDisabled();

            if(j == 0) cameras.EyeSee3DActiveEvent();
            else if(j == 1) cameras.Arrows3DActiveEvent();
            else if(j == 2) cameras.Radar3DActiveEvent();

            testText.gameObject.SetActive(true);
            text.text = $"Round {j+1}";
            testText.text = $"Round {j+1}!";
            yield return new WaitForSeconds(2);
            for (int i = 3; i > 0; i--) {
                testText.text = $"{i}";
                yield return new WaitForSeconds(1);
            }
            testText.text = "START!";
            yield return new WaitForSeconds(2);
            testText.gameObject.SetActive(false);
            randomMarkerList = randomList();
            markerClicked = new List<bool> {false, false, false, false, false, false, false, false, false, false};

            start_time = Time.time;
            for(int i = 0; i < buttons.Length; i++) {
                currId = randomMarkerList[i];
                yield return new WaitUntil(() => anyButtonIsClicked);
                if(markerIsClicked(randomMarkerList[i])) {
                    counts[j]++;
                }
                anyButtonIsClicked = false;
            }
            end_time = Time.time - start_time;
            times[j] = end_time;
            accuracys[j] = (double)counts[j] / 10;
            totalJsonData += dataToJson(mapType, j+1, accuracys[j], times[j]);
            testText.gameObject.SetActive(true);
            testText.text = $"Round {j+1} finished";
            yield return new WaitForSeconds(2);
            testText.gameObject.SetActive(false);
        }
        Array.ForEach(times, i => totalTime += i);
        Array.ForEach(accuracys, i => totalAcc += i);
        text.text = $"Test accuracy: {totalAcc/3*100}\nTest time:{totalTime/3}";
        testStart = false;
        using (var fileStream = new FileStream(filePath, FileMode.Append))
        {
            using (var streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.WriteLine(totalJsonData);
                text.text = $"File wrote";
            }
        }
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
    }

    private string dataToJson(string mapType, int numOfTest, double accuracy, float time) {
        return $"{{\"{mapType}\":" + 
                    $"{{\"Round\":{numOfTest}," + 
                    $"\"Accuracy\":{accuracy}," +
                    $"\"Time\":{time}}}" + "}\n";
    }

    private List<int> randomList() {
        List<int> markerList = new List<int>();
        System.Random random = new System.Random();
        int num = 0;

        for(int i = 0; i < 10; i++) {
            num = random.Next(0, 10);
            do{
                num = random.Next(0, 10);
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
