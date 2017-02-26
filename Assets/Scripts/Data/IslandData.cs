using UnityEngine;

[CreateAssetMenu(fileName = "IslandData", menuName = "Procedural/Island Data")]
public class IslandData : UpdatableData {

    [Header("Size")]
    public int maxWidth = 50;
    public int maxHeight = 50;

    public float tileSize = 1;
    public float depth = 5;

    [Range (0, 100)]
    public float randomFillPercent = 50;

    [Header ("Materials")]
    public Material dirtMaterial;
    public Material grassMaterial;
}
