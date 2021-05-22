using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class EvaluationManager : MonoBehaviour {
    public Button run;

    private GameObject AttFrame, CompFrame, GenFrame;
    private bool isActive;
    private ARRaycastManager arRaycastManager;
    private ARTrackedImageManager arTrackedImageManager;
    private Dictionary<string, Info> dict;
    private GameObject pivot;
    private int selAtt, selComp, selGen;

    private void Awake() {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();

        pivot = transform.GetChild(0).gameObject;
    }

    void Start() {
        pivot.SetActive(false);

        run.onClick.AddListener(OnClickRun);
        AttFrame = pivot.transform.GetChild(0).gameObject;
        CompFrame = pivot.transform.GetChild(1).gameObject;
        GenFrame = pivot.transform.GetChild(2).gameObject;
    }
    int CheckValid(GameObject frame, int type) {
        Ray ray = Camera.main.ScreenPointToRay(frame.transform.position);
        RaycastHit hit;
        var valid = frame.transform.GetChild(1).gameObject;
        string placeholder, type1, type2;

        switch(type) {
            case 1: //attribute
                type1 = "qrh";
                type2 = "qrw";
                placeholder = "키 / 몸무게";
                break;

            case 2: // comp
                type1 = "qrgt";
                type2 = "qrlt";
                placeholder = "크다 / 작다";
                break;

            case 3: // gender
                type1 = "qrboy";
                type2 = "qrgirl";
                placeholder = "남자 / 여자";
                break;

            default:
                type1 = "";
                type2 = "";
                placeholder = "";
                break;
        }

        if(Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            var other = hit.collider.gameObject;
            if(other.name == type1) {
                valid.SetActive(true);
                frame.GetComponentInChildren<Text>().text = "";
                return 1;
            }
            else if(other.name == type2) {
                valid.SetActive(true);
                frame.GetComponentInChildren<Text>().text = "";
                return 2;
            }
            else {
                valid.SetActive(false);
                frame.GetComponentInChildren<Text>().text = placeholder;
                return -1;
            }
        }
        else {
            valid.SetActive(false);
            frame.GetComponentInChildren<Text>().text = placeholder;
            return -1;
        }
    }
    void Update() {
        if (isActive) {
            CheckValid(AttFrame, 1);
            CheckValid(CompFrame, 2);
            CheckValid(GenFrame, 3);
        }       
    }

    void RunEval() {

    }
    void OnClickRun() {
        //set pivot
        if (!isActive) {
            isActive = true;
            pivot.SetActive(true);
        }
        else {
            isActive = false;
            pivot.SetActive(false);
            AttFrame.GetComponentInChildren<Text>().text = "키 / 몸무게";
            CompFrame.GetComponentInChildren<Text>().text = "크다 / 작다";
        }
    }
}