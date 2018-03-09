using System.Collections.Generic;
using UnityEngine;

public class TerrainVerticesDatabase {

	public Dictionary<Vector2, TerrainVertData> verticesDictionary = new Dictionary<Vector2, TerrainVertData> ();
    public Dictionary<UniqueCoord, TerrainVertData> coordDictionary = new Dictionary<UniqueCoord, TerrainVertData> ();
	public float tileSize;

    public void AddVertices (List<Vector3> vertices, List<Coord> coords, Vector3 origin, int isleId) {

		for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToVec2FromXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
                verticesDictionary.Add (key, new TerrainVertData ());
            }

            verticesDictionary[key].isleId = isleId;
            verticesDictionary[key].surfaceVertIndex = i;
            verticesDictionary[key].localOrigin = origin;
			verticesDictionary[key].coordinate = vertices[i] + origin;
			verticesDictionary[key].tileCoord = coords[i];
		}
    }

	public void SetCoordDB () {
		coordDictionary.Clear ();
		foreach (var vertPair in verticesDictionary) {
			UniqueCoord keyCoord = new UniqueCoord (vertPair.Value.tileCoord, vertPair.Value.isleId);
			if (!coordDictionary.ContainsKey (keyCoord)) {
				coordDictionary.Add (keyCoord, vertPair.Value);
			}
		}
	}

	public void SetVerticesAltitude (List<Vector3> vertices, Vector3 origin) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToVec2FromXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
				LoggerTool.Post (key.ToString () + " is not found while setting altitude!");
                continue;
            }

            verticesDictionary[key].altitude = vertices[i].y;
        }
    }

    public void SetVerticesBottomPoint (List<Vector3> vertices, Vector3 origin) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToVec2FromXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
				LoggerTool.Post (key.ToString () + " is not found while setting bottom point!");
                continue;
            }

            verticesDictionary[key].bottomPoint = vertices[i].y;
        }
    }

    public void SetVerticesSector (List<Vector3> vertices, Vector3 origin, int sectorId) {
        for (int i = 0; i < vertices.Count; i++) {
            Vector2 key = (vertices[i] + origin).ToVec2FromXZ ();

            if (!verticesDictionary.ContainsKey (key)) {
                continue;
            }

            vertices[i] = new Vector3 (vertices[i].x, verticesDictionary[key].altitude, vertices[i].z);

            verticesDictionary[key].sectorId = sectorId;
            verticesDictionary[key].sectorVertIndex = i;
        }
    }

    public void SetVerticesInlandPos (List<IsleInfo> islands, AnimationCurve curve) {
        foreach (var vertPair in verticesDictionary) {
            vertPair.Value.inlandPosition = islands[vertPair.Value.isleId].surfaceMeshDetail.gradientMap[vertPair.Value.surfaceVertIndex];
            vertPair.Value.surfaceElevatedCurve = curve.Evaluate (vertPair.Value.inlandPosition);
		}
    }

	public TerrainVertData GetNearestVertData (Vector2 position) {
		Vector2 resultKey = new Vector2 ();
		float minDist = float.MaxValue;

		foreach (var vertPair in verticesDictionary) {
			float distance = Vector2.Distance (position, vertPair.Key);

			if (distance < minDist) {
				minDist = distance;
				resultKey = vertPair.Key;

				if (minDist < 0.1f) {
					return vertPair.Value;
				}
			}
		}

		return verticesDictionary[resultKey];
	}

	public TerrainVertData GetNearestVertData (Vector3 position) {
        Vector2 hPos = position.ToVec2FromXZ ();
		return GetNearestVertData (hPos);
    }

	public TerrainVertData GetVertDataFromRegionTile (Coord tile, int isleId) {
		/*Vector2 hPos = tile.ToVector2 () * tileSize;
		float minDist = float.MaxValue;
		Vector2 resultKey = new Vector2 ();
		
		foreach (var vertPair in verticesDictionary) {
			Vector2 vertex = (vertPair.Value.coordinate - vertPair.Value.localOrigin).ToVec2FromXZ ();
			float distance = Vector2.Distance (hPos, vertex);

			if (distance < minDist) {
				minDist = distance;
				resultKey = vertPair.Key;

				if (minDist < 0.1f) {
					return vertPair.Value;
				}
			}
		}

		return verticesDictionary[resultKey];*/

		UniqueCoord keyCoord = new UniqueCoord(tile, isleId);
		TerrainVertData result;

		if (coordDictionary.TryGetValue(keyCoord, out result)) {
			return result;
		} else {
			return null;
		}
	}

	public void Clear () {
        verticesDictionary.Clear ();
    }
}

public class TerrainVertData {
    public int isleId = -1;
    public int surfaceVertIndex = -1;

    public Vector3 localOrigin;
    public Vector3 coordinate;
	public Coord tileCoord;

	public float altitude = 0;
    public float bottomPoint = 0;
    public float inlandPosition = -1;
	public float surfaceElevatedCurve = 0;

    public int sectorId = 0;
    public int sectorVertIndex = -1;

    public Vector3 GetSurfacePos () {
        return new Vector3 (coordinate.x, altitude, coordinate.z);
	}

	public Vector3 GetSurfacePosNormalized () {
		return new Vector3 (coordinate.x, inlandPosition, coordinate.z);
	}
}

public struct UniqueCoord {
	Coord value;
	int id;

	public UniqueCoord (Coord val, int idNum) {
		value = val;
		id = idNum;
	}

	public bool Equals (UniqueCoord other) {
		return (value == other.value) && (id == other.id);
	}
}