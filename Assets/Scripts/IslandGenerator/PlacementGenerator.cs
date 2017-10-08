﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementGenerator : MonoBehaviour {

    [Range(0, 100)]
    public float probability = 50;
	public GameObject[] items;

    public List<SectorArrangement> sectorTypes = new List<SectorArrangement> ();

    public void GenerateSectorsContent (List<SectorInfo> sectors, TerrainVerticesDatabase vertDatabase) {
        RandomSample rndSample = new RandomSample (sectors.Count);

        foreach (SectorArrangement sectorType in sectorTypes) {
            int rndIndex = rndSample.Next ();
            SectorInfo sectorInfo = sectors[rndIndex];

            sectorType.Setup (sectorInfo, vertDatabase, sectorInfo.gameObject.transform);
        }
    }

    public void GeneratePlacements (List<IsleInfo> islands) {
        foreach (IsleInfo island in islands) {
            List<int> outline = island.surfaceMeshDetail.outlines.First ();

            int skip = 10;
            if (outline.Count > skip) {
                for (int node = 0; node < outline.Count; node += skip) {
                    int randPosIndex = Random.Range (node, Mathf.Min(node + skip, outline.Count));

                    int vertIndex = outline[randPosIndex];
                    Vector3 pos = island.surfaceMeshDetail.Vertices[vertIndex];
                    GameObject objPlaced = Instantiate (items[0], island.gameObject.transform);
                    objPlaced.transform.localPosition = pos;
                }
            }
        }
    }

    public void GenerateTrees (List<IsleInfo> islands) {
        GameObject holder = new GameObject ("Decor");
        holder.transform.parent = transform;

        foreach (IsleInfo island in islands) {
			MeshFilter mf = island.GetSurfaceMesh (0);

			Vector3[] verts = mf.sharedMesh.vertices;
            int[] indices = mf.sharedMesh.triangles;

            int i = 0;
            while (i < mf.sharedMesh.triangles.Length) {

                Vector3 p1 = verts[indices[i++]];
                Vector3 p2 = verts[indices[i++]];
                Vector3 p3 = verts[indices[i++]];

                Vector3 pos = mf.transform.TransformPoint ((p1 + p2 + p3) / 3);

                //Vector3 n1 = verts[indices[i++]];
                //Vector3 n2 = verts[indices[i++]];
                //Vector3 n3 = verts[indices[i++]];

                //Vector3 rot = mf.transform.TransformDirection ((n1 + n2 + n3) / 3);

                if (Random.value * 100 < probability) {
                    GameObject objPlaced = Instantiate (items[Random.Range (0, items.Length)], holder.transform);

                    objPlaced.transform.position = pos;
                    objPlaced.transform.localRotation = Quaternion.identity;
                }
            }
        }
	}
}
