using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainVerticesDatabase {

    public Dictionary<Vector2, TerrainVertData> verticesDictionary = new Dictionary<Vector2, TerrainVertData> ();

    public void AddVertices (List<Vector3> vertices, Vector3 origin, int isleId) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
                verticesDictionary.Add (key, new TerrainVertData ());
            }

            verticesDictionary[key].isleId = isleId;
            verticesDictionary[key].surfaceVertIndex = i;
            verticesDictionary[key].coordinate = vertices[i] + origin;
        }
    }

    public void SetVerticesAltitude (List<Vector3> vertices, Vector3 origin) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
                Debug.Log (key.ToString () + " is not found while setting altitude!");
                continue;
            }

            verticesDictionary[key].altitude = vertices[i].y;
        }
    }

    public void SetVerticesBottomPoint (List<Vector3> vertices, Vector3 origin) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
                Debug.Log (key.ToString () + " is not found while setting bottom point!");
                continue;
            }

            verticesDictionary[key].bottomPoint = vertices[i].y;
        }
    }

    public void SetVerticesSector (List<Vector3> vertices, Vector3 origin, int sectorId) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
                continue;
            }

            vertices[i] = new Vector3 (vertices[i].x, verticesDictionary[key].altitude, vertices[i].z);

            verticesDictionary[key].sectorId = sectorId;
            verticesDictionary[key].sectorVertIndex = i;
        }
    }

    public void SetVerticesInlandPos (List<IsleInfo> islands) {
        foreach (var vertPair in verticesDictionary) {
            vertPair.Value.inlandPosition = islands[vertPair.Value.isleId].surfaceMeshDetail.gradientMap[vertPair.Value.surfaceVertIndex];
        }
    }

    public TerrainVertData GetNearestVertData (Vector3 position) {
        Vector2 hPos = position.ToXZ ();

        Vector2 resultKey = new Vector2 ();
        float minDist = float.MaxValue;

        foreach (var vertPair in verticesDictionary) {
            float distance = Vector2.Distance (hPos, vertPair.Key);

            if (distance < minDist) {
                minDist = distance;
                resultKey = vertPair.Key;
            }

            if (minDist < 0.1f) {
                return verticesDictionary[resultKey];
            }
        }

        return verticesDictionary[resultKey];
    }

    public void Clear () {
        verticesDictionary.Clear ();
    }
}

public class TerrainVertData {
    public int isleId = -1;
    public int surfaceVertIndex = -1;

    public Vector3 coordinate = new Vector3 ();

    public float altitude = 0;
    public float bottomPoint = 0;
    public float inlandPosition = -1;

    public int sectorId = 0;
    public int sectorVertIndex = -1;

    public Vector3 GetSurfacePos () {
        return new Vector3 (coordinate.x, altitude, coordinate.z);
    }
}