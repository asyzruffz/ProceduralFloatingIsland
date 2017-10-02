using System.Collections.Generic;
using UnityEngine;

public class SectorInfo {

    public int id;

    public GameObject gameObject;

    public List<Vector3> GetVertices () {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter> ();
        return new List<Vector3> (meshFilter.sharedMesh.vertices);
    }
}
