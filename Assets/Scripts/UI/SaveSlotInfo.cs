using System;
using UnityEngine;

public class SaveSlotInfo : MonoBehaviour {

    public SlotData slotData;

}

[System.Serializable]
public class SlotData {
    public string timeLastSaved;
    public string fileName;
    public string displayName;

    public DateTime GetLastSavedDateTime () {
        return DateTime.Parse (timeLastSaved);
    }
}
