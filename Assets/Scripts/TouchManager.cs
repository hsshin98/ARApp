using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour {
    public Dictionary<string, Info> dict;
    public GameObject textPrefab;
    public GameObject parent;
    private bool isCleared;

    private ChartManager chart;
    private MenuManager menu;
    private GroupHighlight group;
    private EvaluationManager learn;
    private ButtonState curr = ButtonState.None;
    public enum ButtonState {
        None,
        Chart,
        Menu,
        Group,
        Learn
    }

    private void Awake() {
        chart = FindObjectOfType<ChartManager>();
        menu = FindObjectOfType<MenuManager>();
        group = FindObjectOfType<GroupHighlight>();
        learn = FindObjectOfType<EvaluationManager>();
    }
    void Start() {
        isCleared = true;

        dict = FindObjectOfType<ImageRecognition>().dict;
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
                if (!isCleared) {
                    ClearUI();
                }
            }
        }
    }

    void ClearUI() {
        foreach(Transform child in parent.transform) {
            Destroy(child.gameObject);
        }
        isCleared = true;
    }
    void DisplayInfo(Vector3 pos, Info info, string name) {
        isCleared = false;
        Debug.Log("Display info of " + name);
        GameObject obj = Instantiate(textPrefab, parent.transform, false);

        obj.transform.position = pos;
        obj.name = name;

        var h = obj.transform.GetChild(1);
        var w = obj.transform.GetChild(3);
        var head = obj.transform.GetChild(5);
        var waist = obj.transform.GetChild(7);

        h.GetComponentInChildren<Text>().text = info.height + "cm";
        w.GetComponentInChildren<Text>().text = info.weight + "kg";
        head.GetComponentInChildren<Text>().text = info.head + "cm";
        waist.GetComponentInChildren<Text>().text = info.waist + "cm";
    }

    public void ExclusiveButton(ButtonState next) {
        if(next == curr) {
            curr = ButtonState.None;
            return;
        }
        else if(curr != ButtonState.None) {
            switch(curr) {
                case ButtonState.Menu:
                    menu.OnClick();
                    break;
                case ButtonState.Chart:
                    chart.OnClickButton();
                    break;
                case ButtonState.Learn:
                    learn.OnClickLearn();
                    break;
                case ButtonState.Group:
                    group.OnClick();
                    break;
            }
        }
        curr = next;
    }
}