using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EvaluationManager : MonoBehaviour {
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
    private int value, done; // start and end values for slider
    private bool isRunning;
    private string att;
    private bool gender;
    private float acc;
    private List<int> list;
    private PlacementManager pm;
    private void Awake() {
        pm = FindObjectOfType<PlacementManager>();
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void Start() {
        dict = new Dictionary<string, Info>();
        isRunning = false;
        result.SetActive(false);
    }

    void Update() {

    }

    //----------------------------------//
    //      learning algorithm          //
    //----------------------------------//
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

        if (res > acc) {
            //reset list and update optimal
            acc = res;
            list = new List<int>();
            list.Add(value);
        }
        else if (res == acc) {
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
}