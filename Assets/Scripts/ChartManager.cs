using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
public class ChartManager : MonoBehaviour {
    public Sprite circleSprite;
    public GameObject parent;

    private ARTrackedImageManager arTrackedImageManager;
    private RectTransform containter;
    private Dictionary<string, Info> dict;
    private int xAxis = 0, yAxis = 0;
    private int xMin = 0, xMax = 0, yMin = 0, yMax = 0;
    void Start() {
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        dict = FindObjectOfType<ImageRecognition>().dict;
        containter = parent.GetComponent<RectTransform>();

        Transform yparent = transform.GetChild(0).GetChild(1).GetChild(1);
        foreach(Transform child in yparent)
            child.GetComponent<Button>().onClick.AddListener(OnClickAttribute);

        Transform xparent = transform.GetChild(0).GetChild(2).GetChild(1);
        foreach (Transform child in xparent)
            child.GetComponent<Button>().onClick.AddListener(OnClickAttribute);
        //DrawTest();
    }

    
    void Update() {

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
            CreateCircle(new Vector2(xpos, ypos));
        }
    }

    void DrawTest() {
        ClearCanvas();

        CreateCircle(new Vector2(50f, 50f));
        CreateCircle(new Vector2(0f, 0f));
        CreateCircle(new Vector2(50f, 100f));
        CreateCircle(new Vector2(100f, 50f));
    }

    void CreateCircle(Vector2 pos) {
        GameObject circle = new GameObject("", typeof(Image));
        circle.transform.SetParent(containter, false);
        circle.GetComponent<Image>().sprite = circleSprite;

        var rt = circle.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(10, 10);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);

        Debug.Log("Create circle : " + pos.x + ", " + pos.y);
    }
}
