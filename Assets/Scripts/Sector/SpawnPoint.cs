using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "SpawnPoint", menuName = "Procedural/Sector/Spawn Point")]
public class SpawnPoint : SectorArrangement {
    
    public GameObject objectToSpawn;
    public bool childOfLevel = true;

    public SpawnPoint () {
        type = SectorType.SpawnPoint;
    }

    public override void Setup (SectorInfo sector, TerrainVerticesDatabase vertDatabase, Transform parent) {
        base.Setup (sector, vertDatabase, parent);

        TerrainVertData vertData = vertDatabase.GetNearestVertData (parent.position);
        Vector3 spawnPos = vertData.GetSurfacePos () - parent.position;
        
        GameObject spawn = Instantiate (objectToSpawn, childOfLevel ? parent : null);
        spawn.name = objectToSpawn.name;
        spawn.transform.localPosition = offset + (childOfLevel ? spawnPos : parent.position);
    }
}
