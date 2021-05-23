using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


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
    private enum State {
        Ready,
        Pivot,
        Learn,
        Learning,
        Done,
        NewInput
    }

    public Button learnButton;
    public GameObject panelPrefab;

    private State state;
    private Button run, runLearn;
    private GameObject pivot, learn, learning, done;
    private GameObject AttFrame, CompFrame, GenFrame, IncFrame;
    private ARTrackedImageManager arTrackedImageManager;
    private Dictionary<string, Info> dict;
    private Slider slider, sliderLearn;
    private int selAtt, selComp, selGen, selInc;
    private List<Object> list;
    private int currVal, interval;
    private GameObject panelParent;
    private List<int> optimalVal;
    private float currAcc, bestAcc;
    private bool isPerfect;

    private void Awake() {
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        pivot = transform.GetChild(0).gameObject;
        learn = transform.GetChild(1).gameObject;
        panelParent = transform.GetChild(2).gameObject;
        learning = transform.GetChild(3).gameObject;
        done = transform.GetChild(4).gameObject;

        state = State.Ready;
    }

    void Start() {
        dict = FindObjectOfType<ImageRecognition>().dict;
        list = new List<Object>();
        optimalVal = new List<int>();
        currAcc = 0f;
        bestAcc = 0f;

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
                sliderLearn.interactable = true;
            }
            else {
                sliderLearn.interactable = false;
            }

            if(selInc > 0 && sliderLearn.value > 0) {
                runLearn.gameObject.SetActive(true);
            }
            else {
                runLearn.gameObject.SetActive(false);
            }

            string unit = (selAtt == Info.H) ? "cm" : "kg";
            string str;
            if (selInc == Info.inc) {
                str = "증가";
                sliderLearn.interactable = true;
            }
            else if (selInc == Info.dec) {
                str = "감소";
                sliderLearn.interactable = true;
            }
            else {
                str = "";
                sliderLearn.value = sliderLearn.minValue;
                sliderLearn.interactable = false;
            }
            float v = slider.value / 10f;
            float s = sliderLearn.value / 10f;

            string text = v + unit + "부터 " + s + "만큼 " + str + " 하면서 최적값 찾기";
            learn.transform.GetChild(2).GetComponent<Text>().text = text;
        }
        else if(state == State.Learning) {
            foreach(Transform child in panelParent.transform) {
                foreach(var o in list) {
                    if(child.name == o.name) {
                        var plane = o.obj.transform.GetChild(0);
                        child.position = Camera.main.WorldToScreenPoint(plane.position);
                        child.gameObject.SetActive(plane.gameObject.activeSelf);
                        break;
                    }
                }
            }

            string unit = (selAtt == Info.H) ? "cm" : "kg";
            float v = currVal / 10f;
            float a = (int)(currAcc * 1000f) / 10f;
            learning.GetComponentInChildren<Text>().text = "현재값 : " + v + unit + "\n정확도 : " + a + "%";
        }
        else if(state == State.Done) {

        }
        else if(state == State.NewInput) {
            foreach (Transform child in panelParent.transform) {
                foreach (var img in arTrackedImageManager.trackables) {
                    var name = img.referenceImage.name;
                    if (child.name == name) {
                        var plane = img.gameObject.transform.GetChild(0);
                        child.position = Camera.main.WorldToScreenPoint(plane.position);
                        child.gameObject.SetActive(plane.gameObject.activeSelf);
                        break;
                    }
                }
            }
        }
    }
    public void InitPrefab(ARTrackedImage trackedImage) {
        if (state != State.NewInput)
            return;
        InstantiatePanel(trackedImage);
    }
    void InstantiatePanel(ARTrackedImage trackedImage) {
        var name = trackedImage.referenceImage.name;
        if (name[0] != 'm')
            return;
        
        GameObject obj = Instantiate(panelPrefab, panelParent.transform);
        var planeGo = trackedImage.gameObject.transform.GetChild(0);
        var info = dict[name];
        
        int value = (selAtt == Info.H) ? info.height : info.weight;
        int opt = optimalVal[optimalVal.Count / 2];
        int estimate;

        var pos = Camera.main.WorldToScreenPoint(planeGo.position);
        obj.transform.position = pos;
        obj.name = name;

        if ((selComp == Info.gt && value * 10 > opt) || (selComp == Info.lt && value * 10 < opt)) {
            estimate = selGen;
        }
        else {
            estimate = (selGen == Info.Boy ? Info.Girl : Info.Boy);
        }

        if (estimate == info.gender) {
            obj.transform.GetChild(1).gameObject.SetActive(false);
        }
        else {
            obj.transform.GetChild(0).gameObject.SetActive(false);
        }

        string unit = (selAtt == Info.H) ? "cm" : "kg";
        string g = (info.gender == Info.Boy) ? "남자" : "여자";
        string p = (estimate == Info.Boy) ? "남자" : "여자";
        obj.GetComponentInChildren<Text>().text = value + unit + " / " + g + "\n" + "예측 : " + p;
    }
    void OnValueChanged(float value) {
        string unit = "";
        if (selAtt == Info.H)
            unit = "cm";
        else if (selAtt == Info.W)
            unit = "kg";

        slider.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + unit;
    }

    void OnValueChangedLearn(float value) {
        
    }

    void OnClickLearn() {
        //set pivot
        if (state == State.Ready) {
            state = State.Pivot;
            pivot.SetActive(true);
            slider.value = slider.minValue;
            slider.GetComponentInChildren<Text>().text = "";
        }
        else if(state == State.Pivot) {
            state = State.Ready;
            pivot.SetActive(false);
            AttFrame.GetComponentInChildren<Text>().text = "키 / 몸무게";
            CompFrame.GetComponentInChildren<Text>().text = "크다 / 작다";
        }
        else if(state == State.Learn) {
            state = State.Ready;
            learn.SetActive(false);
        }
        else if(state == State.Done) {
            done.SetActive(false);
            if (isPerfect) {
                state = State.NewInput;
                foreach (var trackedImage in arTrackedImageManager.trackables) {
                    InstantiatePanel(trackedImage);
                }
            }
            else {
                state = State.Ready;
            }
        }
    }

    void OnClickRun() {
        if (state == State.Pivot) {
            OnClickLearn();
            state = State.Learn;
            learn.SetActive(true);
            sliderLearn.value = sliderLearn.minValue;
            Eval();
        }
    }
    void OnClickRunLearn() {
        if (state == State.Learn) {
            state = State.Learning;
            learn.SetActive(false);
            learning.SetActive(true);
            currVal = (int)slider.value;
            interval = (int)sliderLearn.value;
            if (selInc == Info.dec) {
                interval *= -1;
            }
            InvokeRepeating("EvalRepeat", 0f, 0.5f);
        }
    }

    void Eval() {
        //register inputs
        foreach (var trackedImage in arTrackedImageManager.trackables) {
            var name = trackedImage.referenceImage.name;
            if (name[0] == 'm') {
                var info = dict[name];
                int i = (selAtt == Info.H) ? info.height : info.weight;
                Object obj = new Object(trackedImage, i, info.gender);
                list.Add(obj);
                Debug.Log("Registered " + name);
            }
        }

        Debug.Log(list.Count + "inputs in evaluation");
    }

    void EvalRepeat() {
        if ((selInc == Info.inc && selAtt == Info.H && currVal > Info.maxH * 10) ||
            (selInc == Info.dec && selAtt == Info.H && currVal < Info.minH * 10) ||
            (selInc == Info.inc && selAtt == Info.W && currVal > Info.maxW * 10) ||
            (selInc == Info.dec && selAtt == Info.W && currVal < Info.maxW * 10)) {
            CancelInvoke();
            FinishLearning();
            return;
        }
        Debug.Log("repeat : " + currVal);
        int correct = 0;
        int total = list.Count;

        foreach(Transform inst in panelParent.transform) {
            Destroy(inst.gameObject);
        }
        
        foreach (var obj in list) {
            int estimate = 0;
            if((selComp == Info.gt && obj.value * 10 > currVal) || (selComp == Info.lt && obj.value * 10 < currVal)) {
                estimate = selGen;
            }
            else {
                estimate = (selGen == Info.Boy ? Info.Girl : Info.Boy);
            }

            var planeGo = obj.obj.transform.GetChild(0).gameObject;
            var personGo = obj.obj.transform.GetChild(1).gameObject;
            var name = obj.name;
            var pos = Camera.main.WorldToScreenPoint(planeGo.transform.position);
            var panel = Instantiate(panelPrefab, panelParent.transform);
            panel.transform.position = pos;
            panel.name = obj.name;
            if(planeGo.activeSelf) {
                panel.SetActive(true);
            }
            else {
                panel.SetActive(false);
            }

            string unit = (selAtt == Info.H) ? "cm" : "kg";
            string g = (obj.gender == Info.Boy) ? "남자" : "여자";
            string p = (estimate == Info.Boy) ? "남자" : "여자";
            panel.GetComponentInChildren<Text>().text = obj.value + unit + " / " + g + "\n" + "예측 : " + p;

            if (obj.gender == estimate) {
                //correct
                ++correct;
                panel.transform.GetChild(1).gameObject.SetActive(false);
            }
            else {
                //incorrect
                panel.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        currAcc = (float)correct / total;

        if(currAcc == bestAcc) {
            optimalVal.Add(currVal);
        }
        else if(currAcc >= bestAcc) {
            optimalVal = new List<int>();
            optimalVal.Add(currVal);
            bestAcc = currAcc;
        }

        currVal += interval;
    }

    void FinishLearning() {
        foreach(Transform child in panelParent.transform) {
            Destroy(child.gameObject);
        }
        learning.SetActive(false);

        state = State.Done;

        done.SetActive(true);
        float solution = optimalVal[optimalVal.Count / 2] / 10f;
        float ba = (int)(bestAcc * 1000f) / 10f;
        string str = "최적값(중간값) : " + solution + "\n최고정확도 : " + ba;

        if(bestAcc == 1f) {
            str += "\n새로운 입력들도 시도해보세요!";
            isPerfect = true;
        }
        else {
            str += "\n다시해보세요...";
        }
        done.GetComponentInChildren<Text>().text = str;
    }
}