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

    void Update() {
        if (isActive) {
            Ray rayatt = Camera.main.ScreenPointToRay(AttFrame.transform.position);
            RaycastHit hitatt;
            string atttext = "";
            if(Physics.Raycast(rayatt, out hitatt, Mathf.Infinity)) {
                var other = hitatt.collider.gameObject;
                if (other.name == "qrh") {
                    selAtt = 1;
                }
                else if (other.name == "qrw") {
                    selAtt = 2;
                }
                else {
                    //invalid hit
                    selAtt = -1;
                    atttext += "키 / 몸무게";
                }
            }
            else {
                selAtt = -1;
                atttext += "키 / 몸무게";
            }
            AttFrame.GetComponentInChildren<Text>().text = atttext;

            Ray raycomp = Camera.main.ScreenPointToRay(CompFrame.transform.position);
            RaycastHit hitcomp;
            string comptext = "";
            if (Physics.Raycast(raycomp, out hitcomp, Mathf.Infinity)) {
                var other = hitcomp.collider.gameObject;
                if (other.name == "qrgt") {
                    selComp = 1;
                }
                else if (other.name == "qrlt") {
                    selComp = 2;
                }
                else {
                    //invalid hit
                    selAtt = -1;
                    comptext += "크다 / 작다";
                }
            }
            else {
                selAtt = -1;
                comptext += "크다 / 작다";
            }
            CompFrame.GetComponentInChildren<Text>().text = comptext;

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