using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlacementManager : MonoBehaviour {
    public GameObject prefab;
    public Button button;

    private ARTrackedImageManager arTrackedImageManager;
    private ARPlaneManager arPlaneManager;
    private ARRaycastManager arRaycastManager;
    private GameObject spawnedObject;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private void Awake() {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arTrackedImageManager = GetComponent<ARTrackedImageManager>();
        arPlaneManager = GetComponent<ARPlaneManager>();
        Debug.Log("Awake");
    }

    bool TryGetTouchPosition(out Vector2 touchPosition) {
        if(Input.touchCount > 0) {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        touchPosition = default;
        return false;
    }
    void Start() {

    }
    void Update() {
        if(!TryGetTouchPosition(out Vector2 touchPosition)) {
            return;
        }

        if (arRaycastManager.Raycast(touchPosition, hits, TrackableType.Planes)) {
            var hitPose = hits[0].pose;

            if (spawnedObject == null) {
                spawnedObject = Instantiate(prefab, hitPose.position, hitPose.rotation);
            }
            else {
                spawnedObject.transform.position = hitPose.position;
            }
        }

        
    }


}