using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cluster {

    List<MapRegion> regions;
    
    public Cluster (List<MapRegion> regs) {
        regions = regs;
    }

    public int ClusterLocationsInRegions (float epsilon, int minPts, MapPoint[,] points) {
        // DBSCAN cluster algorithm to separate locations in the regions

        Dictionary<Coord, ClusterPoint> clusteredPts = new Dictionary<Coord, ClusterPoint> ();
        foreach (MapRegion region in regions) {
            foreach (Coord tile in region.turf) {
                if (!clusteredPts.ContainsKey (tile)) {
                    clusteredPts.Add (tile, new ClusterPoint ());
                } else {
                    Debug.Log ("DBSCAN: Duplicate of tile found! (" + tile.x + "," + tile.y + ")");
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
                    Debug.Log ("Outlier");
                    continue;
                } else {
                    Debug.Log ("Zone " + regionId);
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

public struct ClusterPoint {
    public int regionNumber;
    public bool done;
}
