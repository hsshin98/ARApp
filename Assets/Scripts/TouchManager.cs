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

        //should implement threshold for pos - avoid printing out of screend

        obj.transform.position = pos;
        obj.name = name;

        var h = obj.transform.GetChild(1);
        var w = obj.transform.GetChild(3);
        
        h.GetComponentInChildren<Text>().text = info.height + "cm";
        w.GetComponentInChildren<Text>().text = info.weight + "kg";
    }
}
