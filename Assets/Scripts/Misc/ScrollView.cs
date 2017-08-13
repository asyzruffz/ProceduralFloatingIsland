using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollView : MonoBehaviour {

    public Camera cam;
    public float smoothingSpeed = 1;
    public bool autoScroll = false;
    [ConditionalHide("autoScroll", true)]
    public float autoDelay = 1;

    [Header("Viewable")]
    public List<CameraView> viewList = new List<CameraView> ();

    int currentIndex, targetIndex;
    bool snapped = true;
    float timer = 0;

    void Start () {
		if (viewList.Count > 0) {
            targetIndex = 0;
            ShiftView (false);
        }

        timer = autoDelay - 1;
    }
	
	void Update () {
        if (viewList.Count > 0) {
            if (!snapped) {
                ShiftView (true);
                SnapToPlace ();
            }
        }

        if (autoScroll) {
            timer += Time.deltaTime;
            if (timer >= autoDelay) {
                timer = 0;
                Transition (1);
            }
        } else {
            if (Input.GetButtonDown ("Horizontal") && Input.GetAxisRaw ("Horizontal") > 0) {
                Transition (1);
            } else if (Input.GetButtonDown ("Horizontal") && Input.GetAxisRaw ("Horizontal") < 0) {
                Transition (-1);
            }
        }
    }

    void ShiftView (bool smoothing) {
        Vector3 targetPos = viewList[targetIndex].target.position + viewList[targetIndex].viewOffset;
        Quaternion targetRot = Quaternion.Euler (viewList[targetIndex].viewRotation.x, viewList[targetIndex].viewRotation.y, viewList[targetIndex].viewRotation.z);

        cam.transform.position = Vector3.Lerp (cam.transform.position, targetPos, smoothing ? smoothingSpeed * Time.deltaTime : 1);
        cam.transform.rotation = Quaternion.Lerp (cam.transform.rotation, targetRot, smoothing ? smoothingSpeed * Time.deltaTime : 1);
    }

    void SnapToPlace () {
        Vector3 targetPos = viewList[targetIndex].target.position + viewList[targetIndex].viewOffset;
        Quaternion targetRot = Quaternion.Euler (viewList[targetIndex].viewRotation.x, viewList[targetIndex].viewRotation.y, viewList[targetIndex].viewRotation.z);

        bool nearPosition = Vector3.Distance (cam.transform.position, targetPos) < 0.01;
        bool nearRotation = Vector3.Distance (cam.transform.position, targetPos) < 0.01;

        if (nearPosition) {
            cam.transform.position = targetPos;
        }
        if (nearRotation) {
            cam.transform.rotation = targetRot;
        }
        if (nearPosition && nearRotation) {
            snapped = true;
        }
    }

    public void Transition (int amount) {
        if (amount != 0) {
            targetIndex = currentIndex + amount;
            targetIndex = targetIndex % viewList.Count;
            while (targetIndex < 0) {
                targetIndex += viewList.Count;
            }

            currentIndex = targetIndex;
            snapped = false;
        }
    }
}

[System.Serializable]
public class CameraView {

    public Transform target;
    public Vector3 viewOffset;
    public Vector3 viewRotation;

}