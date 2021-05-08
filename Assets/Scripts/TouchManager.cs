using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TouchManager : MonoBehaviour {
    public Dictionary<string, Info> dict;
    public GameObject textPrefab;
    private GameObject canvas;
    void Start() {
        canvas = FindObjectOfType<Canvas>().gameObject;
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
        }
    }

    void ClearUI() {
        Debug.Log("Clear");
        foreach(Transform child in canvas.transform) {
            Destroy(child.gameObject);
        }
    }
    void DisplayInfo(Vector3 pos, Info info, string name) {
        GameObject obj;
        obj = Instantiate(textPrefab, pos, Quaternion.identity);
        Debug.Log(pos);
        obj.transform.SetParent(canvas.transform);
        obj.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = info.height + "cm";
        obj.transform.GetChild(3).GetChild(0).GetComponent<Text>().text = info.weight + "kg";
        obj.name = name;
    }
}
