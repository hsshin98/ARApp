using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GroupHighlight : MonoBehaviour {
    public GameObject person;
    public Button button;
    public Material boy, girl, both;
    public Material boyTransparent, girlTransparent;

    private int selAttribute, selComp, selGender;
    private Button att, comp, gender, run;
    private Slider slider;
    private GameObject panel;
    private bool isActive, isHighlighted;
    private ARTrackedImageManager arTrackedImageManager;
    private Dictionary<string, Info> dict;
    private TouchManager tm;

    private void Awake() {
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    void Start() {
        tm = FindObjectOfType<TouchManager>();

        isActive = false;
        isHighlighted = false;

        button.onClick.AddListener(OnClick);

        selAttribute = 0;
        selComp = -1;
        selGender = 0;

        //initialize objects
        foreach(Transform child in transform) {
            switch(child.name) {
                case "Slider":
                    slider = child.GetComponent<Slider>();
                    slider.onValueChanged.AddListener(OnValueChanged);
                    break;

                case "Panel":
                    panel = child.gameObject;
                    break;

                case "Run":
                    run = child.GetComponent<Button>();
                    run.onClick.AddListener(OnClickRun);
                    break;

                case "Att":
                    att = child.GetComponent<Button>();
                    att.onClick.AddListener(OnClickAtt);
                    break;

                case "Comp":
                    comp = child.GetComponent<Button>();
                    comp.onClick.AddListener(OnClickComp);
                    break;

                case "Gender":
                    gender = child.GetComponent<Button>();
                    gender.onClick.AddListener(OnClickGender);
                    break;
            }
        }

        dict = FindObjectOfType<ImageRecognition>().dict;
        slider.interactable = false;

        gameObject.SetActive(false);
        person.SetActive(false);
    }

    void Update() {
        
    }


    void OnClickAtt() {
        var obj = EventSystem.current.currentSelectedGameObject;
        string text;
        selAttribute = 1 + (selAttribute) % 4;
        switch(selAttribute) {
            case Info.Height:
                slider.maxValue = Info.maxHeight * 10;
                slider.minValue = Info.minHeight * 10;
                text = "키";
                break;
            case Info.Weight:
                slider.maxValue = Info.maxWeight * 10;
                slider.minValue = Info.minWeight * 10;
                text = "몸무게";
                break;
            case Info.Head:
                slider.maxValue = Info.maxHead * 10;
                slider.minValue = Info.minHead * 10;
                text = "머리둘레";
                break;
            case Info.Waist:
                slider.maxValue = Info.maxWaist * 10;
                slider.minValue = Info.minWaist * 10;
                text = "허리둘레";
                break;
            default:
                text = "속성";
                break;
        }

        obj.GetComponentInChildren<Text>().text = text;

        if (selAttribute > 0) {
            slider.value = slider.minValue;
            slider.interactable = true;
        }
        else {
            slider.value = slider.minValue;
            panel.GetComponentInChildren<Text>().text = "";
            slider.interactable = false;
        }
    }

    void OnClickComp() {
        var obj = EventSystem.current.currentSelectedGameObject;
        if (selComp != Info.gt) {
            //1 = greater than equal
            obj.GetComponentInChildren<Text>().text = "이상";
            selComp = 1;
        }
        else if (selComp != Info.lt) {
            //2 = less than equal
            obj.GetComponentInChildren<Text>().text = "이하";
            selComp = 2;
        }
    }

    void OnClickGender() {
        var obj = EventSystem.current.currentSelectedGameObject;
        if (selGender == 0) {
            //0 : both -> 1 : boy
            obj.GetComponentInChildren<Text>().text = "남자";
            selGender  = Info.Boy;
            person.GetComponentInChildren<MeshRenderer>().material = boy;
        }
        else if (selGender == Info.Boy) {
            //1 : boy -> 2 : girl
            obj.GetComponentInChildren<Text>().text = "여자";
            selGender = Info.Girl;
            person.GetComponentInChildren<MeshRenderer>().material = girl;
        }
        else if(selGender == Info.Girl) {
            //2 : girl -> 0 : both
            obj.GetComponentInChildren<Text>().text = "남자/여자";
            selGender = 0;
            person.GetComponentInChildren<MeshRenderer>().material = both;
        }
    }

    public void OnClick() {
        tm.ExclusiveButton(TouchManager.ButtonState.Group);
        if(isHighlighted) {
            isHighlighted = false;
            DisableHighlight();
            return;
        }

        isActive = !isActive;
        if (isActive) {
            person.transform.localScale = new Vector3(Info.minWeight / 25f, Info.minHeight / 75f, Info.minWeight / 25f);
            InitButton();
        }
        person.SetActive(isActive);
        gameObject.SetActive(isActive);
    }

    void OnValueChanged(float value) {
        Vector3 scale = person.transform.localScale;
        string text = "" + value / 10f;

        switch (selAttribute) {
            case Info.Height:
                text += "cm";
                scale.y = value / 750f;
                break;
            case Info.Weight:
                scale.x = value / 250f;
                scale.z = value / 250f;
                text += "kg";
                break;
            case Info.Head:
                text += "cm";
                break;
            case Info.Waist:
                text += "cm";
                break;
            default:
                text = "";
                break;
        }

        panel.GetComponentInChildren<Text>().text = text;
        person.transform.localScale = scale;
    }

    void OnClickRun() {
        if (selAttribute != -1 && selComp != -1 && selGender != -1) {
            Debug.Log("Highlight");
            Highlight();
            InitButton();
        }
    }

    void InitButton() {
        att.GetComponentInChildren<Text>().text = "속성";
        comp.GetComponentInChildren<Text>().text = "이상/\n이하";
        gender.GetComponentInChildren<Text>().text = "남자/여자";

        selAttribute = 0;
        selComp = -1;
        selGender = 0;
        person.GetComponentInChildren<MeshRenderer>().material = both;

        slider.value = slider.minValue;
        panel.GetComponentInChildren<Text>().text = "";
    }

    void Highlight() {
        int value = (int)slider.value;
        Debug.Log("Att: " + selAttribute + ", Gen: " + selGender + "selComp: " + selComp);
        foreach (var trackedImage in arTrackedImageManager.trackables) {
            var personGo = trackedImage.transform.GetChild(1).gameObject;
            var name = trackedImage.referenceImage.name;
            
            if (name[0] == 'q')
                continue;

            var info = dict[name];
            Material mat = (info.gender == Info.Boy) ? boyTransparent : girlTransparent;
            if (selGender == 0 || (selGender == Info.Boy && info.gender == Info.Boy) || (selGender == Info.Girl && info.gender == Info.Girl)) {
                int val = 0;
                switch(selAttribute) {
                    case Info.Height:
                        val = info.height;
                        break;
                    case Info.Weight:
                        val = info.weight;
                        break;
                    case Info.Head:
                        val = info.head;
                        break;
                    case Info.Waist:
                        val = info.waist;
                        break;
                }
                if ((selComp == Info.gt && val * 10 < value) || (selComp == Info.lt && val * 10 > value)) {
                    personGo.GetComponentInChildren<MeshRenderer>().material = mat;
                }
            }
            else if((selGender == Info.Girl && info.gender == Info.Boy) || (selGender == Info.Boy && info.gender == Info.Girl)) {
                personGo.GetComponentInChildren<MeshRenderer>().material = mat;
            }
            
        }

        person.SetActive(false);
        gameObject.SetActive(false);
        isHighlighted = true;
    }

    void DisableHighlight() {
        foreach (var trackedImage in arTrackedImageManager.trackables) {
            var personGo = trackedImage.transform.GetChild(1).gameObject;
            var name = trackedImage.referenceImage.name;
            if (name[0] == 'q')
                continue;
            var info = dict[name];

            personGo.GetComponentInChildren<MeshRenderer>().material = (info.gender == Info.Boy) ? boy : girl;
        }
    }
}
