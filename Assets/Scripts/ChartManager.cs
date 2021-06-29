using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
public class ChartManager : MonoBehaviour {
    public Sprite boySprite, girlSprite;
    public GameObject parent;
    public Button active;

    private GameObject panel, table, tableparent;
    private ARTrackedImageManager arTrackedImageManager;
    private RectTransform containter;
    private Dictionary<string, Info> dict;
    private TouchManager tm;
    private int xAxis = 0, yAxis = 0;
    private int xMin = 0, xMax = 0, yMin = 0, yMax = 0;
    void Start() {
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        tm = FindObjectOfType<TouchManager>();
        dict = FindObjectOfType<ImageRecognition>().dict;
        containter = parent.GetComponent<RectTransform>();
        panel = transform.GetChild(0).gameObject;

        tableparent = panel.transform.GetChild(4).gameObject;
        

        Transform yparent = panel.transform.GetChild(2);
        foreach(Transform child in yparent)
            child.GetComponent<Button>().onClick.AddListener(OnClickAttribute);

        Transform xparent = panel.transform.GetChild(3);
        foreach (Transform child in xparent)
            child.GetComponent<Button>().onClick.AddListener(OnClickAttribute);

        active.onClick.AddListener(OnClickButton);
        panel.SetActive(false);
    }

    private void Awake() {
        Debug.Log("Table:");
        table = GameObject.Find("LearningUI").transform.GetChild(3).GetChild(1).gameObject;
        Debug.Log(table.name);
    }

    void Update() {

    }
    void CreateTable() {
        var obj = Instantiate(table, tableparent.transform, false);
        obj.SetActive(true);
    }
    
    void DestroyTable() {
        Destroy(tableparent.transform.GetChild(0).gameObject);
    }
    public void OnClickButton() {
        tm.ExclusiveButton(TouchManager.ButtonState.Chart);
        if(panel.activeSelf) {
            panel.SetActive(false);
            DestroyTable();
        }
        else {
            panel.SetActive(true);
            CreateTable();
            DrawPoint();
        }
    }
    void OnClickAttribute() {
        var curr = EventSystem.current.currentSelectedGameObject;
        var name = curr.name;
        int axis = name[0] == 'x' ? 0 : 1;
        int att;

        switch(name[1]) {
            case 'H':
                att = Info.Height;
                break;
            case 'h':
                att = Info.Head;
                break;
            case 'W':
                att = Info.Weight;
                break;
            case 'w':
                att = Info.Waist;
                break;
            default:
                att = -1;
                break;
        }
        var range = Info.getRange(att);

        if (axis == 0) {
            xAxis = att;
            xMin = range.x;
            xMax = range.y;
        }
        else {
            yAxis = att;
            yMin = range.x;
            yMax = range.y;
        }

        foreach(Transform child in curr.transform.parent) {
            child.GetComponent<Button>().interactable = true;
        }
        curr.GetComponent<Button>().interactable = false;

        var axisnumber = panel.transform.GetChild(1).GetChild(axis);
        int count = 0;
        string unit = att == Info.Weight ? "kg" : "cm";
        float interval = (float)(range.y - range.x) / 4f;
        foreach(Transform child in axisnumber) {
            child.GetComponent<Text>().text = "" + (range.x + interval * count) + unit;
            ++count;
        }

        DrawPoint();
    }

    void ClearCanvas() {
        foreach(Transform child in containter.transform) {
            Destroy(child.gameObject);
        }
    }
    void DrawPoint() {
        ClearCanvas();
        
        foreach(var trackedImage in arTrackedImageManager.trackables) {
            var name = trackedImage.referenceImage.name;
            if (name[0] != 'm')
                continue;
            Debug.Log(name);
            Info i = dict[name];
            
            float xval = xAxis != 0 ? ((float)i.getVal(xAxis) - xMin) / ((float)xMax - xMin) : 0f;
            float xpos = containter.sizeDelta.x * xval;
            
            float yval = yAxis != 0 ? ((float)i.getVal(yAxis) - yMin) / ((float)yMax - yMin) : 0f;
            float ypos = containter.sizeDelta.y * yval;
            Debug.Log(xpos + ", " + ypos);
            CreateCircle(new Vector2(xpos, ypos), i.gender);
        }
    }


    void CreateCircle(Vector2 pos, int gender) {
        GameObject circle = new GameObject("", typeof(Image));
        circle.transform.SetParent(containter, false);
        circle.GetComponent<Image>().sprite = (gender == Info.Boy ? boySprite : girlSprite);

        var rt = circle.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(15, 15);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);

        Debug.Log("Create circle : " + pos.x + ", " + pos.y);
    }
}
