using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlots : MonoBehaviour {

    public Text title;
    public GameObject slotPrefab;

    List<GameObject> slots = new List<GameObject> ();
    
    public void SetSlots (bool isSaving) {
        title.text = isSaving ? "Save to..." : "Load from...";

        ClearAllSlots ();

        if (isSaving) {
            AddCreateSlotButton ();
        }

        string[] saveFiles = JsonFile.GetAllFilesIn (GameController.Instance.saveFolder);

        if (saveFiles != null) {
            for (int i = 0; i < saveFiles.Length; i++) {
                SlotData slotData = new SlotData ();
                slotData.id = -1;
                slotData.fileName = saveFiles[i];
                slotData.displayName = saveFiles[i];

                AddExistingSlotButton (slotData, isSaving);
            }
        }
    }

    void AddCreateSlotButton () {
        GameObject newSlot = Instantiate (slotPrefab, transform);

        SlotData slotData = new SlotData ();
        slotData.id = Random.Range (0, 1000);
        slotData.fileName = "save" + slotData.id + ".json";

        GameController.Instance.saveData.Id = slotData.id;

        newSlot.GetComponent<SaveSlotInfo> ().slotData = slotData;
        newSlot.GetComponentInChildren<Text> ().text = "New Slot";

        Button slotButton = newSlot.GetComponent<Button> ();
        slotButton.onClick.AddListener (() => { SaveToFile (slotData.fileName); });

        slots.Add (newSlot);
    }

    void AddExistingSlotButton (SlotData slotData, bool isSaving) {
        GameObject slot = Instantiate (slotPrefab, transform);

        slot.GetComponent<SaveSlotInfo> ().slotData = slotData;
        slot.GetComponentInChildren<Text> ().text = slotData.displayName;

        Button slotButton = slot.GetComponent<Button> ();

        if (isSaving) {
            slotButton.onClick.AddListener (() => { SaveToFile (slotData.fileName); });
        } else {
            slotButton.onClick.AddListener (() => { LoadFromFile (slotData.fileName); });
        }

        slots.Add (slot);
    }

    void ClearAllSlots () {
        foreach (GameObject slot in slots) {
            Destroy (slot);
        }
        slots.Clear ();
    }

    void SaveToFile (string fileName) {
        JsonFile.Save (fileName, GameController.Instance.saveFolder, GameController.Instance.saveData);
    }

    void LoadFromFile (string fileName) {
        JsonFile.Load (fileName, GameController.Instance.saveFolder, ref GameController.Instance.saveData);
    }
}
