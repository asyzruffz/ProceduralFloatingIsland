using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDisplay : MonoBehaviour {

    public GameObject[] UIList;
    
	public void SetDisplay (GameObject obj, bool show) {
        obj.SetActive (show);
	}
}
