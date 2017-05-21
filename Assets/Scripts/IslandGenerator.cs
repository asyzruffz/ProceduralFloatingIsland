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

    [Header ("Data")]
    public IslandData islandData;
    public NoiseData surfaceNoiseData;
    public NoiseData undersideNoiseData;
	
	LandMap map;
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
		if (useRandomSeed) {
			seed = System.DateTime.Now.ToString ();
		}

		System.Random pseudoRandom = new System.Random (seed.GetHashCode ());

		map = new LandMap (islandData.maxWidth, islandData.maxHeight);

		// Fill the map randomly with 0s and 1s based on percentage fill
		map.RandomFillMap (ref pseudoRandom, islandData.randomFillPercent);

        // Smooth the map 5 times
        for(int i = 0; i < 5; i++) {
			map.SmoothMap ();
        }

        // Create separate islands
        PartitionIslands ();

        ElevationGenerator elevGen = GetComponent<ElevationGenerator> ();
        elevGen.elevateSurface (islands, islandData.altitude, surfaceNoiseData, seed.GetHashCode(), 0); // elevate hills on the surface
        elevGen.elevateSurface (islands, -islandData.stalactite, undersideNoiseData, seed.GetHashCode(), 2); // extend stakes at surface below

        SetColliders ();
        
        if(flatShading) {
            for(int surfaceIndex = 0; surfaceIndex < 3; surfaceIndex++) {
                List<MeshFilter> meshFilters = IsleInfo.GetSurfaceMeshes (islands, surfaceIndex);
                for (int i = 0; i < meshFilters.Count; i++) {
                    float oldVertCount = meshFilters[i].sharedMesh.vertexCount;
                    meshFilters[i].sharedMesh = FlatShade.DuplicateSharedVertex (meshFilters[i].sharedMesh);
                    float newVertCount = meshFilters[i].sharedMesh.vertexCount;
                    Debug.Log (meshFilters[i].transform.parent.name + "." + meshFilters[i].transform.name + " new vertices are at " + (newVertCount / oldVertCount * 100) + "% with " + newVertCount + " verts.");
                }
            }
        }

		PlacementGenerator placement = GetComponent<PlacementGenerator> ();
		if (placement) {
			placement.GeneratePlacement (islands);
		}
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

		List<MapRegion> islandRegions = map.GetRegions (1);
        IslandMeshGenerator meshGen = GetComponent<IslandMeshGenerator> ();

        int islandCount = 1;
        foreach(MapRegion region in islandRegions) {
            IsleInfo isle = new IsleInfo ();
            isle.id = islandCount;

            // Create each isle game object
            isle.gameObject = new GameObject ("Island " + isle.id);
            isle.gameObject.transform.parent = transform;
            isle.gameObject.transform.localRotation = Quaternion.identity;
            isle.offset = region.GetCentre ();
            isle.gameObject.transform.localPosition = isle.offset * islandData.tileSize;
            
            // Child game object of isle to store surface
            GameObject surface = AddChildMesh ("Surface", isle.gameObject.transform, withCollider);
            // Child game object of isle to store wall
            GameObject wall = AddChildMesh ("Wall", isle.gameObject.transform);
            // Child game object of isle to store underside
            GameObject underside = AddChildMesh ("Underside", isle.gameObject.transform);
            underside.transform.position += Vector3.up * -islandData.depth;

            List<Mesh> meshes = meshGen.GenerateMesh (map.GetRegionMap (), isle, islandData.tileSize, islandData.depth);

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
}
