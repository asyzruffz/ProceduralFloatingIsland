using UnityEngine;

public class SaveSlotInfo : MonoBehaviour {

    public SlotData slotData;

}

[System.Serializable]
public struct SlotData {
    public string timeLastSaved;
    public string fileName;
    public string displayName;
}
