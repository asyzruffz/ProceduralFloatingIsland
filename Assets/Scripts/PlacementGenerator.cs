using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementGenerator : MonoBehaviour {

    [Range(0, 100)]
    public float probability = 50;
	public GameObject[] items;

	public void GeneratePlacement (List<IsleInfo> islands, ref System.Random randomizer) {
        GameObject holder = new GameObject ("Decor");
        holder.transform.parent = transform;

        List<MeshFilter> meshFilters = IsleInfo.GetSurfaceMeshes (islands, 0);

        foreach (MeshFilter mf in meshFilters) {

            Vector3[] verts = mf.sharedMesh.vertices;
            int[] indices = mf.sharedMesh.triangles;

            int i = 0;
            while (i < mf.sharedMesh.triangles.Length) {

                Vector3 p1 = verts[indices[i++]];
                Vector3 p2 = verts[indices[i++]];
                Vector3 p3 = verts[indices[i++]];

                Vector3 pos = mf.transform.TransformPoint ((p1 + p2 + p3) / 3);

                Vector3 n1 = verts[indices[i++]];
                Vector3 n2 = verts[indices[i++]];
                Vector3 n3 = verts[indices[i++]];

                //Vector3 rot = mf.transform.TransformDirection ((n1 + n2 + n3) / 3);

                if (randomizer.NextDouble() * 100 < probability) {
                    GameObject objPlaced = Instantiate (items[randomizer.Next(0, items.Length)], holder.transform);

                    objPlaced.transform.position = pos;
                    objPlaced.transform.localRotation = Quaternion.identity;
                }
            }
        }
	}
}
