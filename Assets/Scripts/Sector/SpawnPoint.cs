using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "SpawnPoint", menuName = "Procedural/Sector/Spawn Point")]
public class SpawnPoint : SectorArrangement {

    public int amount = 1;
    public GameObject objectToSpawn;

    public SpawnPoint () {
        type = SectorType.SpawnPoint;
    }

    public override void Setup (List<Vector3> points, Transform parent) {
        base.Setup (points, parent);

        GameObject spawn = Instantiate (objectToSpawn, parent);
        spawn.name = objectToSpawn.name;
    }
}
