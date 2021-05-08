using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchManager : MonoBehaviour {
    public Dictionary<string, Info> dict;
    public GameObject textPrefab;
    public Button button;
    private GameObject parent;
    void Start() {
        parent = FindObjectOfType<Canvas>().transform.Find("Instances").gameObject;
        //button.onClick.AddListener(OnClick);
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
        GameObject obj = Instantiate(textPrefab, pos, Quaternion.identity);
        obj.transform.SetParent(parent.transform);
        obj.name = name;

        var h = obj.transform.GetChild(1);
        var w = obj.transform.GetChild(3);
        
        h.GetComponentInChildren<Text>().text = info.height + "cm";
        w.GetComponentInChildren<Text>().text = info.weight + "kg";

        h.GetComponent<Button>().onClick.AddListener(OnClickAttribute);
        w.GetComponent<Button>().onClick.AddListener(OnClickAttribute);
    }

    public void OnClick() {
        
    }

    void OnClickAttribute() {
        //Instantiate prefab to decide wheter increase/decrease
        //Set current value as pivot value

    }
}
