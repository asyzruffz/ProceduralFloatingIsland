using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationGenerator : MonoBehaviour {

    List<IsleInfo> islands;

    public void GenerateElevation (List<IsleInfo> islandInfos) {
        islands = islandInfos;

        List<MeshFilter> surfaceMeshes = GetSurfaceMeshes ();
        for(int i = 0; i < surfaceMeshes.Count; i++) {
            //List<Vector3> v = Test (surfaceMeshes[i].sharedMesh.vertices);
            //surfaceMeshes[i].sharedMesh.vertices = v.ToArray ();
        }
    }

    List<MeshFilter> GetSurfaceMeshes () {
        List<MeshFilter> meshes = new List<MeshFilter> ();

        foreach(IsleInfo island in islands) {
            MeshFilter[] meshFilter = island.gameObject.GetComponentsInChildren<MeshFilter> ();
            meshes.Add (meshFilter[0]);
        }

        return meshes;
    }

    List<Vector3> Test (Vector3[] vertices) {
        List<Vector3> newVertices = new List<Vector3> ();
        for (int i = 0; i < vertices.Length; i++) {
            newVertices.Add(new Vector3(vertices[i].x, Mathf.Sin(i * Mathf.Deg2Rad), vertices[i].z));
        }
        return newVertices;
    }
}
