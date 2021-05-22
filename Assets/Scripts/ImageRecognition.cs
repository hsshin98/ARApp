using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public struct Info {
    public const int Boy = 1, Girl = 2;
    public const int minH = 90, maxH = 190;
    public const int minW = 20, maxW = 100;
    public const int gt = 1, lt = 2;
    public const int H = 1, W = 2;
    public const int inc = 1, dec = 2;

    public int height;
    public int weight;
    public int gender;
};

public class ImageRecognition : MonoBehaviour {
    ARTrackedImageManager arTrackedImageManager;
    public TextAsset textfile;
    public Material blue, pink;
    public Material correct, incorrect;
    public Material height, weight, lt, gt, boy, girl, inc, dec;
    public Dictionary<string, Info> dict;
    public GameObject result;

    private PlacementManager pm;

    private string att;
    private int value;
    private int gender;
    public bool isDone;
    private bool isRunning;
    private void Awake() {
        arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        pm = FindObjectOfType<PlacementManager>();
        dict = new Dictionary<string, Info>();
        ParseInfo();
    }

    void Start() {
        //arTrackedImageManager.referenceLibrary = myImageLibrary;
        isRunning = false;
        result.SetActive(false);
    }

    void Update() {
        
    }
    
    public void OnEnable() {
        arTrackedImageManager.trackedImagesChanged += OnImageChanged;
    }

    public void OnDisable() {
        arTrackedImageManager.trackedImagesChanged -= OnImageChanged;
    }
    void OnImageChanged(ARTrackedImagesChangedEventArgs args) {
        foreach (var trackedImage in args.added) {
            InitTrackable(trackedImage);
        }

        foreach (var trackedImage in args.updated) {
            UpdateTrackable(trackedImage);
        }

        foreach(var trackedImage in args.removed) {
            Debug.Log("Removed");
        }
    }

    void InitTrackable(ARTrackedImage trackedImage) {
        var planeGo = trackedImage.transform.GetChild(0).gameObject;
        var personGo = trackedImage.transform.GetChild(1).gameObject;
        var name = trackedImage.referenceImage.name;
        
        if(name[0] == 'q') {
            Debug.Log("non object marker detected");
            personGo.SetActive(false);
            planeGo.name = name;
            Material mat = null;
            switch(name) {
                case "qrw":
                    mat = weight;
                    break;
                case "qrh":
                    mat = height;
                    break;
                case "qrlt":
                    mat = lt;
                    break;
                case "qrgt":
                    mat = gt;
                    break;
                case "qrboy":
                    mat = boy;
                    break;
                case "qrgirl":
                    mat = girl;
                    break;
                case "qrinc":
                    mat = inc;
                    break;
                case "qrdec":
                    mat = dec;
                    break;
            }
            planeGo.GetComponent<MeshRenderer>().material = mat;
            return;
        }
        
        var info = dict[name];
        Debug.Log("Init trackable: " + name);
        personGo.name = name;
        personGo.transform.GetChild(0).gameObject.name = name;
        personGo.GetComponentInChildren<MeshRenderer>().material = (info.gender == Info.Boy) ? blue : pink;

        planeGo.GetComponent<MeshRenderer>().material.color = Color.white;

        // Set scale
        Vector3 scale = new Vector3(info.weight / 25f, info.height / 75f, info.weight / 25f);
        personGo.transform.localScale = scale;

        // Instantiate Object Button
        pm.InstantiateButton(trackedImage);
    }
    void UpdateTrackable(ARTrackedImage trackedImage) {        
        var planeGo = trackedImage.transform.GetChild(0).gameObject;
        var personGo = trackedImage.transform.GetChild(1).gameObject;
        var name = trackedImage.referenceImage.name;

        if(name[0] == 'q') {
            if(trackedImage.trackingState == TrackingState.Tracking) {
                planeGo.SetActive(true);
            }
            else {
                planeGo.SetActive(false);
            }
            return;
        }

        var info = dict[name];

        // Disable the visual plane if it is not being tracked
        if (trackedImage.trackingState == TrackingState.Tracking) { 
            personGo.SetActive(true);
            planeGo.SetActive(true);

            if (isRunning) {
                int val = (att == "H") ? info.height : info.weight;
                if(val >= value && info.gender == gender)
                    planeGo.GetComponent<MeshRenderer>().material = correct;
                else 
                    planeGo.GetComponent<MeshRenderer>().material = incorrect;
            }
        }
        else {
            planeGo.SetActive(false);
            personGo.SetActive(false);
        }
    }
    void RemoveTrackable(ARTrackedImage trackedImage) {
        //should able manual placement via plane detection
    }

    //Parsing Data
    void ParseInfo() {
        int index = 1;
        var splitFile = new string[] { "\r\n", "\r", "\n" };
        string[] d = textfile.text.Split(splitFile, StringSplitOptions.None);
        foreach (string s in d) {
            Info input;
            string[] inp = s.Split(' ');
            string name = "m" + index;
            input.height = int.Parse(inp[1]);
            input.weight = int.Parse(inp[2]);
            input.gender = (inp[3] == "³²ÀÚ") ? Info.Boy : Info.Girl;
            dict.Add(name, input);
            ++index;
            Debug.Log("Parsed " + name);
        }
    }
}