using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "SpawnScattered", menuName = "Procedural/Sector/Spawn Scattered")]
public class SpawnScattered : SectorArrangement {
    
    public GameObject[] objectsToSpawn;
    [MinMax]
    public Vector2 amount;
    [MinMaxSlider(0,1)]
    public Vector2 inlandRange;

    public SpawnScattered () {
        type = SectorType.SpawnScattered;
    }

    public override void Setup (SectorInfo sector, TerrainVerticesDatabase vertDatabase, Transform parent) {
        base.Setup (sector, vertDatabase, parent);

        List<TerrainVertData> verts = sector.GetTerrainVerts (vertDatabase);
        List<Vector3> points = new List<Vector3> ();
        foreach (TerrainVertData point in verts) {
            if (point.inlandPosition >= inlandRange.x && point.inlandPosition <= inlandRange.y) {
                points.Add (point.GetSurfacePos () - parent.transform.position);
            }
        }
        
        int quantity = (int)Random.Range (amount.x, amount.y);
        for (int i = 0; i < quantity; i++) {
            GameObject spawn = Instantiate (objectsToSpawn[Random.Range (0, objectsToSpawn.Length)], parent);
            spawn.name = objectsToSpawn[0].name;
            spawn.transform.localPosition = offset + points[Random.Range (0, points.Count)];
            spawn.transform.localRotation = Quaternion.AngleAxis (Random.Range (0.0f, 360.0f), spawn.transform.up);
        }
    }
}
