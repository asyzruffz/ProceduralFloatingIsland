using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Cluster {

    List<MapRegion> regions;
    
    public Cluster (List<MapRegion> regs) {
        regions = regs;
    }
	
	// Called by LandMap.GetZones (), returns number of subregions
	public int ClusterLocationsKMeans (MapPoint[,] points) {
		// K-means cluster algorithm to separate locations in the regions

		int regionId = 0;
		foreach (MapRegion region in regions) {
			int k = Mathf.RoundToInt (Mathf.Sqrt (region.turf.Count / 16.0f));
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

	// Helper func for K-Means
	/*float ObtainDistancePenalty (Coord a, Coord b, float penalty) {
        // Using line drawing algorithm to find obstacle along the line

        int h = b.y - a.y;
        int w = b.x - a.x;

        int incrIfFNegative = 2 * h;
        int incrIfFNotNegative = 2 * (h - w);

        int F = 2 * h - w;
        float totalPenalty = 0;

        int y = a.y;
        for (int x = a.x; x <= b.x; x++) {
            // Add penalty if there is a hole along the line
            if (!spots[x, y].filled) {
                totalPenalty += penalty;
            }

            if (F < 0)
                F += incrIfFNegative;
            else {
                y++;
                F += incrIfFNotNegative;
            }
        }

        if (totalPenalty > 0) Debug.Log ("Penalty: " + totalPenalty);
        return totalPenalty;
    }*/
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
