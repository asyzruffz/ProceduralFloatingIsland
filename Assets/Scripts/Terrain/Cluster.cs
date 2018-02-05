using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Cluster {

    List<MapRegion> regions;
	float tileSize;
    
    public Cluster (List<MapRegion> regs, float tlSize) {
        regions = regs;
		tileSize = tlSize;
    }
	
	// Called by LandMap.GetZones (), returns number of subregions
	public int ClusterLocationsKMeans (MapPoint[,] points) {
		// K-means cluster algorithm to separate locations in the regions

		int regionId = 0;
		foreach (MapRegion region in regions) {
			int k = Mathf.RoundToInt (Mathf.Sqrt (region.turf.Count / 9.0f));
			k = Mathf.Max (1, k);
			//Debug.Log (k + " centroid(s)");

			Vector2[] centroids = new Vector2[k];
			for (int i = 0; i < k; i++) {
				// Assign centroid to first three data points
				centroids[i] = region.turf[i * (region.turf.Count / k)].ToVector2 ();
			}

			// Loop until converged
			int changes = -1;
			int iter = 0;
			while (changes != 0) {
				changes = 0;

				foreach (Coord tile in region.turf) {
					Vector2 tilePos = tile.ToVector2 ();

					int initialArea = points[tile.x, tile.y].areaValue;
					float distanceToCentroid = float.MaxValue;

					for (int i = 0; i < k; i++) {
						//float currDistToCentroid = centroids[i].ManhattanDist (tilePos);
						float currDistToCentroid = Vector2.SqrMagnitude (centroids[i] - tilePos);
						//currDistToCentroid += ObtainDistancePenalty (tile, new Coord (Mathf.RoundToInt(centroids[i].x), Mathf.RoundToInt (centroids[i].y)), 2);
						if (currDistToCentroid < distanceToCentroid) {
							distanceToCentroid = currDistToCentroid;
							points[tile.x, tile.y].areaValue = regionId + i;
						}
					}

					if (initialArea != points[tile.x, tile.y].areaValue) {
						changes++;
					}
				}

				Vector2[] cumulativeCentroids = new Vector2[k];
				int[] frequency = new int[k];
				foreach (Coord tile in region.turf) {
					cumulativeCentroids[Mathf.Max (0, points[tile.x, tile.y].areaValue - regionId)] += tile.ToVector2 ();
					frequency[Mathf.Max (0, points[tile.x, tile.y].areaValue - regionId)]++;
				}

				for (int i = 0; i < k; i++) {
					centroids[i] = cumulativeCentroids[i] / frequency[i];
				}
				iter++;
			}
			//Debug.Log ("Iteration: " + iter);
			regionId += k;
		}

		return regionId;
	}

	// Called by LandMap.GetZones (), returns number of subregions
	public int ClusterLocationsKMeans (MapPoint[,] points, TerrainVerticesDatabase vertDatabase) {
		// K-means cluster algorithm to separate locations in the regions

		int regionId = 0;
		foreach (MapRegion region in regions) {
			Vector3[] tileLocations = new Vector3[region.turf.Count];
			for (int i = 0; i < tileLocations.Length; i++) {
				TerrainVertData vertData = vertDatabase.GetVertDataFromRegionTile (region.turf[i], tileSize);
				if (vertData != null) {
					tileLocations[i] = new Vector3 (region.turf[i].x, vertData.inlandPosition, region.turf[i].y);
				} else {
					LoggerTool.Post ("Null from VertDB for " + region.turf[i].ToString ());
				}
			}

			int k = Mathf.RoundToInt (Mathf.Sqrt (tileLocations.Length / 16.0f));
			k = Mathf.Max (1, k);
			Debug.Log (k + " centroid(s)");

			Vector3[] centroids = new Vector3[k];
			for (int i = 0; i < k; i++) {
				// Assign centroid to first k data points
				centroids[i] = tileLocations[i * (tileLocations.Length / k)];
				//centroids[i] = tileLocations[i];
			}

			// Loop until converged
			int changes = -1;
			int iter = 0;
			while (changes != 0 && iter < 1) {
				changes = 0;

				for (int tIndex = 0; tIndex < tileLocations.Length; tIndex++) {
					Coord tile = region.turf[tIndex];

					int initialArea = points[tile.x, tile.y].areaValue;
					float distanceToCentroid = float.MaxValue;

					for (int i = 0; i < k; i++) {
						//float currDistToCentroid = centroids[i].ManhattanDist (tileLocations[tIndex]);
						//float currDistToCentroid = Vector3.Distance (centroids[i], tileLocations[tIndex]);
						float currDistToCentroid = Vector2.Distance (centroids[i].ToVec2FromXZ (), tileLocations[tIndex].ToVec2FromXZ ());
						if (currDistToCentroid < distanceToCentroid) {
							distanceToCentroid = currDistToCentroid;
							points[tile.x, tile.y].areaValue = regionId + i;
						}
					}

					if (initialArea != points[tile.x, tile.y].areaValue) {
						changes++;
					}
				}

				int[] frequency = new int[k];
				Vector3[] cumulativeCentroids = new Vector3[k];
				for (int i = 0; i < k; i++) {
					frequency[i] = 0;
					cumulativeCentroids[i] = Vector3.zero;
				}

				for (int i = 0; i < tileLocations.Length; i++) {
					Coord tile = region.turf[i];
					cumulativeCentroids[Mathf.Max (0, points[tile.x, tile.y].areaValue - regionId)] += tileLocations[i];
					frequency[Mathf.Max (0, points[tile.x, tile.y].areaValue - regionId)]++;
				}

				for (int i = 0; i < k; i++) {
					if (frequency[i] != 0) {
						centroids[i] = cumulativeCentroids[i] / frequency[i];
					} else {
						centroids[i] = tileLocations[Random.Range (0, tileLocations.Length)];
					}
				}

				iter++;
			}

			for (int i = 0; i < k; i++) {
				Debug.Log ("Centroid " + (regionId + i) + " at " + centroids[i].ToString ());
			}

			LoggerTool.Post ("No of iteration: " + iter);
			regionId += k;
		}

		return regionId;
	}

	// Called by LandMap.GetZones (), returns number of subregions
	public int ClusterLocationsDBSCAN (float epsilon, int minPts, MapPoint[,] points) {
        // DBSCAN cluster algorithm to separate locations in the regions

        Dictionary<Coord, ClusterPoint> clusteredPts = new Dictionary<Coord, ClusterPoint> ();
        foreach (MapRegion region in regions) {
            foreach (Coord tile in region.turf) {
                if (!clusteredPts.ContainsKey (tile)) {
                    clusteredPts.Add (tile, new ClusterPoint ());
                } else {
					LoggerTool.Post ("DBSCAN: Duplicate of tile found! (" + tile.x + "," + tile.y + ")");
                }
            }
        }
        
        int regionId = 0;
        foreach (MapRegion region in regions) {
            foreach (Coord tile in region.turf) {
                ClusterPoint cPt;
                if (clusteredPts[tile].done) {
                    continue;
                }

                List<Coord> n = RangeQueries (region.turf, tile, epsilon);
                if (n.Count + 1 < minPts) {
                    cPt = clusteredPts[tile];
                    cPt.regionNumber = -1;
                    points[tile.x, tile.y].areaValue = -1;
                    cPt.done = true;
                    clusteredPts[tile] = cPt;
					LoggerTool.Post ("Outlier", true, false);
                    continue;
                } else {
					LoggerTool.Post ("Zone " + regionId);
                    cPt = clusteredPts[tile];
                    cPt.regionNumber = regionId;
                    points[tile.x, tile.y].areaValue = regionId;
                    cPt.done = true;
                    clusteredPts[tile] = cPt;
                }

                for (int i = 0; i < n.Count; i++) {
                    Coord q = n[i];
                    if (clusteredPts[q].regionNumber == -1) {
                        cPt = clusteredPts[q];
                        cPt.regionNumber = regionId;
                        points[q.x, q.y].areaValue = regionId;
                        cPt.done = true;
                        clusteredPts[q] = cPt;
                    }

                    if (clusteredPts[q].done) {
                        continue;
                    }

                    cPt = clusteredPts[q];
                    cPt.regionNumber = regionId;
                    points[q.x, q.y].areaValue = regionId;
                    cPt.done = true;
                    clusteredPts[q] = cPt;

                    List<Coord> nb = RangeQueries (region.turf, q, epsilon);
                    if (nb.Count >= minPts) {
                        n.AddRange (nb);
                    }
                }

                regionId++;
            }
        }

        return regionId + 1;
    }

	// Called by LandMap.GetZones (), returns number of subregions
	public int ClusterLocationsKMedoidsPAM (MapPoint[,] points) {
		// K-medoids cluster algorithm to separate locations in the regions

		ExecutionTimer clock = new ExecutionTimer ();

		int regionId = 0;
		foreach (MapRegion region in regions) {
			int k = Mathf.RoundToInt (Mathf.Sqrt (region.turf.Count / 16.0f));
			k = Mathf.Max (1, k);
			Debug.Log (k + " medoid(s)");

			Coord[] medoids = new Coord[k];
			bool[] isMedoid = new bool[region.turf.Count];
			for (int i = 0; i < k; i++) {
				// Assign medoid to first k data points
				medoids[i] = region.turf[i * (region.turf.Count / k)];
				isMedoid[i * (region.turf.Count / k)] = true;
			}

			// Loop until converged
			int changes = -1;
			int iter = 0;
			float cost;
			while (changes != 0 && iter < 10) {
				clock.Start ();
				changes = 0;
				cost = 0;

				foreach (Coord tile in region.turf) {
					float distanceToMedoid = float.MaxValue;

					for (int i = 0; i < k; i++) {
						float currDistToMedoid = tile.ManhattanDist (medoids[i]);
						if (currDistToMedoid < distanceToMedoid) {
							distanceToMedoid = currDistToMedoid;
							points[tile.x, tile.y].areaValue = regionId + i;
						}
					}

					cost += distanceToMedoid;
				}

				for (int medoidIndex = 0; medoidIndex < k; medoidIndex++) {
					//float tt = clock.Elapsed ();
					float tSum = 0;
					Coord oldMedoid = medoids[medoidIndex];
					for (int i = 0; i < region.turf.Count; i++) {
						Coord tile = region.turf[i];

						if (tile != oldMedoid) {
							float newCost = 0;
							medoids[medoidIndex] = tile;

							float tCost = clock.Elapsed ();
							region.turf.Sum (til => til.ManhattanDist (medoids[points[til.x, til.y].areaValue - regionId]));
							/*foreach (Coord t in region.turf) {
								newCost += t.ManhattanDist (medoids[points[t.x, t.y].areaValue - regionId]);
							}*/
							tSum += clock.Elapsed () - tCost;

							if (newCost < cost) {
								cost = newCost;
								changes++;
							} else {
								medoids[medoidIndex] = oldMedoid;
							}
						}
					}
					
					//Debug.Log ("Medoid " + medoidIndex + " takes " + (clock.Elapsed () - tt) + " seconds. Sum cost calc time = " + tSum);
				}
				
				iter++;
				//Debug.Log ("Iteration run with " + clock.Elapsed () + " seconds.");
			}
			Debug.Log ("Iteration: " + iter);
			regionId += k;
		}

		return regionId;
	}

	public int ClusterLocationsKMedoidsVoronoi (MapPoint[,] points) {
		// K-medoids cluster algorithm to separate locations in the regions

		ExecutionTimer clock = new ExecutionTimer ();

		int regionId = 0;
		foreach (MapRegion region in regions) {
			int k = Mathf.RoundToInt (Mathf.Sqrt (region.turf.Count / 16.0f));
			k = Mathf.Max (1, k);
			Debug.Log (k + " medoid(s)");

			Coord[] medoids = new Coord[k];
			for (int i = 0; i < k; i++) {
				// Assign medoid to first k data points
				medoids[i] = region.turf[i * (region.turf.Count / k)];
			}

			List<List<Coord>> clustering = new List<List<Coord>> ();
			for (int i = 0; i < k; i++) {
				clustering.Add (new List<Coord> ());
			}

			// Loop until converged
			int changes = -1;
			int iter = 0;
			float cost;
			while (changes != 0) {
				clock.Start ();
				changes = 0;
				cost = 0;

				for (int i = 0; i < k; i++) {
					clustering[i].Clear ();
				}

				foreach (Coord tile in region.turf) {
					float distanceToMedoid = float.MaxValue;

					for (int i = 0; i < k; i++) {
						float currDistToMedoid = tile.ManhattanDist (medoids[i]);
						if (currDistToMedoid < distanceToMedoid) {
							distanceToMedoid = currDistToMedoid;
							points[tile.x, tile.y].areaValue = regionId + i;
						}
					}

					clustering[points[tile.x, tile.y].areaValue - regionId].Add (tile);
					cost += distanceToMedoid;
				}

				for (int i = 0; i < k; i++) {
					List<Coord> cluster = clustering[i];

					float minDist = cluster.Sum (til => til.ManhattanDist (medoids[i]));

					for (int t = 0; t < cluster.Count; t++) {
						float newDist = cluster.Sum (til => til.ManhattanDist (cluster[t]));
						if (newDist < minDist) {
							medoids[i] = cluster[t];
							minDist = newDist;
							changes++;
						}
					}
				}
				//Debug.Log ("Medoid " + medoidIndex + " takes " + (clock.Elapsed () - tt) + " seconds. Sum cost calc time = " + tSum);

				iter++;
				//Debug.Log ("Iteration run with " + clock.Elapsed () + " seconds.");
			}
			Debug.Log ("Iteration: " + iter);
			regionId += k;
		}

		return regionId;
	}

	// Helper func for DBSCAN
	List<Coord> RangeQueries (List<Coord> db, Coord t, float eps) {
        List<Coord> neighbours = new List<Coord> ();
        foreach (Coord p in db) {
            if (t != p && t.Dist (p) <= eps) {
                neighbours.Add (p);
            }
        }
        return neighbours;
    }
	
}

public enum CAMethod {
	KMeans,
	DBSCAN,
	KMedoidsPAM,
	KMedoidsVoronoi
}

public struct ClusterPoint {
    public int regionNumber;
    public bool done;
}
