using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;

public struct Flower {
    public int color;
    public int petal;
    public bool tulip;
};

public class FlowerScript : MonoBehaviour {
    [SerializeField]
    ARTrackedImageManager arTrackedImageManager;
    [SerializeField]
    XRReferenceImageLibrary myImageLibrary;
    public TextAsset textfile;
    public Dictionary<string, Flower> dict;
    void Start() {
        arTrackedImageManager.referenceLibrary = myImageLibrary;
        dict = new Dictionary<string, Flower>();
        ParseInfo();
    }

    void Update() {
        
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
        Color32 blue = new Color32(204, 255, 255, 255);
        Color32 pink = new Color32(255, 153, 204, 255);

        var planeGo = trackedImage.transform.GetChild(0).gameObject;
        var personGo = trackedImage.transform.GetChild(1).gameObject;
        var name = trackedImage.referenceImage.name;

        // Disable the visual plane if it is not being tracked
        if (trackedImage.trackingState == TrackingState.Tracking) {
            planeGo.SetActive(true);
            personGo.SetActive(true);

            // Set the texture
            var material = planeGo.GetComponentInChildren<MeshRenderer>().material;
            //material.mainTexture = trackedImage.referenceImage.texture;
            // Set the color
            foreach (Transform child in personGo.transform) {
                child.GetComponent<MeshRenderer>().material.color = blue;
                child.name = name;
            }
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

        foreach (string s in d) {
            Info input;
            string[] inp = s.Split(' ');
            string name = "m" + index;
            input.height = int.Parse(inp[1]);
            input.weight = int.Parse(inp[2]);
            input.gender = (inp[3] == "³²ÀÚ") ? true : false;
            //dict.Add(name, input);
            ++index;
            Debug.Log("Parsed " + name);
        }

        //give info to touchmanager
        //FindObjectOfType<TouchManager>().dict = dict;
    }
}