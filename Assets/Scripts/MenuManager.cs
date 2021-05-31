using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
public class MenuManager : MonoBehaviour {
    private bool toggle = false;
    private EvaluationManager em;
    private Slider slider;
    private Button menu, display, reset, option;
    private GameObject speed;

    private void Awake() {
        em = FindObjectOfType<EvaluationManager>();

        menu = transform.GetChild(0).GetComponent<Button>();
        display = transform.GetChild(1).GetComponent<Button>();
        option = transform.GetChild(2).GetComponent<Button>();
        reset = transform.GetChild(3).GetComponent<Button>();
        speed = transform.GetChild(4).gameObject;
        slider = speed.GetComponent<Slider>();
    }
    void Start() {
        menu.onClick.AddListener(OnClick);
        reset.onClick.AddListener(OnClickRestart);
        option.onClick.AddListener(OnClickSettings);
        display.gameObject.SetActive(false);

        slider.onValueChanged.AddListener(OnValueChanged);
        slider.value = 10f;
    }

    public void OnClick() {
        slider.gameObject.SetActive(false);
        if (toggle == false) {
            toggle = true;
            //display.gameObject.SetActive(true);
            option.gameObject.SetActive(true);
            reset.gameObject.SetActive(true);
        }
        else {
            toggle = false;
            //display.gameObject.SetActive(false);
            option.gameObject.SetActive(false);
            reset.gameObject.SetActive(false);
        }
    }

    void OnClickRestart() {
        FindObjectOfType<ARSession>().Reset();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnClickSettings() {
        OnClick();

        slider.gameObject.SetActive(true);
    }

    void OnValueChanged(float val) {
        em.SetSpeed(val / 10f);
        slider.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = "" + (val / 10f) + "√ ";
    }
}
