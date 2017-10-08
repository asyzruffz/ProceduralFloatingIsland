using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SectorArrangement : ScriptableObject {

    public string description;
    [HideInInspector]
    public SectorType type;

    public SectorArrangement() {
        type = SectorType.Blank;
    }

    public virtual void Setup (SectorInfo sector, TerrainVerticesDatabase vertDatabase, Transform parent) {

    }
}

public enum SectorType {
    Blank,
    SpawnPoint,
    SpawnScattered
}
