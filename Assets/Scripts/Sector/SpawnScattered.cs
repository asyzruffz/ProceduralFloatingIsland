using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "SpawnScattered", menuName = "Procedural/Sector/Spawn Scattered")]
public class SpawnScattered : SectorArrangement {
    
    public GameObject[] objectsToSpawn;
    [MinMax]
    public Vector2 amount;
    public Vector3 offset;

    public SpawnScattered () {
        type = SectorType.SpawnScattered;
    }

    public override void Setup (SectorInfo sector, TerrainVerticesDatabase vertDatabase, Transform parent) {
        base.Setup (sector, vertDatabase, parent);

        List<Vector3> points = sector.GetVertices ();

        int quantity = (int)Random.Range (amount.x, amount.y);
        for (int i = 0; i < quantity; i++) {
            GameObject spawn = Instantiate (objectsToSpawn[Random.Range (0, objectsToSpawn.Length)], parent);
            spawn.name = objectsToSpawn[0].name;
            spawn.transform.localPosition = offset + points[Random.Range (0, points.Count)];
            spawn.transform.localRotation = Quaternion.AngleAxis (Random.Range (0.0f, 360.0f), spawn.transform.up);
        }
    }
}
