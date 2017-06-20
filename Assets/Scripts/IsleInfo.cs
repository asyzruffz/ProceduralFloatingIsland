using System.Collections.Generic;
using UnityEngine;

public class IsleInfo {

    public int id;

    public GameObject gameObject;

    public Vector3 offset;
    
    public MeshRegion surfaceMeshRegion;

    public static List<MeshFilter> GetSurfaceMeshes (List<IsleInfo> islandInfos, int index) {
        List<MeshFilter> meshes = new List<MeshFilter> ();

        foreach (IsleInfo island in islandInfos) {
            MeshFilter[] meshFilter = island.gameObject.GetComponentsInChildren<MeshFilter> ();
            meshes.Add (meshFilter[index]);
        }

        return meshes;
    }
}
