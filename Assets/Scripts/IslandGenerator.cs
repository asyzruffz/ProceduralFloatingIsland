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

    [Header ("Settings")]
    public bool withCollider;
    public bool flatShading;
    public bool shouldElevate;
    public bool decorateTerrain;

    [Header ("Data")]
    public IslandData islandData;
    public NoiseData surfaceNoiseData;
    public NoiseData undersideNoiseData;
	
	LandMap map;
    List<IsleInfo> islands = new List<IsleInfo>();

    void Start () {
        GenerateIsland ();
    }
	
    public void GenerateIsland () {
		if (useRandomSeed) {
			seed = System.DateTime.Now.ToString ();
		}

        int seedHash = seed.GetHashCode ();
        System.Random pseudoRandom = new System.Random (seedHash);

        for (int i = 0; i < randCol.Length; i++) {
            randCol[i] = Random.ColorHSV (0, 1, 0, 1, 0.5f, 1);
        }

        map = new LandMap (islandData.maxWidth, islandData.maxHeight);

		// Fill the map randomly with 0s and 1s based on percentage fill
		map.RandomFillMap (ref pseudoRandom, islandData.randomFillPercent);

        // Smooth the map 5 times
		map.SmoothMap (5);

		// Find strategic locations
		List<MapRegion> regions = map.GetRegions ();

		// Create separate islands
		PartitionIslands (regions);

        if (shouldElevate) {
			int highestPeak = 0;
            foreach (IsleInfo island in islands) {
				int peak = island.surfaceMeshRegion.localPeak;
				highestPeak = peak > highestPeak ? peak : highestPeak;
            }
			foreach (IsleInfo island in islands) {
				island.surfaceMeshRegion.NormalizeGradientMap (highestPeak);
			}

			ElevationGenerator elevGen = GetComponent<ElevationGenerator> ();
            elevGen.elevateSurface (islands, islandData.altitude, islandData.mountainCurve, surfaceNoiseData, seedHash, 0); // elevate hills on the surface
            elevGen.elevateSurface (islands, -islandData.stalactite, islandData.bellyCurve, undersideNoiseData, seedHash, 2); // extend stakes at surface below
        }

        SetColliders ();

		PlacementGenerator placement = GetComponent<PlacementGenerator> ();
		if (placement && decorateTerrain) {
			placement.GenerateTrees (islands, ref pseudoRandom);
		} else if (placement) {
            placement.GeneratePlacements (islands);
        }

		if (flatShading) {
            foreach (IsleInfo island in islands) {
                for (int surfaceIndex = 0; surfaceIndex < 3; surfaceIndex++) {
                    MeshFilter mf = island.GetSurfaceMesh (surfaceIndex);
                    float oldVertCount = mf.sharedMesh.vertexCount;
                    mf.sharedMesh = FlatShade.DuplicateSharedVertex (mf.sharedMesh);
                    float newVertCount = mf.sharedMesh.vertexCount;
                    //Debug.Log (mf.transform.parent.name + "." + mf.transform.name + " new vertices are at " + (newVertCount / oldVertCount * 100) + "% with " + newVertCount + " verts.");
                }
            }
        }
    }
	
    void PartitionIslands (List<MapRegion> islandRegions) {
        // Based on regions, create separate child GameObject for each island

        // Destroy all the previous islands
        islands.Clear ();
        var childList = transform.Cast<Transform> ().ToList ();
        foreach (Transform island in childList) {
#if UNITY_EDITOR
			////////////////////////////////////////////////////////////  for debugging, only for in editor
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

		IslandMeshGenerator meshGen = GetComponent<IslandMeshGenerator> ();

        int islandCount = 1;
        foreach(MapRegion region in islandRegions) {
            IsleInfo isle = new IsleInfo ();
            isle.id = islandCount;

            // Create each isle game object
            isle.gameObject = new GameObject ("Island " + isle.id);
            isle.gameObject.transform.parent = transform;
            isle.gameObject.transform.localRotation = Quaternion.identity;
            isle.offset = region.GetCentre () * islandData.tileSize;
            isle.gameObject.transform.localPosition = isle.offset;
            
            // Child game object of isle to store surface
            GameObject surface = AddChildMesh ("Surface", isle.gameObject.transform, withCollider);
            // Child game object of isle to store wall
            GameObject wall = AddChildMesh ("Wall", isle.gameObject.transform);
            // Child game object of isle to store underside
            GameObject underside = AddChildMesh ("Underside", isle.gameObject.transform);
            underside.transform.position += Vector3.up * -islandData.depth;
            
            List<Mesh> meshes = meshGen.GenerateMesh (region, isle, islandData.tileSize, islandData.depth);
            
            // Mesh for surface
            surface.GetComponent<MeshFilter> ().mesh = meshes[0];
            surface.GetComponent<MeshRenderer> ().material = islandData.grassMaterial;

            // Mesh for wall
            wall.GetComponent<MeshFilter> ().mesh = meshes[1];
            wall.GetComponent<MeshRenderer> ().material = islandData.wallMaterial;

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

                Mesh collMesh = new Mesh ();
                collMesh.vertices = meshFilter.sharedMesh.vertices;
                collMesh.triangles = meshFilter.sharedMesh.triangles;
                collMesh.RecalculateNormals ();

                colliders[i].sharedMesh = collMesh;
			}
		}
	}
	
    void OnValuesUpdated () {
        if(!Application.isPlaying) {
            GenerateIsland ();
        }
    }

    void OnValidate () {
        if (surfaceNoiseData != null) {
            surfaceNoiseData.OnValuesUpdated -= OnValuesUpdated;
            surfaceNoiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (islandData != null) {
            islandData.OnValuesUpdated -= OnValuesUpdated;
            islandData.OnValuesUpdated += OnValuesUpdated;
        }
    }

    Color[] randCol = new Color[20];

    void OnDrawGizmos() {
        if (map != null) {
            int width = map.spots.GetLength (0);
            int length = map.spots.GetLength (1);

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < length; y++) {
                    Gizmos.color = map.spots[x, y].areaValue == 0 ? Color.white : randCol[Mathf.Max(0, (map.spots[x, y].areaValue - 1) % 20)];
                    Vector3 pos = new Vector3 (-width / 2.0f + x + 0.5f, 150, -length / 2.0f + y + 0.5f) * islandData.tileSize;
                    Gizmos.DrawCube (pos, Vector3.one * islandData.tileSize);
                }
            }
        }
    }
}
