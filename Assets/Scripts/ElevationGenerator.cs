using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationGenerator : MonoBehaviour {

    List<IsleInfo> islands;

    public void elevateSurface (List<IsleInfo> islandInfos, float altitude, NoiseData noiseData, int seed, int surfaceIndex) {
        islands = islandInfos;

        List<MeshFilter> surfaceMeshes = IsleInfo.GetSurfaceMeshes (islands, surfaceIndex);
        for(int i = 0; i < surfaceMeshes.Count; i++) {
            List<Vector3> verts = ApplyHeight (surfaceMeshes[i].sharedMesh.vertices, altitude, noiseData, seed);
            verts = FlattenAtBorder (verts, i);
            surfaceMeshes[i].sharedMesh.vertices = verts.ToArray ();
            surfaceMeshes[i].sharedMesh.RecalculateNormals();
        }
    }

    List<Vector3> ApplyHeight (Vector3[] vertices, float altitude, NoiseData noiseData, int seed) {
        List<Vector3> newVertices = new List<Vector3> ();
        
        // Get the rectangle region of the island
        Rect lot = GetRectContainingVertices (vertices);

        float[,] noiseMap = Noise.GenerateNoiseMap (Mathf.RoundToInt (lot.width + 1), Mathf.RoundToInt (lot.height + 1), noiseData, seed);

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 vertexPos = vertices[i];

            int mapX = Mathf.RoundToInt (Mathf.InverseLerp (lot.xMin, lot.xMax, vertexPos.x) * lot.width);
            int mapY = Mathf.RoundToInt (Mathf.InverseLerp (lot.yMin, lot.yMax, vertexPos.z) * lot.height);

            vertexPos.y = noiseMap[mapX, mapY] * altitude;
            newVertices.Add(vertexPos);
        }

        return newVertices;
    }

    List<Vector3> FlattenAtBorder (List<Vector3> vertices, int index) {
        foreach (List<int> outline in islands[index].surfaceMeshRegion.outlines) {
            for (int i = 0; i < outline.Count; i++) {
                vertices[outline[i]] = new Vector3(vertices[outline[i]].x, 0, vertices[outline[i]].z);
            }
        }

        return vertices;
    }

    Rect GetRectContainingVertices(Vector3[] vertices) {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < vertices.Length; i++) {
            if (vertices[i].x > maxX) {
                maxX = vertices[i].x;
            } else if (vertices[i].x < minX) {
                minX = vertices[i].x;
            }

            if (vertices[i].z > maxY) {
                maxY = vertices[i].z;
            } else if (vertices[i].z < minY) {
                minY = vertices[i].z;
            }
        }

        return new Rect (minX, minY, maxX - minX, maxY - minY);
    }
}
