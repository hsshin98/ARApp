using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour {
    // Start is called before the first frame update

    public Button m_ToggleButton;

    void Start() {
        m_ToggleButton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick() {
        Debug.Log("Toggle AR");
    }
    // Update is called once per frame
    void Update() {

    }
}
