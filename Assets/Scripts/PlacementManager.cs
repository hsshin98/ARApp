using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacementManager : MonoBehaviour {
    private enum State {
        Ready,
        SelectingObject,
        PlacingObject
    }

    public GameObject prefab;
    public GameObject objParent;

    private State state;
    private GameObject buttonParent;
    private ARTrackedImageManager arTrackedImageManager;
    private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private GameObject selectedObj;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake() {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        arTrackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        arPlaneManager = FindObjectOfType<ARPlaneManager>();
        buttonParent = transform.GetChild(0).gameObject;
        buttonParent.gameObject.SetActive(false);
        Debug.Log("Awake");
    }

    void Start() {
        gameObject.GetComponent<Button>().onClick.AddListener(OnClickMaster);
        state = State.Ready;
    }

    private void OnEnable() {
        arPlaneManager.planesChanged += OnPlaneChanged;
    }
    private void OnDisable() {
        arPlaneManager.planesChanged -= OnPlaneChanged;
    }

    void OnPlaneChanged(ARPlanesChangedEventArgs args) {
        foreach(var plane in args.added) {
            UpdatePlane(plane);
        }
        foreach(var plane in args.updated) {
            UpdatePlane(plane);
        }
    }
    void UpdatePlane(ARPlane plane) {
        if(plane.trackingState == TrackingState.Tracking && state == State.PlacingObject) {
            plane.gameObject.SetActive(true);
        }
        else {
            plane.gameObject.SetActive(false);
        }
    }
    bool TryGetTouchPosition(out Vector2 touchPosition) {
        if(Input.touchCount > 0) {
            touchPosition = Input.GetTouch(0).position;

            if(EventSystem.current.IsPointerOverGameObject()) {
                return false;
            }
            PointerEventData currPos = new PointerEventData(EventSystem.current);
            currPos.position = touchPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(currPos, results);
            
            return results.Count == 0;
        }
        touchPosition = default;
        return false;
    }
    void OnClickMaster() {
        if(state == State.Ready) {
            state = State.SelectingObject;
            buttonParent.SetActive(true);
        }
        else if(state == State.SelectingObject) {
            state = State.Ready;
            buttonParent.SetActive(false);
        }
        else if(state == State.PlacingObject) {
            state = State.Ready;
            buttonParent.SetActive(false);
        }
    }

    void OnClickObjButton() {
        var curr = EventSystem.current.currentSelectedGameObject;
        Debug.Log("Pressed : " + curr.name);

        if (state == State.SelectingObject) {
            //check if it is already instantiated
            foreach (Transform child in objParent.transform) {
                if (child.name == curr.name) {
                    state = State.PlacingObject;
                    return;
                }
            }

            //select corresponding gameobject
            foreach (var trackedImage in arTrackedImageManager.trackables) {
                if (trackedImage.referenceImage.name == curr.name) {
                    selectedObj = trackedImage.gameObject;
                    selectedObj.name = curr.name;
                    state = State.PlacingObject;
                    return;
                }
            }
        }
        else if(state == State.PlacingObject) {
            state = State.SelectingObject;
            return;
        }
    }
    public void InstantiateButton(ARTrackedImage trackedImage) {
        Transform parent = buttonParent.transform;
        GameObject obj = Instantiate(prefab, parent);
        
        float dist = (obj.GetComponent<RectTransform>().rect.width + 30f) * parent.childCount;
        
        var pos = obj.transform.position;
        pos.x += dist;
        obj.transform.position = pos;

        obj.name = trackedImage.referenceImage.name;
        obj.GetComponentInChildren<Text>().text = obj.name;
        obj.GetComponent<Button>().onClick.AddListener(OnClickObjButton);
    }

    void Update() {
        if (state == State.SelectingObject) {
            foreach (var trackedImage in arTrackedImageManager.trackables) {
                var name = trackedImage.referenceImage.name;
                if (name[0] != 'm')
                    continue;

                if (trackedImage.trackingState == TrackingState.Tracking) {
                    //disable button
                    foreach (Transform child in buttonParent.transform) {
                        if (child.name == name) {
                            child.GetComponent<Button>().interactable = false;
                            break;
                        }
                    }
                }
                else {
                    //enable button
                    foreach (Transform child in buttonParent.transform) {
                        if (child.name == name) {
                            child.GetComponent<Button>().interactable = true;
                            break;
                        }
                    }
                }
            }
        }
        else if (state == State.PlacingObject) {
            Vector2 touchPosition;
            if (TryGetTouchPosition(out touchPosition)) {
                if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.Planes)) {
                    var hitPose = hits[0].pose;
                    foreach (Transform child in objParent.transform) {
                        if(child.name == selectedObj.name) {
                            child.position = hitPose.position;
                            return;
                        }
                    }
                    
                    //if obj is not instantiated
                    Debug.Log("Instantiate obj");
                    
                    var obj = Instantiate(selectedObj, hitPose.position, hitPose.rotation);
                    obj.transform.SetParent(objParent.transform);
                    obj.name = obj.transform.GetChild(1).name;
                    obj.SetActive(true);

                    obj.transform.GetChild(0).gameObject.SetActive(true);
                    obj.transform.GetChild(1).gameObject.SetActive(true);
                    Debug.Log("Instantiated " + obj.transform.GetChild(1).name);
                }
            }
        }
    }

    public void DestroyInstance(ARTrackedImage trackedImage) {
        var name = trackedImage.referenceImage.name;
        foreach(Transform child in objParent.transform) {
            if(child.name == name) {
                Destroy(child.gameObject);
                return;
            }
        }
    }
}