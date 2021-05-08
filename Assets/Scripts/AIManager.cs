using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class AIManager : MonoBehaviour {
    public Button Attribute1, Attribute2, Comparator1, Comparator2;
    public Button Input, Run;
    public Button Main, Retry;

    private int att, comp, val;
    private int input;
    private Data[] data;
    private GameObject[] tables;
    private List<GameObject> instantiated;
    private TouchScreenKeyboard keyboard;
    void Start() {
        input = -1;
        att = -1;
        comp = -1;
        Attribute1.onClick.AddListener(SetOnClick);
        Attribute2.onClick.AddListener(SetOnClick);
        Comparator1.onClick.AddListener(SetOnClick);
        Comparator2.onClick.AddListener(SetOnClick);
        Input.onClick.AddListener(SetInput);
        Run.onClick.AddListener(OnClickRun);
        Retry.onClick.AddListener(OnClickRetry);
        Main.onClick.AddListener(OnClickMain);

        tables = new GameObject[2];
        tables[0] = transform.Find("/Canvas/GroupA/Scroll View/Viewport/Content/row1").gameObject;
        tables[1] = transform.Find("/Canvas/GroupB/Scroll View/Viewport/Content/row1").gameObject;
        tables[0].SetActive(false);
        tables[1].SetActive(false);

        instantiated = new List<GameObject>();
    }

    public void SetOnClick() {
        Button toggleOn, toggleOff;
        GameObject obj = EventSystem.current.currentSelectedGameObject;

        if (obj == null) {
            return;
        }

        if (obj.name == Attribute1.gameObject.name) {
            toggleOn = Attribute1;
            toggleOff = Attribute2;
            att = 2;
        }
        else if (obj.name == Attribute2.gameObject.name) {
            toggleOn = Attribute2;
            toggleOff = Attribute1;
            att = 3;
        }
        else if (obj.name == Comparator1.gameObject.name) {
            toggleOn = Comparator1;
            toggleOff = Comparator2;
            comp = 1;
        }
        else if (obj.name == Comparator2.gameObject.name) {
            toggleOn = Comparator2;
            toggleOff = Comparator1;
            comp = 2;
        }
        else {
            return;
        }

        toggleOn.interactable = false;
        toggleOff.interactable = true;

        Debug.Log(obj.name);
    }
    public void SetInput() {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad); 
    }
    public void SetData(Data[] d) {
        data = d;
    }
    public void OnClickRun() {
        if(att == -1 || comp == -1 || input == -1) {
            return;
        }

        ClearTable();

        int result = 0;
        Array.Sort<Data>(data, (x, y) => x.getByType(att).CompareTo(y.getByType(att)));
        foreach (Data d in data) {
            int a, b;
            if (comp == 1) {
                a = d.getByType(att);
                b = input;
            }
            else {
                a = input;
                b = d.getByType(att);
            }

            if(a > b) {
                result += AddTable(d, 0);
            }
            else {
                result += AddTable(d, 1);
            }
        }

        float val = (float)result / data.Length * 100.0f;
        GameObject.Find("Result").GetComponent<Text>().text = "Result : " + val + "%";
    }

    private void OnClickMain() {
        SceneManager.LoadScene("MainScene");
    }
    private void OnClickRetry() {
        SceneManager.LoadScene("AIScene");
    }
    private void ClearTable() { 
        foreach (var i in instantiated) {
            Destroy(i);
        }
    }
    private int AddTable(Data d, int t) {
        int ret = 0;
        GameObject obj = tables[t];
        GameObject clone = Instantiate(obj);
        clone.transform.SetParent(obj.transform.parent);
        clone.SetActive(true);
        var dat = d.getVal();

        for(int i = 0; i < 4; ++i) {
            clone.transform.GetChild(i).GetComponent<Text>().text = dat[i];
        }
        Debug.Log("" + d.getByType(4) + ", " + t);

        if(d.getByType(4) == t) {
            clone.GetComponent<Image>().color = Color.green;
            ++ret;
        }
        else {
            clone.GetComponent<Image>().color = Color.red;
        }

        instantiated.Add(clone);
        return ret;
    }
    void Update() {
        if(keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Done) {
            string str = keyboard.text;
            input = int.Parse(str);
            Input.transform.GetChild(0).GetComponent<Text>().text = str;
        }
    }
}