using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;


public struct Data { 
    int index, height, weight, gender;
    string[] ret;

    public void setVal(string[] input) {
        index = int.Parse(input[0]);
        height = int.Parse(input[1]);
        weight = int.Parse(input[2]);
        gender = input[3] == "³²ÀÚ" ? 0 : 1;

        ret = input;
    }
    public int getByType(int i) {
        if (i == 1) return index;
        else if (i == 2) return height;
        else if (i == 3) return weight;
        else if (i == 4) return gender;
        return -1;
    }
    public string[] getVal() {
        return ret;
    }
}

public class TableManager : MonoBehaviour {
    public Button Index, Height, Weight;
    public TextAsset textfile;
    private Data[] input;
    private int sortType; // + ascending , - descending 
    
    void Start() {
        ReadData();
        InitTable();
        Index.onClick.AddListener(OnClick);
        Height.onClick.AddListener(OnClick);
        Weight.onClick.AddListener(OnClick);
        UpdateTable();
        sortType = 1;
    }

    public Data[] GetData() {
        Debug.Log("return data");
        return input;
    }
    public void OnClick() {
        GameObject obj = EventSystem.current.currentSelectedGameObject;
        if (obj == null)
            return;
       
        string name = obj.name;
        if (name == "IndexButton")
            SortData(1);
        else if (name == "HeightButton")
            SortData(2);
        else if (name == "WeightButton")
            SortData(3);
        
        UpdateTable();
    }
    void SortData(int att) {
        if (att == sortType || att == -sortType) {
            if (sortType > 0) {
                Array.Sort<Data>(input, (y, x) => x.getByType(att).CompareTo(y.getByType(att)));
            } else {
                Array.Sort<Data>(input, (x, y) => x.getByType(att).CompareTo(y.getByType(att)));
            }
            sortType = -sortType;
        }
        else {
            Array.Sort<Data>(input, (x, y) => x.getByType(att).CompareTo(y.getByType(att)));
            sortType = att;
        }
    }

    private void ReadData() {
        int idx = 0;
        var splitFile = new string[] { "\r\n", "\r", "\n" };
        string[] d = textfile.text.Split(splitFile, StringSplitOptions.None);
        
        input = new Data[d.Length];
        
        foreach (string s in d) {
            string[] inp = s.Split(' ');
            input[idx].setVal(inp);
            ++idx;
        }

        GameObject.Find("Panel").GetComponent<AIManager>().SetData(input);
    }

    private void InitTable() {
        int rows = input.Length;
        GameObject obj = transform.Find("Scroll View/Viewport/Content/row1").gameObject;
        for (int i = 2; i <= rows; ++i) {
            GameObject clone = Instantiate(obj);
            clone.name = "row" + i;
            clone.transform.SetParent(obj.transform.parent);
        }
    }
    private void UpdateTable() {
        int n = input.Length;
        Transform obj = transform.Find("Scroll View/Viewport/Content");
        for (int i = 0; i < n; ++i) {
            Transform row = obj.GetChild(i);
            string[] val = input[i].getVal();
            for(int j = 0; j < 4; ++j)
                row.GetChild(j).GetComponent<Text>().text = val[j];
        }
    }

    private void PrintTable() {
        for(int i = 0; i < input.Length; ++i) {
            Debug.Log("index : " + i);
            string[] val = input[i].getVal();
            foreach (string s in val)
                Debug.Log(s);
        }
    }
}