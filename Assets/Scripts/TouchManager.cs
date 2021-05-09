using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour {
    public Dictionary<string, Info> dict;
    public int minH, maxH, minW, maxW;
    public GameObject textPrefab;
    public GameObject person, group;
    public Button button;
    public Material blue, pink;
    public GameObject parent;
    private Button H, W, B, G, run;
    private Slider slider;
    private GameObject panel;
    private bool isActive;

    void Start() {
        isActive = false;

        //parent = FindObjectOfType<Canvas>().transform.Find("Instances").gameObject;
        button.onClick.AddListener(OnClick);
        group.SetActive(false);
        person.SetActive(false);
        
        H = group.transform.GetChild(0).GetComponent<Button>();
        H.onClick.AddListener(OnClickAttribute);

        W = group.transform.GetChild(1).GetComponent<Button>();
        W.onClick.AddListener(OnClickAttribute);

        B = group.transform.GetChild(2).GetComponent<Button>();
        B.onClick.AddListener(OnClickAttribute);

        G = group.transform.GetChild(3).GetComponent<Button>();
        G.onClick.AddListener(OnClickAttribute);

        slider = group.transform.GetChild(4).GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);

        panel = group.transform.GetChild(5).gameObject;

        run = group.transform.GetChild(6).GetComponent<Button>();
        run.onClick.AddListener(OnClickRun);
    }

    void Update() {
        if (Input.touchCount > 0) {
            Vector3 touch = Input.GetTouch(0).position;
            Ray ray = Camera.main.ScreenPointToRay(touch);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                if (Input.GetTouch(0).phase == TouchPhase.Began) {
                    var other = hit.collider.gameObject;
                    var name = other.name;
                    Debug.Log("hit : " + name);
                    ClearUI();
                    if (name[0] == 'm') {
                        DisplayInfo(touch, dict[name], name);
                    }
                }
            }
            else {
                ClearUI();
            }
        }
    }

    void ClearUI() {
        Debug.Log("Clear");
        foreach(Transform child in parent.transform) {
            Destroy(child.gameObject);
        }
    }
    void DisplayInfo(Vector3 pos, Info info, string name) {
        Debug.Log("Display info of " + name);
        GameObject obj = Instantiate(textPrefab, parent.transform, false);
        //GameObject obj = Instantiate(textPrefab, pos, Quaternion.identity);
        
        obj.transform.position = pos;
        obj.name = name;

        var h = obj.transform.GetChild(1);
        var w = obj.transform.GetChild(3);
        
        h.GetComponentInChildren<Text>().text = info.height + "cm";
        w.GetComponentInChildren<Text>().text = info.weight + "kg";
    }
    void OnClickAttribute() {
        //Instantiate prefab to decide wheter increase/decrease
        //Set current value as pivot value
        var obj = EventSystem.current.currentSelectedGameObject;
        Debug.Log("Selected " + obj.name);
        if (obj.name == "H") {
            H.interactable = false;
            W.interactable = true;

            slider.maxValue = maxH;
            slider.minValue = minH;
            slider.value = slider.minValue;
        }
        else if (obj.name == "W") {
            H.interactable = true;
            W.interactable = false;

            slider.maxValue = maxW;
            slider.minValue = minW;
            slider.value = slider.minValue;
        }
        else if (obj.name == "B") {
            B.interactable = false;
            G.interactable = true;
            
            person.GetComponentInChildren<MeshRenderer>().material = blue;
        }
        else if (obj.name == "G") {
            B.interactable = true;
            G.interactable = false;

            person.GetComponentInChildren<MeshRenderer>().material = pink;
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
        if(isActive) {
            person.transform.localScale = new Vector3(minW / 25f, minH / 75f, minW / 25f);
        }
        person.SetActive(isActive);
        group.SetActive(isActive);
    }

    void OnValueChanged(float value) {
        Vector3 scale = person.transform.localScale;
        string text = "" + value;
        if (!H.interactable) {
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
        var run = FindObjectOfType<ImageRecognition>();
        OnClick();
        if (!H.interactable) {
            if(!B.interactable)
                run.Run("H", true);
            else if(!G.interactable)
                run.Run("H", false);
        }
        else if (!W.interactable)
            if (!B.interactable)
                run.Run("W", true);
            else if (!G.interactable)
                run.Run("W", false);
    }
}
