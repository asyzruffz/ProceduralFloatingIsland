using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationGenerator : MonoBehaviour {

	List<IsleInfo> islands;

    public void elevateSurface (List<IsleInfo> islandInfos, float altitude, AnimationCurve curve, NoiseData noiseData, int seed, int surfaceIndex, TerrainVerticesDatabase vertDatabase) {
        islands = islandInfos;

        foreach (IsleInfo island in islands) {
            MeshFilter mf = island.GetSurfaceMesh (surfaceIndex);

            List<Vector3> verts = ApplyHeight (island.surfaceMeshDetail, altitude, curve, noiseData, seed);

            mf.sharedMesh.vertices = verts.ToArray ();
            mf.sharedMesh.RecalculateNormals ();

            if (surfaceIndex == 0) {
                vertDatabase.SetVerticesAltitude (verts, transform.position + island.offset);
            } else if (surfaceIndex == 2) {
                vertDatabase.SetVerticesBottomPoint (verts, transform.position + island.offset);
            }
        }
    }

	List<Vector3> ApplyHeight (IsleMeshDetail meshReg, float altitude, AnimationCurve curve, NoiseData noiseData, int seed) {
		List<Vector3> newVertices = new List<Vector3> ();

		// Get the rectangle region of the island
		Rect lot = meshReg.GetRectContainingVertices ();

		float[,] noiseMap = Noise.GenerateNoiseMap (Mathf.RoundToInt (lot.width + 2), Mathf.RoundToInt (lot.height + 2), noiseData, seed);

		for (int i = 0; i < meshReg.Vertices.Count; i++) {
			Vector3 vertexPos = meshReg.Vertices[i];

			int mapX = Mathf.RoundToInt (Mathf.InverseLerp (lot.xMin, lot.xMax, vertexPos.x) * lot.width);
			int mapY = Mathf.RoundToInt (Mathf.InverseLerp (lot.yMin, lot.yMax, vertexPos.z) * lot.height);

			if (meshReg.gradientMap.ContainsKey (i)) {
				vertexPos.y = curve.Evaluate (meshReg.gradientMap[i]) * altitude;
			} else {
				vertexPos.y = 0;
			}

			vertexPos.y *= noiseMap[mapX, mapY];

			newVertices.Add (vertexPos);
		}

		return newVertices;
	}
}
