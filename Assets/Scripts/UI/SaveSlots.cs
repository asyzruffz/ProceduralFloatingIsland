using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlots : MonoBehaviour {

    public Text title;
    public GameObject slotPrefab;

    List<GameObject> slots = new List<GameObject> ();

	void Start () {
        SetSlots (true);

    }
	
	void Update () {
		
	}

    public void SetSlots (bool isSaving) {
        title.text = isSaving ? "Save to..." : "Load from...";

        ClearAllSlots ();

        if (isSaving) {
            AddCreateSlotButton ();
        }

        for (int i = 0; i < 3; i++) {
            AddExistingSlotButton (i + 1);
        }
    }

    void AddCreateSlotButton () {
        GameObject newSlot = Instantiate (slotPrefab, transform);

        newSlot.GetComponentInChildren<Text> ().text = "New Slot";

        slots.Add (newSlot);
    }

    void AddExistingSlotButton (int num) {
        GameObject slot = Instantiate (slotPrefab, transform);

        slot.GetComponentInChildren<Text> ().text = "Slot " + num;

        slots.Add (slot);
    }

    void ClearAllSlots () {
        foreach (GameObject slot in slots) {
            Destroy (slot);
        }
        slots.Clear ();
    }
}
