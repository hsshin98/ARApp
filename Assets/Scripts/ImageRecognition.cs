using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public struct Info {
    public int height;
    public int weight;
    public bool gender;
};

public class ImageRecognition : MonoBehaviour {
    [SerializeField]
    ARTrackedImageManager arTrackedImageManager;
    [SerializeField]
    XRReferenceImageLibrary myImageLibrary;
    public TextAsset textfile;
    public Material blue, pink;
    public Material correct, incorrect;
    public Dictionary<string, Info> dict;
    public GameObject result;
    public bool isDone;
    private int minH, maxH, minW, maxW;
    private int value, done;
    private bool isRunning;
    private string att;
    private bool gender;
    private float acc;
    private List<int> list;
    void Start() {
        arTrackedImageManager.referenceLibrary = myImageLibrary;
        dict = new Dictionary<string, Info>();
        ParseInfo();
        isRunning = false;
        result.SetActive(false);
    }

    public void Run(string attribute, bool g) {
        var slider = result.GetComponentInChildren<Slider>();
        result.SetActive(true);
        isRunning = true;
        att = attribute;
        gender = g;
        acc = -1.0f;
        if (attribute == "H") {
            value = minH;
            done = maxH;
            slider.minValue = minH;
            slider.maxValue = maxH;
        }
        else {
            value = minW;
            done = maxW;
            slider.minValue = minW;
            slider.maxValue = maxW;
        }
        InvokeRepeating("Evaluate", 0, 0.1f);
    }
    void Update() {
        
    }
    void Evaluate() {
        float res, correct = 0.0f;
        string str = att + "eight greater than " + value + " is ";
        str += gender ? "Boy" : "Girl";
        result.GetComponentInChildren<Slider>().value = value;
        foreach (var p in dict) {
            var i = p.Value;
            int val = 0;
            
            if (att == "H")
                val = i.height;
            else if (att == "W")
                val = i.weight;

            if (val >= value && i.gender == gender)
                correct += 1.0f;
            else if (val < value && i.gender != gender)
                correct += 1.0f;
        }
        res = correct / dict.Count * 100;

        if(res > acc) {
            //reset list and update optimal
            acc = res;
            list = new List<int>();
            list.Add(value);
        }
        else if(res == acc) {
            //add to optimal values
            list.Add(value);
        }

        //display result
        result.transform.GetChild(2).GetComponent<Text>().text = "" + res + "%";
        result.transform.GetChild(3).GetComponent<Text>().text = str;
        ++value;
        
        if (value > done) {
            InvokeRepeating("Finish", 0.1f, 1f);
        }
            
        return;
    }

    void Finish() {
        //finish evaluation
        float avg = 0.0f;
        float optimal;
        isRunning = false;
        CancelInvoke();

        //get optimal value
        foreach (int i in list)
            avg += i;
        optimal = avg / list.Count;

        string str = att + "eight greater than " + optimal + " is ";
        str += gender ? "Boy" : "Girl";

        result.transform.GetChild(2).GetComponent<Text>().text = "" + acc + "%";
        result.transform.GetChild(3).GetComponent<Text>().text = str;

        isDone = true;
    }
    public void OnEnable() {
        arTrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    public void OnDisable() {
        arTrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }
    public void OnImageChanged(ARTrackedImagesChangedEventArgs args) {
        foreach (var trackedImage in args.added) {
            UpdateInfo(trackedImage);
        }

        foreach (var trackedImage in args.updated) {
            UpdateInfo(trackedImage);
        }
    }

    void UpdateInfo(ARTrackedImage trackedImage) {        
        var planeGo = trackedImage.transform.GetChild(0).gameObject;
        var personGo = trackedImage.transform.GetChild(1).gameObject;
        var name = trackedImage.referenceImage.name;
        var info = dict[name];

        // Disable the visual plane if it is not being tracked
        if (trackedImage.trackingState == TrackingState.Tracking) {
            planeGo.SetActive(true);
            personGo.SetActive(true);

            // Set name
            personGo.transform.GetChild(0).gameObject.name = name;

            // Set texture
            personGo.GetComponentInChildren<MeshRenderer>().material = info.gender ? blue : pink;

            // Set plane color
            planeGo.GetComponent<MeshRenderer>().material.color = Color.white;
            if(isRunning) {
                int val = (att == "H") ? info.height : info.weight;
                if(val >= value && info.gender == gender)
                    planeGo.GetComponent<MeshRenderer>().material = correct;
                else 
                    planeGo.GetComponent<MeshRenderer>().material = incorrect;
            }
            // Set scale
            Vector3 scale = new Vector3(info.weight / 25f, info.height / 75f, info.weight / 25f);
            personGo.transform.localScale = scale;
        }
        else {
            planeGo.SetActive(false);
            personGo.SetActive(false);
        }
    }

    void ParseInfo() {
        int index = 1;
        var splitFile = new string[] { "\r\n", "\r", "\n" };
        string[] d = textfile.text.Split(splitFile, StringSplitOptions.None);
        string[] inp = { "" };
        foreach (string s in d) {
            Info input;
            inp = s.Split(' ');
            if(int.Parse(inp[0]) == -1)
                break;
            string name = "m" + index;
            input.height = int.Parse(inp[1]);
            input.weight = int.Parse(inp[2]);
            input.gender = (inp[3] == "³²ÀÚ") ? true : false;
            dict.Add(name, input);
            ++index;
            Debug.Log("Parsed " + name);
        }

        //give info to touchmanager
        var tm = FindObjectOfType<TouchManager>();
        tm.dict = dict;
        tm.minH = minH = int.Parse(inp[1]);
        tm.maxH = maxH = int.Parse(inp[2]);
        tm.minW = minW = int.Parse(inp[3]);
        tm.maxW = maxW = int.Parse(inp[4]);
    }
}