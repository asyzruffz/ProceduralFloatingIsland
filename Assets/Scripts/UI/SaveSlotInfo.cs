using UnityEngine;

public class SaveSlotInfo : MonoBehaviour {

    public SlotData slotData;

}

[System.Serializable]
public struct SlotData {
    public int id;
    public string fileName;
    public string displayName;
}
