using UnityEngine;

[System.Serializable]
public class SectorArrangement : ScriptableObject {

    public string description;
    public Vector3 offset;

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
