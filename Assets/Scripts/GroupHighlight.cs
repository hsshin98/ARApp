using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GroupHighlight : MonoBehaviour {
    [SerializeField]
    ARTrackedImageManager arTrackedImageManager;

    public GameObject person;
    public Button button;
    public Material boy, girl, both;
    public Material boyTransparent, girlTransparent;

    private int minH, maxH, minW, maxW;
    private int selAttribute, selComp, selGender;
    private Button att, comp, gender, run;
    private Slider slider;
    private GameObject panel;
    private bool isActive, isHighlighted;
    public Dictionary<string, Info> dict;
    void Start() {
        isActive = false;
        isHighlighted = false;

        button.onClick.AddListener(OnClick);

        selAttribute = -1;
        selComp = -1;
        selGender = 0;

        minH = 90;
        maxH = 190;
        minW = 20;
        maxW = 100;

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

        gameObject.SetActive(false);
        person.SetActive(false);
    }

    void Update() {
        
    }


    void OnClickAtt() {
        var obj = EventSystem.current.currentSelectedGameObject;
        if(selAttribute != 1) {
            //1 = height
            obj.GetComponentInChildren<Text>().text = "키";
            selAttribute = 1;

            slider.maxValue = maxH * 10;
            slider.minValue = minH * 10;
            slider.value = slider.minValue;
        }
        else if(selAttribute != 2) {
            //2 = weight
            obj.GetComponentInChildren<Text>().text = "몸무게";
            selAttribute = 2;

            slider.maxValue = maxW * 10;
            slider.minValue = minW * 10;
            slider.value = slider.minValue;
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

    void OnClick() {
        if(isHighlighted) {
            isHighlighted = false;
            DisableHighlight();
            return;
        }

        isActive = !isActive;
        if (isActive)
            person.transform.localScale = new Vector3(minW / 25f, minH / 75f, minW / 25f);
        person.SetActive(isActive);
        gameObject.SetActive(isActive);
    }

    void OnValueChanged(float value) {
        Vector3 scale = person.transform.localScale;
        string text = "" + value / 10f;
        if (selAttribute == 1) {
            text += "cm";
            scale.y = value / 750f;
        }
        else {
            text += "kg";
            scale.x = value / 250f;
            scale.z = value / 250f;
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
        att.GetComponentInChildren<Text>().text = "키/\n몸무게";
        comp.GetComponentInChildren<Text>().text = "이상/\n이하";
        gender.GetComponentInChildren<Text>().text = "남자/여자";

        selAttribute = -1;
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
            var name = personGo.transform.GetChild(0).name;
            var info = dict[name];
            Material mat = (info.gender == Info.Boy) ? boyTransparent : girlTransparent;
            if (selGender == 0 || (selGender == Info.Boy && info.gender == Info.Boy) || (selGender == Info.Girl && info.gender == Info.Girl)) {
                if (selAttribute == Info.H) {
                    if ((selComp == Info.gt && info.height * 10 < value) || (selComp == Info.lt && info.height * 10 > value)) {
                        personGo.GetComponentInChildren<MeshRenderer>().material = mat;
                    }
                }
                else if (selAttribute == Info.W) {
                    if ((selComp == Info.gt && info.weight * 10 < value) || (selComp == Info.lt && info.weight * 10 > value)) {
                        personGo.GetComponentInChildren<MeshRenderer>().material = mat;
                    }
                }
            }
            else if((selGender == Info.Girl && info.gender == Info.Boy) || (selGender == Info.Boy && info.gender == Info.Girl)) {
                personGo.GetComponentInChildren<MeshRenderer>().material = mat;
            }
            
        }
        OnClick();
        isHighlighted = true;
    }

    void DisableHighlight() {
        foreach (var trackedImage in arTrackedImageManager.trackables) {
            var personGo = trackedImage.transform.GetChild(1).gameObject;
            var name = personGo.transform.GetChild(0).name;
            var info = dict[name];

            personGo.GetComponentInChildren<MeshRenderer>().material = (info.gender == Info.Boy) ? boy : girl;
        }
    }
}
