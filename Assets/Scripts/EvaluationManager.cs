using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


enum State {
    Ready,
    Pivot,
    Learn,
    Learning,
    Done
}

struct Object {
    public string name;
    public GameObject obj;
    public int value, gender;

    public Object(ARTrackedImage trackedImage, int val, int gen) {
        name = trackedImage.referenceImage.name;
        value = val;
        gender = gen;
        obj = trackedImage.gameObject;
    }
}

public class EvaluationManager : MonoBehaviour {
    public Button learnButton;

    private State state;
    private Button run, runLearn;
    private GameObject pivot, learn;
    private GameObject AttFrame, CompFrame, GenFrame, IncFrame;
    private ARTrackedImageManager arTrackedImageManager;
    private Dictionary<string, Info> dict;
    private Dictionary<string, Info> localDict;
    private Slider slider, sliderLearn;
    private int selAtt, selComp, selGen, selInc;
    private List<Object> list;
    private int currVal, interval;

    private void Awake() {
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        pivot = transform.GetChild(0).gameObject;
        learn = transform.GetChild(1).gameObject;
    }

    void Start() {
        //pivot
        pivot.SetActive(false);

        learnButton.onClick.AddListener(OnClickLearn);
        
        AttFrame = pivot.transform.GetChild(0).gameObject;
        CompFrame = pivot.transform.GetChild(1).gameObject;
        GenFrame = pivot.transform.GetChild(2).gameObject;
        
        slider = pivot.transform.GetChild(3).GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
        
        run = pivot.transform.GetChild(4).GetComponent<Button>();
        run.onClick.AddListener(OnClickRun);

        //learn
        learn.SetActive(false);
        IncFrame = learn.transform.GetChild(0).gameObject;
        sliderLearn = learn.transform.GetChild(1).GetComponent<Slider>();
        sliderLearn.onValueChanged.AddListener(OnValueChangedLearn);

        runLearn = learn.transform.GetComponentInChildren<Button>();
        runLearn.onClick.AddListener(OnClickRunLearn);
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

            case 4: // inc
                type1 = "qrinc";
                type2 = "qrdec";
                placeholder = "증가 / 감소";
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
        if (state == State.Pivot) {
            selAtt = CheckValid(AttFrame, 1);
            selComp = CheckValid(CompFrame, 2);
            selGen = CheckValid(GenFrame, 3);

            if(selAtt == Info.H) {
                if(slider.interactable == false) {
                    slider.interactable = true;
                }
                if(slider.minValue != Info.minH * 10) {
                    slider.minValue = Info.minH * 10;
                    slider.maxValue = Info.maxH * 10;
                    slider.value = slider.minValue;
                    slider.gameObject.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + "cm";
                }
            }
            else if(selAtt == Info.W) {
                if(slider.interactable == false) {
                    slider.interactable = true;
                }
                if (slider.minValue != Info.minW * 10) {
                    slider.minValue = Info.minW * 10;
                    slider.maxValue = Info.maxW * 10;
                    slider.value = slider.minValue;
                    slider.gameObject.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + "kg";
                }
            }
            else {
                if(slider.interactable == true) {
                    slider.gameObject.GetComponentInChildren<Text>().text = "";
                    slider.interactable = false;
                }
            }

            if(selAtt > 0 && selComp > 0 && selGen > 0) {
                run.gameObject.SetActive(true);
            }
            else {
                run.gameObject.SetActive(false);
            }
        }
        else if(state == State.Learn) {
            selInc = CheckValid(IncFrame, 4);
            if(selInc > 0) {
                runLearn.gameObject.SetActive(true);
            }
            else {
                runLearn.gameObject.SetActive(false);
            }

        }
    }

    void OnValueChanged(float value) {
        string unit = "";
        if (selAtt == Info.H)
            unit = "cm";
        else if (selAtt == Info.W)
            unit = "kg";

        slider.gameObject.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + unit;
    }

    void OnValueChangedLearn(float value) {

    }

    void OnClickLearn() {
        //set pivot
        if (state == State.Ready) {
            state = State.Pivot;
            pivot.SetActive(true);
        }
        else if(state == State.Pivot) {
            state = State.Ready;
            pivot.SetActive(false);
            AttFrame.GetComponentInChildren<Text>().text = "키 / 몸무게";
            CompFrame.GetComponentInChildren<Text>().text = "크다 / 작다";
        }
        else if(state == State.Learn) {

        }
    }

    void OnClickRun() {
        if (state == State.Pivot) {
            OnClickLearn();
            state = State.Learn;
            learn.SetActive(true);
            Eval(selAtt, selComp, selGen, slider.value);
        }
    }

    void Eval(int att, int comp, int gen, float val) {
        //register inputs
        foreach (var trackedImage in arTrackedImageManager.trackables) {
            var name = trackedImage.referenceImage.name;
            if (name[0] == 'm') {
                var info = dict[name];
                int i = (att == Info.H) ? info.height : info.weight;
                Object obj = new Object(trackedImage, i, info.gender);
                list.Add(obj);
            }
        }
    }

    void EvalRepeat() {
        if((selInc == Info.inc && selAtt == Info.H && currVal > Info.maxH * 10) || 
            (selInc == Info.dec && selAtt == Info.H && currVal < Info.minH * 10) ||
            (selInc == Info.inc && selAtt == Info.W && currVal > Info.maxW * 10) ||
            (selInc == Info.dec && selAtt == Info.W && currVal < Info.maxW * 10)) {
            CancelInvoke();
            return;
        }

        else if (selInc == Info.dec && selAtt == Info.H && currVal < Info.minH * 10) {

        }

        foreach (var obj in list) {

        }
        currVal += interval;
    }

    void OnClickRunLearn() {
        if(state == State.Learn) {
            state = State.Learning;
            learn.SetActive(false);

            currVal = (int)slider.value;
            interval = (int)sliderLearn.value;

        }

        //InvokeRepeating("EvalRepeat", 0f, 1f);
    }
}