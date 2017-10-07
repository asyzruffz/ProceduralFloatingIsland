using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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

                GameSave tempData = new GameSave ();
                JsonFile.Load (saveFiles[i], GameController.Instance.saveFolder, ref tempData);

                SlotData slotData = new SlotData ();
                slotData.timeLastSaved = tempData.TimeLastSaved;
                slotData.fileName = saveFiles[i];
                slotData.displayName = tempData.Name + " (" + tempData.TimeLastSaved + ")";

                AddExistingSlotButton (slotData, isSaving);
            }
        }

        SortSlots ();
    }

    public void AddFurtherAction (UnityAction calledFunction) {
        foreach (GameObject slot in slots) {
            Button slotButton = slot.GetComponent<Button> ();
            slotButton.onClick.AddListener (calledFunction);
        }
    }

    void AddCreateSlotButton () {
        GameObject newSlot = Instantiate (slotPrefab, transform);
        
        newSlot.GetComponentInChildren<Text> ().text = "New Slot";

        Button slotButton = newSlot.GetComponent<Button> ();
        slotButton.onClick.AddListener (() => { SaveToFile (CreateFileName ()); });

        Destroy (newSlot.GetComponentsInChildren<Button> ()[1].gameObject); // Remove the delete button for this slot

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

    void SortSlots () {
        GameObject temp = new GameObject ("Temp");
        temp.transform.SetParent (transform, false);

        for (int i = 0; i < slots.Count; i++) {
            slots[i].transform.SetParent (temp.transform, false);
        }

        slots.Sort (delegate (GameObject a, GameObject b) {
            if (string.IsNullOrEmpty (a.GetComponent<SaveSlotInfo> ().slotData.timeLastSaved) &&
                string.IsNullOrEmpty (b.GetComponent<SaveSlotInfo> ().slotData.timeLastSaved)) {
                return 0;
            } else if (string.IsNullOrEmpty (a.GetComponent<SaveSlotInfo> ().slotData.timeLastSaved)) {
                return -1;
            } else if (string.IsNullOrEmpty (b.GetComponent<SaveSlotInfo> ().slotData.timeLastSaved)) {
                return 1;
            } else {
                return b.GetComponent<SaveSlotInfo> ().slotData.GetLastSavedDateTime ().CompareTo (
                       a.GetComponent<SaveSlotInfo> ().slotData.GetLastSavedDateTime ());
            }
        });

        for (int i = 0; i < slots.Count; i++) {
            slots[i].transform.SetParent (transform, false);
        }

        Destroy (temp);
    }

    string CreateFileName () {
        return "save-" + DateTime.Now.ToString("yy-MM-dd-HH-mm-ss") + ".json";
    }

    void SaveToFile (string fileName) {
        GameController.Instance.saveData.TimeLastSaved = DateTime.Now.ToString ();
        JsonFile.Save (fileName, GameController.Instance.saveFolder, GameController.Instance.saveData);
    }

    void LoadFromFile (string fileName) {
        JsonFile.Load (fileName, GameController.Instance.saveFolder, ref GameController.Instance.saveData);
    }
}
