using System.Collections.Generic;
using UnityEngine;

public class SectorInfo {

    public int id;

    public GameObject gameObject;

    public Vector3 offset;

    public List<Vector3> GetVertices () {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter> ();
        return new List<Vector3> (meshFilter.sharedMesh.vertices);
    }

    public List<TerrainVertData> GetTerrainVerts (TerrainVerticesDatabase vertDatabase) {
        List<TerrainVertData> sectorVerts = new List<TerrainVertData> ();

        foreach (var vertPair in vertDatabase.verticesDictionary) {
            if (vertPair.Value.sectorId == id) {
                sectorVerts.Add (vertPair.Value);
            }
        }

        return sectorVerts;
    }
}
