using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuDisplay : MonoBehaviour {
    
    public GameObject[] UIItems;

    public void ShowItem (int index) {
        UIItems[index].SetActive (true);
    }

    public void HideItem (int index) {
        UIItems[index].SetActive (false);
    }
}
