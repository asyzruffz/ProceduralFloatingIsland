using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycleRegionOutline : MonoBehaviour {

    cakeslice.Outline[] outlines;
    float timer = 1;
    int indexTurn = 0;
    
	void Update () {
		if(outlines == null || outlines.Length == 0) {
            outlines = gameObject.GetComponentsInChildren<cakeslice.Outline> ();
        }

		if (outlines.Length == 0) {
			return;
		}

        if (timer >= 0.8f) {
            timer = 0;

            for(int i = 0; i < outlines.Length; i++) {
                if(i == indexTurn) {
                    outlines[i].enabled = true;
                } else {
                    outlines[i].enabled = false;
                }
            }

            indexTurn++;
            indexTurn = indexTurn % outlines.Length;
        }

        timer += Time.deltaTime;
	}
}
