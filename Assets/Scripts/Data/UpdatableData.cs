﻿using System;
using UnityEngine;

public class UpdatableData : ScriptableObject {

    public event Action OnValuesUpdated;
    public bool autoUpdate = true;

    protected virtual void OnValidate () {
        if(autoUpdate) {
            NotifyOfUpdatedValues ();
        }
    }

    public void NotifyOfUpdatedValues () {
		if(OnValuesUpdated != null) {
            OnValuesUpdated ();
        }
	}
}
