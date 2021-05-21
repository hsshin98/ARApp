using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlacementManager : MonoBehaviour {
    public GameObject prefab;
    public Button button;
    public GameObject target;
    public GameObject objParent;
    public GameObject buttonParent;

    private bool isActive, placeObject;
    private ARTrackedImageManager arTrackedImageManager;
    private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private GameObject spawnedObject;
    private GameObject selectedObj;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake() {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        //arPlaneManager.enabled = false;
        Debug.Log("Awake");
        
    }

    void Start() {
        isActive = false;
        target.SetActive(false);
        button.onClick.AddListener(onClick);


        //disabled due to plane detection quality
        this.enabled = false;
        arPlaneManager.enabled = false;
    }
    bool TryGetTouchPosition(out Vector2 touchPosition) {
        if(Input.touchCount > 0) {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        touchPosition = default;
        return false;
    }

    void onClick() {
        if(isActive) {
            placeObject = true;
        }
    }

    void onClickObjButton() {
        var curr = EventSystem.current.currentSelectedGameObject;
        //var b = curr.GetComponent<Button>();
        Debug.Log("Pressed : " + curr.name);
        if (isActive) {
            if (selectedObj == null) {
                Debug.LogError("Null object referenced!");
                return;
            }

            if (curr.name == selectedObj.transform.GetChild(1).name) {
                Debug.Log("toggle off button");
                //var colors = curr.GetComponent<Button>().colors;
                //colors.normalColor = Color.white;
                //curr.GetComponent<Button>().colors = colors;
                isActive = false;
                return;
            }
        }

        isActive = true;
        foreach (var trackedImage in arTrackedImageManager.trackables) {
            if (trackedImage.referenceImage.name == curr.name) {
                Debug.Log("toggle on button");
                selectedObj = trackedImage.gameObject;
                //var colors = curr.GetComponent<Button>().colors;
                //colors.normalColor = Color.blue;
                //curr.GetComponent<Button>().colors = colors;

                foreach(Transform child in objParent.transform) {
                    if(child.name == curr.name) {
                        Destroy(child.gameObject);
                        break;
                    }
                }

                return;
            }
        }
        Debug.LogError("GameObject corresponding to the button cannot be found!");
    }
    public void InstantiateButton(ARTrackedImage trackedImage) {
        Transform p = buttonParent.transform;
        GameObject obj = Instantiate(p.GetChild(0).gameObject, p);
        
        float dist = p.childCount * obj.GetComponent<RectTransform>().rect.width;

        obj.transform.localPosition = new Vector2(dist, 0);
        obj.name = trackedImage.referenceImage.name;
        obj.GetComponentInChildren<Text>().text = obj.name;
        obj.GetComponent<Button>().onClick.AddListener(onClickObjButton);

        obj.SetActive(true);
    }

    void Update() {
        if(isActive) {
            foreach (var plane in arPlaneManager.trackables) {
                plane.gameObject.SetActive(false);
            }
        }
        else {
            foreach (var plane in arPlaneManager.trackables) {
                plane.gameObject.SetActive(false);
            }
        }
        arPlaneManager.enabled = isActive;

        foreach(var trackedImage in arTrackedImageManager.trackables) {
            if (trackedImage.trackingState == TrackingState.Tracking) {
                //disable button
                foreach(Transform child in buttonParent.transform) {
                    if(child.name == trackedImage.referenceImage.name) {
                        child.GetComponent<Button>().interactable = false;
                        break;
                    }
                }
            }
            else {
                //enable button
                foreach (Transform child in buttonParent.transform) {
                    if (child.name == trackedImage.referenceImage.name) {
                        child.GetComponent<Button>().interactable = true;
                        break;
                    }
                }
            }
        }


        if (isActive) {
            //check if center of screen has ARPlane
            Vector2 center = new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
            if (arRaycastManager.Raycast(center, hits, TrackableType.Planes)) {
                target.SetActive(true);

                if (placeObject) {
                    Debug.Log("Instantiate obj");
                    var hitPose = hits[0].pose;

                    var obj = Instantiate(selectedObj, hitPose.position, hitPose.rotation);
                    obj.transform.SetParent(objParent.transform);
                    obj.name = obj.transform.GetChild(1).name;
                    obj.SetActive(true);

                    obj.transform.GetChild(0).gameObject.SetActive(true);
                    obj.transform.GetChild(1).gameObject.SetActive(true);
                    Debug.Log("Instantiated " + obj.transform.GetChild(1).name);

                    isActive = false;
                    placeObject = false;
                    target.SetActive(false);
                }
            }
            else {
                //Debug.Log("No plane found");
                target.SetActive(false);
            }
        }
        
    }
}