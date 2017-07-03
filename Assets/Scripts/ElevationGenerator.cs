using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationGenerator : MonoBehaviour {

    List<IsleInfo> islands;

    public void elevateSurface (List<IsleInfo> islandInfos, float altitude, NoiseData noiseData, int seed, int surfaceIndex) {
        islands = islandInfos;

        foreach (IsleInfo island in islands) {
            MeshFilter mf = island.GetSurfaceMesh (surfaceIndex);

            List<Vector3> verts = ApplyHeight (island.surfaceMeshRegion, altitude, noiseData, seed);
            //verts = FlattenAtBorder (verts, island.surfaceMeshRegion.outlines);

            mf.sharedMesh.vertices = verts.ToArray ();
            mf.sharedMesh.RecalculateNormals ();
        }
    }

    List<Vector3> ApplyHeight (MeshRegion meshReg, float altitude, NoiseData noiseData, int seed) {
        List<Vector3> newVertices = new List<Vector3> ();
        
        // Get the rectangle region of the island
        Rect lot = meshReg.GetRectContainingVertices ();

        Dictionary<int, int> gradientMap = meshReg.gradientMap;

        float[,] noiseMap = Noise.GenerateNoiseMap (Mathf.RoundToInt (lot.width + 1), Mathf.RoundToInt (lot.height + 1), noiseData, seed);

        Debug.Log ("GradMap size " + gradientMap.Count + ", Verts size " + meshReg.Vertices.Count);
        for (int i = 0; i < meshReg.Vertices.Count; i++) {
            Vector3 vertexPos = meshReg.Vertices[i];

            int mapX = Mathf.RoundToInt (Mathf.InverseLerp (lot.xMin, lot.xMax, vertexPos.x) * lot.width);
            int mapY = Mathf.RoundToInt (Mathf.InverseLerp (lot.yMin, lot.yMax, vertexPos.z) * lot.height);

            vertexPos.y = noiseMap[mapX, mapY] * altitude;

            if (gradientMap.ContainsKey(i)) {
                vertexPos.y *= gradientMap[i];
            } else {
                vertexPos.y *= 20;
            }

            newVertices.Add(vertexPos);
        }

        return newVertices;
    }

    List<Vector3> FlattenAtBorder (List<Vector3> vertices, List<List<int>> outlines) {
        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count; i++) {
                vertices[outline[i]] = new Vector3(vertices[outline[i]].x, 0, vertices[outline[i]].z);
            }
        }

        return vertices;
    }
}
