using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent (typeof (IslandMeshGenerator), typeof (ElevationGenerator))]
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
    public bool debug;
	public CAMethod clusterAnalysis;

    [Header ("Data")]
    public IslandData islandData;
    public NoiseData surfaceNoiseData;
    public NoiseData undersideNoiseData;

    IslandMeshGenerator meshGen;
    LandMap map;
    List<IsleInfo> islands = new List<IsleInfo>();
    List<SectorInfo> sectors = new List<SectorInfo> ();
    TerrainVerticesDatabase vertDatabase = new TerrainVerticesDatabase ();
    bool finished = false;

    public void GenerateIsland (bool inGame = true) {
        #region Generate in Editor
#if UNITY_EDITOR
        if (!inGame) {
            GenerateInEditor ();
            return;
        }
#endif
        #endregion

        StartCoroutine (GenerateInProgress ());
    }
    
    void GenerateInEditor () {
        if (!Application.isPlaying) {
            finished = false;

            if (useRandomSeed) {
                seed = DateTime.Now.ToString ();
            }

            Random.State oldState = Random.state;
            int seedHash = seed.GetHashCode ();
            Random.InitState (seedHash);

            for (int i = 0; i < randCol.Length; i++) {
                randCol[i] = Random.ColorHSV (0, 1, 0, 1, 0.5f, 1);
            }

            map = new LandMap (islandData.maxWidth, islandData.maxHeight);

            // Fill the map randomly with 0s and 1s based on percentage fill
            map.RandomFillMap (islandData.randomFillPercent);

			// Mold to the base shape
			if (islandData.baseShape) {
				map.makeBaseShape (islandData.baseShape);
			}

            // Smooth the map 5 times
            map.SmoothMap (5);

            meshGen = GetComponent<IslandMeshGenerator> ();
            vertDatabase.Clear ();

            // Find separated regions to form an island
            List<MapRegion> regions = map.GetRegions ();
			map.SetTileSize (islandData.tileSize);

			// Create separate islands
			SeparateIslands (regions);

            if (shouldElevate) {
                int highestPeak = 0;
                foreach (IsleInfo island in islands) {
                    int peak = island.surfaceMeshDetail.localPeak;
                    highestPeak = peak > highestPeak ? peak : highestPeak;
                }
                foreach (IsleInfo island in islands) {
                    island.surfaceMeshDetail.NormalizeGradientMap (highestPeak);
                }

                vertDatabase.SetVerticesInlandPos (islands);

                ElevationGenerator elevGen = GetComponent<ElevationGenerator> ();
                elevGen.elevateSurface (islands, islandData.altitude, islandData.mountainCurve, surfaceNoiseData, seedHash, 0, vertDatabase);   // elevate hills on the surface
                elevGen.elevateSurface (islands, -islandData.stalactite, islandData.bellyCurve, undersideNoiseData, seedHash, 2, vertDatabase); // extend stakes at surface below
            }

            // Find strategic locations in each region
            List<MapRegion> zones = map.GetZones (regions, vertDatabase, clusterAnalysis);
			SpliceTerritory (zones);

			SetColliders ();

            PlacementGenerator placement = GetComponent<PlacementGenerator> ();
            if (placement && decorateTerrain) {
                placement.GenerateTrees (islands);
                placement.GenerateSectorsContent (sectors, vertDatabase);
            } else if (placement) {
                //placement.GeneratePlacements (islands);
                placement.GenerateSectorsContent (sectors, vertDatabase);
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

            Random.state = oldState;
        }
    }

    IEnumerator GenerateInProgress () {
        finished = false;

        if (useRandomSeed) {
            seed = DateTime.Now.ToString ();
        }

        Random.State oldState = Random.state;
        int seedHash = seed.GetHashCode ();
        Random.InitState (seedHash);

        yield return new WaitForEndOfFrame ();

        for (int i = 0; i < randCol.Length; i++) {
            randCol[i] = Random.ColorHSV (0, 1, 0, 1, 0.5f, 1);
        }

        yield return new WaitForEndOfFrame ();

        map = new LandMap (islandData.maxWidth, islandData.maxHeight);

        // Fill the map randomly with 0s and 1s based on percentage fill
        map.RandomFillMap (islandData.randomFillPercent);

		// Mold to the base shape
		if (islandData.baseShape) {
			map.makeBaseShape (islandData.baseShape);
		}

		// Smooth the map 5 times
		map.SmoothMap (5);

        yield return new WaitForEndOfFrame ();

        meshGen = GetComponent<IslandMeshGenerator> ();
        vertDatabase.Clear ();

        // Find separated regions to form an island
        List<MapRegion> regions = map.GetRegions ();
		map.SetTileSize (islandData.tileSize);

        yield return new WaitForEndOfFrame ();

        // Create separate islands
        SeparateIslands (regions);

        yield return new WaitForEndOfFrame ();

        if (shouldElevate) {
            int highestPeak = 0;
            foreach (IsleInfo island in islands) {
                int peak = island.surfaceMeshDetail.localPeak;
                highestPeak = peak > highestPeak ? peak : highestPeak;
            }
            foreach (IsleInfo island in islands) {
                island.surfaceMeshDetail.NormalizeGradientMap (highestPeak);
            }

            vertDatabase.SetVerticesInlandPos (islands);

            ElevationGenerator elevGen = GetComponent<ElevationGenerator> ();
            elevGen.elevateSurface (islands, islandData.altitude, islandData.mountainCurve, surfaceNoiseData, seedHash, 0, vertDatabase);   // elevate hills on the surface
            elevGen.elevateSurface (islands, -islandData.stalactite, islandData.bellyCurve, undersideNoiseData, seedHash, 2, vertDatabase); // extend stakes at surface below
        }

        yield return new WaitForEndOfFrame ();

		// Find strategic locations in each region
		List<MapRegion> zones = map.GetZones (regions, vertDatabase, clusterAnalysis);
        SpliceTerritory (zones);

        yield return new WaitForEndOfFrame ();

        SetColliders ();

        yield return new WaitForEndOfFrame ();

        PlacementGenerator placement = GetComponent<PlacementGenerator> ();
        if (placement && decorateTerrain) {
            placement.GenerateTrees (islands);
            placement.GenerateSectorsContent (sectors, vertDatabase);
        } else if (placement) {
            //placement.GeneratePlacements (islands);
            placement.GenerateSectorsContent (sectors, vertDatabase);
        }

        yield return new WaitForEndOfFrame ();

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

        Random.state = oldState;

        finished = true;
        yield return null;
    }

    void SeparateIslands (List<MapRegion> islandRegions) {
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
                    if (island) {                                   ////
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

        int islandCount = 0;
        foreach (MapRegion region in islandRegions) {
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

            List<Mesh> meshes = meshGen.GenerateIslandMesh (region, isle, islandData.tileSize, islandData.depth, vertDatabase);

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
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;

        child.AddComponent<MeshFilter> ();
        child.AddComponent<MeshRenderer> ();

		if(addCollider) {
			child.AddComponent<MeshCollider> ();
		}

        return child;
    }

    void SpliceTerritory (List<MapRegion> zones) {
        sectors.Clear ();

        GameObject territories = new GameObject ("Territories");
        territories.transform.parent = transform;
        territories.transform.localPosition = Vector3.zero;
        territories.transform.localRotation = Quaternion.identity;

        int zoneCount = 1;
        foreach (MapRegion zone in zones) {
            SectorInfo sector = new SectorInfo ();
            sector.id = zoneCount;
            sector.offset = zone.GetCentre () * islandData.tileSize;

            sector.gameObject = AddChildMesh ("Zone " + sector.id, territories.transform);
            sector.gameObject.transform.localPosition = sector.offset;

            sector.gameObject.GetComponent<MeshFilter> ().mesh = meshGen.GenerateZoneMesh (zone, sector, islandData.tileSize, vertDatabase);
            sector.gameObject.GetComponent<MeshRenderer> ().material = islandData.invisibleMaterial;
			sector.gameObject.GetComponent<MeshRenderer> ().material.color = randCol[(zoneCount % 20)];

			cakeslice.Outline outlineComponent = sector.gameObject.AddComponent<cakeslice.Outline> ();
            outlineComponent.color = zoneCount % 3;

            sectors.Add (sector);
            zoneCount++;
        }
        
        territories.AddComponent<CycleRegionOutline> ();
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
	
    public bool IsDone () {
        return finished;
    }

    void OnValuesUpdated () {
        if(!Application.isPlaying) {
			LoggerTool.Post ("Generate island from Editor");
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
        if (debug && map != null) {
            int width = map.spots.GetLength (0);
            int length = map.spots.GetLength (1);
			
			// Draw black & white
			/*for (int x = 0; x < width; x ++) {
				for (int y = 0; y < length; y ++) {
					Gizmos.color = map.spots[x, y].filled ? Color.black : Color.white;
					Vector3 pos = new Vector3(-width/2 + x + .5f,0, -length/2 + y+.5f);
					Gizmos.DrawCube(pos, Vector3.one);
				}
			}*/
			
			// Draw colour for clusters
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < length; y++) {
                    Gizmos.color = !map.spots[x, y].filled ? Color.white : randCol[(map.spots[x, y].areaValue % 20)];
                    Vector3 pos = new Vector3 (-width / 2.0f + x + 0.5f, 150, -length / 2.0f + y + 0.5f) * islandData.tileSize;
                    Gizmos.DrawCube (pos, Vector3.one * islandData.tileSize);
                }
            }
        }
    }
}
