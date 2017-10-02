﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "SpawnPoint", menuName = "Procedural/Sector/Spawn Point")]
public class SpawnPoint : SectorArrangement {
    
    public GameObject objectToSpawn;
    public Vector3 offset;
    public bool childOfLevel = true;

    public SpawnPoint () {
        type = SectorType.SpawnPoint;
    }

    public override void Setup (List<Vector3> points, Transform parent) {
        base.Setup (points, parent);

        GameObject spawn = Instantiate (objectToSpawn, childOfLevel ? parent : null);
        spawn.name = objectToSpawn.name;
        spawn.transform.localPosition = offset + (childOfLevel ? Vector3.zero : parent.position);
    }
}
