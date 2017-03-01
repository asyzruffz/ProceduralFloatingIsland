using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (IslandMeshGenerator))]
public class IslandGenerator : MonoBehaviour {

    public bool autoUpdate = true;

	[Header ("Randomness")]
	public bool useRandomSeed;
    [ConditionalHide ("useRandomSeed", false, true)]
    public string seed;

    [Header ("Data")]
    public NoiseData noiseData;
    public IslandData islandData;

	[Header ("Settings")]
	public bool withCollider;

    int[,] map;
    int[,] regionMap;
    List<IsleInfo> islands = new List<IsleInfo>();

    void Start () {
        GenerateIsland ();
    }
	
	void Update () {
		if(Input.GetButtonDown("Fire1")) {
            GenerateIsland ();
        }
	}

    public void GenerateIsland () {
        map = new int[islandData.maxWidth, islandData.maxHeight];
        regionMap = new int[islandData.maxWidth, islandData.maxHeight];

        // Fill the map randomly with 0s and 1s based on percentage fill
        RandomFillMap ();

        // Smooth the map 5 times
        for(int i = 0; i < 5; i++) {
            SmoothMap ();
        }

        // Create separate islands
        PartitionIslands ();

        ElevationGenerator elevGen = GetComponent<ElevationGenerator> ();
        elevGen.GenerateElevation (islands, islandData.altitude, noiseData, seed.GetHashCode());

		SetColliders ();
    }

    void RandomFillMap () {
        // Fill the map randomly with 0s and 1s based on percentage fill

        if (useRandomSeed) {
            seed = Time.time.ToString ();
        }

        System.Random pseudoRandom = new System.Random (seed.GetHashCode());

        for (int x = 0; x < islandData.maxWidth; x++) {
            for (int y = 0; y < islandData.maxHeight; y++) {
                if (x == 0 || y == 0 || x == islandData.maxWidth - 1 || y == islandData.maxHeight - 1) {
                    map[x, y] = 0;
                } else {
                    map[x, y] = pseudoRandom.Next (0, 100) < islandData.randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap() {
        // Change the state in each cell within the cellular automaton based on its neighbour

        for (int x = 0; x < islandData.maxWidth; x++) {
            for (int y = 0; y < islandData.maxHeight; y++) {
                int neighbourLandTile = GetSurroundingLandCount (x, y);

                if (neighbourLandTile > 4) {
                    map[x, y] = 1;
                } else if (neighbourLandTile < 4) {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingLandCount(int gridX, int gridY) {
        // Count the adjacent cell which is a land
        int landCount = 0;

        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
                
                if (IsInMapRange (neighbourX, neighbourY)) {
                    if (neighbourX != gridX || neighbourY != gridY) { // not checking the middle
                        landCount += map[neighbourX, neighbourY];
                    }
                }
            }
        }

        return landCount;
    }

    void PartitionIslands() {
        // Based on regions, create separate child GameObject for each island

        // Destroy all the previous islands
        islands.Clear ();
        var childList = transform.Cast<Transform> ().ToList ();
        foreach (Transform island in childList) {
#if UNITY_EDITOR
			////////////////////////////////////////////////////////////  for debugging, onnly for in editor
			if (!Application.isPlaying) {                           ////
                UnityEditor.EditorApplication.delayCall += () =>    ////
                {                                                   ////
					if (island) {									////
						DestroyImmediate (island.gameObject);       ////
					}												////
                };                                                  ////
            } else {                                                ////
			////////////////////////////////////////////////////////////
#endif
				Destroy (island.gameObject);
#if UNITY_EDITOR
			////////////////////////////////////////////////////////////
			}                                                       ////
			////////////////////////////////////////////////////////////
#endif
		}

		List<List<Coord>> islandRegions = GetRegions (1);
        IslandMeshGenerator meshGen = GetComponent<IslandMeshGenerator> ();

        int islandCount = 1;
        foreach(List<Coord> region in islandRegions) {
            IsleInfo isle = new IsleInfo ();
            isle.id = islandCount;

            // Create each isle game object
            isle.gameObject = new GameObject ("Island " + isle.id);
            isle.gameObject.transform.parent = transform;
            isle.gameObject.transform.localRotation = Quaternion.identity;
            isle.offset = GetRegionCentre (region);
            isle.gameObject.transform.localPosition = isle.offset * islandData.tileSize;
            
            // Child game object of isle to store surface
            GameObject surface = AddChildMesh ("Surface", isle.gameObject.transform, true);
            // Child game object of isle to store wall
            GameObject wall = AddChildMesh ("Wall", isle.gameObject.transform);
            // Child game object of isle to store underside
            GameObject underside = AddChildMesh ("Underside", isle.gameObject.transform);
            underside.transform.position += Vector3.up * -islandData.depth;

            List<Mesh> meshes = meshGen.GenerateMesh (regionMap, isle, islandData.tileSize, islandData.depth);

            // Mesh for surface
            surface.GetComponent<MeshFilter> ().mesh = meshes[0];
            surface.GetComponent<MeshRenderer> ().material = islandData.grassMaterial;

            // Mesh for wall
            wall.GetComponent<MeshFilter> ().mesh = meshes[1];
            wall.GetComponent<MeshRenderer> ().material = islandData.dirtMaterial;

            // Mesh for underside
            underside.GetComponent<MeshFilter> ().mesh = meshes[2];
            underside.GetComponent<MeshRenderer> ().material = islandData.dirtMaterial;
			
            islands.Add (isle);
            islandCount++;
        }
    }

    GameObject AddChildMesh(string name, Transform parent, bool addCollider = false) {
        GameObject child = new GameObject (name);
        child.transform.parent = parent;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localPosition = Vector3.zero;

        child.AddComponent<MeshFilter> ();
        child.AddComponent<MeshRenderer> ();

		if(addCollider) {
			child.AddComponent<MeshCollider> ();
		}

        return child;
    }

	void SetColliders () {
		foreach (IsleInfo isle in islands) {
			MeshCollider[] colliders = isle.gameObject.GetComponentsInChildren<MeshCollider> ();
			for (int i = 0; i < colliders.Length; i++) {
				MeshFilter meshFilter = colliders[i].GetComponent<MeshFilter> ();
				colliders[i].sharedMesh = meshFilter.sharedMesh;
			}
		}
	}
	
    List<List<Coord>> GetRegions(int tileType) {
        // Get all regions of a tile type

        List<List<Coord>> regions = new List<List<Coord>> ();
        int[,] mapFlags = new int[islandData.maxWidth, islandData.maxHeight]; // marked tiles that's already visited

        int islandId = 1;
        for (int x = 0; x < islandData.maxWidth; x++) {
            for (int y = 0; y < islandData.maxHeight; y++) {
                if(mapFlags[x, y] == 0 && map[x, y] == tileType) {
                    List<Coord> newRegion = GetRegionTiles (x, y);
                    regions.Add (newRegion);

                    foreach(Coord tile in newRegion) {
                        mapFlags[tile.x, tile.y] = 1;
                        regionMap[tile.x, tile.y] = islandId;
                    }
                    islandId++;
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY) {
        // Flood fill algorithm to find the coord encompassing a region

        List<Coord> tiles = new List<Coord> ();
        int[,] mapFlags = new int[islandData.maxWidth, islandData.maxHeight]; // marked tiles that's already visited
        int tileType = map[startX, startY]; // start tile determine the other tiles to be checked

        Queue<Coord> queue = new Queue<Coord> ();
        queue.Enqueue (new Coord (startX, startY));
        mapFlags[startX, startY] = 1;

        while(queue.Count > 0) {
            Coord tile = queue.Dequeue ();
            tiles.Add (tile);

            for (int x = tile.x - 1; x <= tile.x + 1; x++) {
                for (int y = tile.y - 1; y <= tile.y + 1; y++) {

                    if (IsInMapRange (x, y) && (x == tile.x || y == tile.y)) { // ignore diagonally
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType) {
                            mapFlags[x, y] = 1;
                            queue.Enqueue (new Coord (x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    Vector3 GetRegionCentre(List<Coord> region) {
        if(!region.Any()) {
            return Vector3.zero;
        }

        float minX = region.Min (coord => coord.x);
        float minY = region.Min (coord => coord.y);
        float maxX = region.Max (coord => coord.x);
        float maxY = region.Max (coord => coord.y);

        return new Vector3 ((minX + maxX - islandData.maxWidth) / 2, 0, (minY + maxY - islandData.maxWidth) / 2);
    }

    bool IsInMapRange(int x, int y) {
        return x >= 0 && y >= 0 && x < islandData.maxWidth && y < islandData.maxHeight;
    }
    
    void OnValuesUpdated () {
        if(!Application.isPlaying) {
            GenerateIsland ();
        }
    }

    void OnValidate () {
        if (noiseData != null) {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (islandData != null) {
            islandData.OnValuesUpdated -= OnValuesUpdated;
            islandData.OnValuesUpdated += OnValuesUpdated;
        }
    }

    struct Coord {
        public int x, y;

        public Coord(int tileX, int tileY) {
            x = tileX;
            y = tileY;
        }
    }
}
