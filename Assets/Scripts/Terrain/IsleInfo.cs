using System.Collections.Generic;
using UnityEngine;

public class IsleInfo {

    public int id;

    public GameObject gameObject;

    public Vector3 offset;
    
    public IsleMeshDetail surfaceMeshDetail;
    
    public MeshFilter GetSurfaceMesh (int index) {
        MeshFilter[] meshFilter = gameObject.GetComponentsInChildren<MeshFilter> ();
        return meshFilter[index];
    }
}
