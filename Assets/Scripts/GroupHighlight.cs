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
    private bool isActive;
    public Dictionary<string, Info> dict;
    void Start() {
        isActive = false;

        button.onClick.AddListener(OnClick);
        person.SetActive(false);

        selAttribute = -1;
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
    }

    void Update() {
        
    }

    public void SetBounds(int minh, int maxh, int minw, int maxw) {
        minH = minh;
        maxH = maxh;
        minW = minw;
        maxW = maxw;
        gameObject.SetActive(false);
        Debug.Log("bounds set: " + minH + "~" + maxH);
    }

    void OnClickAtt() {
        var obj = EventSystem.current.currentSelectedGameObject;
        if(selAttribute != 1) {
            //1 = height
            obj.GetComponentInChildren<Text>().text = "키";
            selAttribute = 1;

            slider.maxValue = maxH;
            slider.minValue = minH;
            slider.value = slider.minValue;
        }
        else if(selAttribute != 2) {
            //2 = weight
            obj.GetComponentInChildren<Text>().text = "몸무게";
            selAttribute = 2;

            slider.maxValue = maxW;
            slider.minValue = minW;
            slider.value = slider.minValue;
        }
    }

    void OnClickComp() {
        var obj = EventSystem.current.currentSelectedGameObject;
        if (selComp != 1) {
            //1 = greater than equal
            obj.GetComponentInChildren<Text>().text = "이상";
            selComp = 1;
        }
        else if (selComp != 2) {
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
            selGender  = 1;
            person.GetComponentInChildren<MeshRenderer>().material = boy;
        }
        else if (selGender == 1) {
            //1 : boy -> 2 : girl
            obj.GetComponentInChildren<Text>().text = "여자";
            selGender = 2;
            person.GetComponentInChildren<MeshRenderer>().material = girl;
        }
        else if(selGender == 2) {
            //2 : girl -> 0 : both
            obj.GetComponentInChildren<Text>().text = "남자/여자";
            selGender = 0;
            person.GetComponentInChildren<MeshRenderer>().material = both;
        }
    }

    void OnClick() {
        var run = FindObjectOfType<ImageRecognition>();
        if (run.isDone) {
            Debug.Log("Done");
            run.result.SetActive(false);
            run.isDone = false;
            return;
        }
        isActive = !isActive;
        if (isActive) {
            person.transform.localScale = new Vector3(minW / 25f, minH / 75f, minW / 25f);
        }
        person.SetActive(isActive);
        gameObject.SetActive(isActive);
    }

    void OnValueChanged(float value) {
        Vector3 scale = person.transform.localScale;
        string text = "" + value;
        if (selAttribute == 1) {
            text += "cm";
            scale.y = value / 75f;
        }
        else {
            text += "kg";
            scale.x = value / 25f;
            scale.z = value / 25f;
        }
        panel.GetComponentInChildren<Text>().text = text;
        person.transform.localScale = scale;
    }

    void OnClickRun() {
        if (selAttribute != -1 && selComp != -1 && selGender != -1) {
            Debug.Log("Highlight");
            Highlight();
        }
    }

    void InitButton() {

    }

    void Highlight() {
        foreach(var trackedImage in arTrackedImageManager.trackables) {
            SetTransparent(trackedImage);
        }
    }

    void SetTransparent(ARTrackedImage trackedImage) {
        var personGo = trackedImage.transform.GetChild(1).gameObject;
        var name = personGo.transform.GetChild(0).name;
        var mat = dict[name].gender ? boyTransparent : girlTransparent;
        
        personGo.GetComponentInChildren<MeshRenderer>().material = mat;
    }

    void SetOpaque(ARTrackedImage trackedImage) {
        var personGo = trackedImage.transform.GetChild(1).gameObject;
        var name = personGo.transform.GetChild(0).name;
        var mat = dict[name].gender ? boy : girl;

        personGo.GetComponentInChildren<MeshRenderer>().material = mat;
    }
}
