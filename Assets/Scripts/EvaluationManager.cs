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
    private Text valText;
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
    private int minBound, maxBound;
    private float repeatSpeed = 1.0f;
    private GameObject content;
    private TouchManager tm;
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
        tm = FindObjectOfType<TouchManager>();
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
        valText = pivot.transform.GetChild(7).GetComponent<Text>();

        //learn
        learn.SetActive(false);
        IncFrame = learn.transform.GetChild(0).gameObject;

        sliderLearn = learn.transform.GetChild(1).GetComponent<Slider>();
        sliderLearn.onValueChanged.AddListener(OnValueChangedLearn);

        runLearn = learn.transform.GetComponentInChildren<Button>();
        runLearn.onClick.AddListener(OnClickRunLearn);

        //learning
        content = learning.transform.GetChild(1).GetChild(0).GetChild(0).gameObject;
    }
    int CheckValid(GameObject frame, int type) {
        Ray ray = Camera.main.ScreenPointToRay(frame.transform.position);
        RaycastHit hit;
        var valid = frame.transform.GetChild(1).gameObject;
        string placeholder, type1, type2;
        string type3 = "", type4 = "";

        switch(type) {
            case 1: //attribute
                type1 = "qrh";
                type2 = "qrw";
                type3 = "qrhead";
                type4 = "qrwaist";
                placeholder = "속성";
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
            if (other.name == null || other.name == "")
                return -1;

            valid.SetActive(true);
            frame.GetComponentInChildren<Text>().text = "";

            if (other.name == type1) {
                return 1;
            }
            else if(other.name == type2) {
                return 2;
            }
            else if(other.name == type3) {
                return 3;
            }
            else if(other.name == type4) {
                return 4;
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

            if(selAtt == Info.Height) {
                if(slider.interactable == false) {
                    slider.interactable = true;
                }
                if(slider.minValue != Info.minHeight * 10) {
                    slider.minValue = Info.minHeight * 10;
                    slider.maxValue = Info.maxHeight * 10;
                    slider.value = slider.minValue;
                    slider.gameObject.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + "cm";
                }
            }
            else if(selAtt == Info.Weight) {
                if(slider.interactable == false) {
                    slider.interactable = true;
                }
                if (slider.minValue != Info.minWeight * 10) {
                    slider.minValue = Info.minWeight * 10;
                    slider.maxValue = Info.maxWeight * 10;
                    slider.value = slider.minValue;
                    slider.gameObject.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + "kg";
                }
            }
            else if(selAtt == Info.Head) {
                if (slider.interactable == false) {
                    slider.interactable = true;
                }
                if (slider.minValue != Info.minHead * 10) {
                    slider.minValue = Info.minHead * 10;
                    slider.maxValue = Info.maxHead * 10;
                    slider.value = slider.minValue;
                    slider.gameObject.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + "cm";
                }
            }
            else if(selAtt == Info.Waist) {
                if (slider.interactable == false) {
                    slider.interactable = true;
                }
                if (slider.minValue != Info.minWaist * 10) {
                    slider.minValue = Info.minWaist * 10;
                    slider.maxValue = Info.maxWaist * 10;
                    slider.value = slider.minValue;
                    slider.gameObject.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + "cm";
                }
            }
            else {
                if(slider.interactable == true) {
                    slider.gameObject.GetComponentInChildren<Text>().text = "";
                    slider.interactable = false;
                }
            }

            SetValText();

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

            string unit = (selAtt == Info.Weight) ? "kg" : "cm";
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

            string text = v + unit + "부터 " + s + "만큼 " + str + " 하면서 최적값 찾아보기";
            learn.transform.GetChild(3).GetComponent<Text>().text = text;
        }
        else if(state == State.Learning) {
            //update panel position
            foreach (Transform child in panelParent.transform) {
                foreach(var o in list) {
                    if(child.name == o.name) {
                        var plane = o.obj.transform.GetChild(0);
                        child.position = Camera.main.WorldToScreenPoint(plane.position);
                        child.gameObject.SetActive(plane.gameObject.activeSelf);
                        break;
                    }
                }
            }

            //update text
            string unit = (selAtt == Info.Weight) ? "kg" : "cm";
            float v = currVal / 10f;
            float a = (int)(currAcc * 1000f) / 10f;
            learning.GetComponentInChildren<Text>().text = "현재값 : " + v + unit + "\n정확도 : " + a + "%";
        }
        else if(state == State.Done) {

        }
        else if(state == State.NewInput) {
            //update panel position
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

        int value = 0;
        if (selAtt == Info.Height) {
            value = info.height;
        }
        else if (selAtt == Info.Weight) {
            value = info.weight;
        }
        else if (selAtt == Info.Head) {
            value = info.head;
        }
        else if (selAtt == Info.Waist) {
            value = info.waist;
        }
        //int value = (selAtt == Info.Height) ? info.height : info.weight;
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

        string unit = (selAtt == Info.Weight) ? "kg" : "cm";
        string g = (info.gender == Info.Boy) ? "남자" : "여자";
        string p = (estimate == Info.Boy) ? "남자" : "여자";
        obj.GetComponentInChildren<Text>().text = value + unit + " / " + g + "\n" + "예측 : " + p;
    }
    void OnValueChanged(float value) {
        string unit = "";
        if (selAtt == Info.Height || selAtt == Info.Head || selAtt == Info.Waist)
            unit = "cm";
        else if (selAtt == Info.Weight)
            unit = "kg";

        slider.GetComponentInChildren<Text>().text = "" + (slider.value / 10) + unit;
    }

    void SetValText() {
        string att = "(속성)";
        string v = "()";
        string com = "(크면/작으면)";
        string g = "(남자/여자)";

        switch(selAtt) {
            case Info.Height:
                att = "키";
                break;
            case Info.Weight:
                att = "몸무게";
                break;
            case Info.Head:
                att = "머리둘레";
                break;
            case Info.Waist:
                att = "허리둘레";
                break;
        }

        if(slider.interactable) {
            v = "" + (slider.value / 10f);
        }

        switch(selComp) {
            case Info.gt:
                com = "크면";
                break;
            case Info.lt:
                com = "작으면";
                break;
        }

        switch(selGen) {
            case Info.Boy:
                g = "남자";
                break;
            case Info.Girl:
                g = "여자";
                break;
        }

        valText.text = att + "가 " + v + "보다 " + com + " " + g + "입니다";
    }

    void OnValueChangedLearn(float value) {
        
    }

    public void OnClickLearn() {
        tm.ExclusiveButton(TouchManager.ButtonState.Learn);
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
            AttFrame.GetComponentInChildren<Text>().text = "속성";
            CompFrame.GetComponentInChildren<Text>().text = "크다 / 작다";
        }
        else if(state == State.Learn) {
            state = State.Ready;
            learn.SetActive(false);
        }
        else if(state == State.Done) {
            done.SetActive(false);
            learning.SetActive(false);
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
        else if(state == State.NewInput) {
            
        }
    }

    void OnClickRun() {
        if (state == State.Pivot) {
            if(!Eval()) {
                return;
            }
            pivot.SetActive(false);
            AttFrame.GetComponentInChildren<Text>().text = "속성";
            CompFrame.GetComponentInChildren<Text>().text = "크다 / 작다";

            state = State.Learn;
            learn.SetActive(true);
            sliderLearn.value = sliderLearn.minValue;
            minBound = (int)(slider.minValue / 10f);
            maxBound = (int)(slider.maxValue / 10f);
        }
    }
    void OnClickRunLearn() {
        if (state == State.Learn) {
            ClearTable();
            state = State.Learning;
            learn.SetActive(false);
            learning.SetActive(true);
            currVal = (int)slider.value;
            interval = (int)sliderLearn.value;
            if (selInc == Info.dec) {
                interval *= -1;
            }
            string text = "";
            switch (selAtt) {
                case Info.Height:
                    text = "키";
                    break;
                case Info.Weight:
                    text = "몸무게";
                    break;
                case Info.Head:
                    text = "머리둘레";
                    break;
                case Info.Waist:
                    text = "허리둘레";
                    break;
            }
            learning.transform.GetChild(1).GetChild(3).GetChild(1).GetComponent<Text>().text = text;
            InvokeRepeating("EvalRepeat", 0f, repeatSpeed);
        }
    }

    bool Eval() {
        //register inputs
        list = new List<Object>();
        foreach (var trackedImage in arTrackedImageManager.trackables) {
            var name = trackedImage.referenceImage.name;
            if (name[0] == 'm') {
                var info = dict[name];
                int i = 0;
                switch (selAtt) {
                    case Info.Height:
                        i = info.height;
                        break;
                    case Info.Weight:
                        i = info.weight;
                        break;
                    case Info.Head:
                        i = info.head;
                        break;
                    case Info.Waist:
                        i = info.waist;
                        break;
                }
                Object obj = new Object(trackedImage, i, info.gender);
                list.Add(obj);
                Debug.Log("Registered " + name);
            }
        }

        Debug.Log(list.Count + "inputs in evaluation");
        return list.Count > 0;
    }

    void EvalRepeat() {
        if ((selInc == Info.inc && currVal > maxBound * 10) || (selInc == Info.dec && currVal < minBound * 10)) {
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

            string unit = (selAtt == Info.Weight) ? "kg" : "cm";
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

        InsertTable();
        UpdateTable();

        if(currAcc == 1f) {
            //stop when 100%
            FinishLearning();
            return;
        }
        currVal += interval;
    }

    void InsertTable() {
        Debug.Log("Insert: ");
        var row = content.transform.GetChild(0).gameObject;
        var i = Instantiate(row, content.transform);
        string unit = (selAtt == Info.Weight) ? "kg" : "cm";
        Debug.Log(i.transform.position);
        i.transform.GetChild(1).GetComponent<Text>().text = "" + (currVal / 10f) + unit;
        i.transform.GetChild(2).GetComponent<Text>().text = "" + ((int)(currAcc * 1000f) / 10f) + "%";
        i.SetActive(true);
    }

    void UpdateTable() {
        var accstr = "" + ((int)(currAcc * 1000f) / 10f) + "%";
        foreach (Transform child in content.transform) {
            if (child.gameObject.activeSelf) {
                Color c;
                if (child.GetChild(2).GetComponent<Text>().text == accstr) {
                    c = Color.green;
                }
                else {
                    c = Color.white;
                }
                c.a = 0.5f;

                child.GetChild(0).GetComponent<Image>().color = c;
            }
        }
    }

    void ClearTable() {
        foreach(Transform child in content.transform) {
            if(child.gameObject.activeSelf) {
                Destroy(child.gameObject);
            }
        }
    }
    void FinishLearning() {
        CancelInvoke();
        foreach (Transform child in panelParent.transform) {
            Destroy(child.gameObject);
        }
        //learning.SetActive(false);
        learning.transform.GetChild(0).gameObject.SetActive(false);

        state = State.Done;

        done.SetActive(true);
        float solution = optimalVal[optimalVal.Count / 2] / 10f;
        float ba = (int)(bestAcc * 1000f) / 10f;
        string str = "최적값 : " + solution + "\n최고정확도 : " + ba;

        if(bestAcc == 1f) {
            str += "\n새로운 입력들도 시도해보세요!";
            isPerfect = true;
        }
        else {
            str += "\n다시해보세요...";
        }
        done.GetComponentInChildren<Text>().text = str;
    }

    public void SetSpeed(float s) {
        repeatSpeed = s;
    }
}