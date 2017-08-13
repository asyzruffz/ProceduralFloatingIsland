using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDisplay : MonoBehaviour {

    public GameObject UIItem1;
    public GameObject UIItem2;
    public GameObject UIItem3;
    public GameObject UIItem4;
    public GameObject UIItem5;

    public void SetItem1Display (bool show) {
        UIItem1.SetActive (show);
    }

    public void SetItem2Display (bool show) {
        UIItem2.SetActive (show);
    }

    public void SetItem3Display (bool show) {
        UIItem3.SetActive (show);
    }

    public void SetItem4Display (bool show) {
        UIItem4.SetActive (show);
    }

    public void SetItem5Display (bool show) {
        UIItem5.SetActive (show);
    }
}
