using System.Collections.Generic;
using UnityEngine;

public class LandMap {

    // The 2-dimensional array to store the map data as MapPoint struct
	public MapPoint[,] spots;

	int width;
	int length;

	public LandMap (int width, int length) {
		this.width = width;
		this.length = length;

		spots = new MapPoint[width, length];
	}

	public void RandomFillMap (float fillPercent) {
		// Fill the map randomly with 0s and 1s based on percentage fill
		
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < length; y++) {
				if (x == 0 || y == 0 || x == width - 1 || y == length - 1) {
					spots[x, y].filled = false;
				} else {
					spots[x, y].filled = (Random.Range (0, 100) < fillPercent);
				}
			}
		}
	}

	public void makeBaseShape (Texture2D shapeTexture) {
		shapeTexture.Resize (width, length);

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < length; y++) {
				Color colour = shapeTexture.GetPixel (x, y);
				bool blank = (colour.grayscale < 0.1f);

				if (blank) {
					spots[x, y].filled = false;
				}
			}
		}
	}

	public void SmoothMap (int passes) {
        // Change the state in each cell within the cellular automaton based on its neighbour

        for (int i = 0; i < passes; i++) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < length; y++) {
                    int neighbourLandTile = GetSurroundingLandCount (x, y);

                    if (neighbourLandTile > 4) {
                        spots[x, y].filled = true;
                    } else if (neighbourLandTile < 4) {
                        spots[x, y].filled = false;
                    }
                }
            }
        }
	}

	// Called by SmoothMap ()
	int GetSurroundingLandCount (int gridX, int gridY) {
		// Count the adjacent cell which is a land
		int landCount = 0;

		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {

				if (IsInMapRange (neighbourX, neighbourY)) {
					if (neighbourX != gridX || neighbourY != gridY) { // not checking the middle
						landCount += spots[neighbourX, neighbourY].filled ? 1 : 0;
					}
				}
			}
		}

		return landCount;
	}

	public List<MapRegion> GetRegions () {
		// Get all regions of a tile type

		List<MapRegion> regions = new List<MapRegion> ();
		bool[,] mapFlags = new bool[width, length]; // marked tiles that's already visited

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < length; y++) {
				if (!mapFlags[x, y] && spots[x, y].filled) {
					MapRegion newRegion = new MapRegion(GetRegionTiles (x, y), width, length);

					foreach (Coord tile in newRegion.turf) {
						mapFlags[tile.x, tile.y] = true;
					}

					regions.Add (newRegion);
				}
			}
		}

		return regions;
	}

	// Called by GetRegions ()
	List<Coord> GetRegionTiles (int startX, int startY) {
		// Flood fill algorithm to find the coord encompassing a region

		List<Coord> tiles = new List<Coord> ();
		bool[,] mapFlags = new bool[width, length]; // marked tiles that's already visited
		bool tileType = spots[startX, startY].filled; // start tile determine the other tiles to be checked

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags[startX, startY] = true;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue ();
			tiles.Add (tile);

			for (int x = tile.x - 1; x <= tile.x + 1; x++) {
				for (int y = tile.y - 1; y <= tile.y + 1; y++) {

					if (IsInMapRange (x, y) && (x == tile.x || y == tile.y)) { // ignore diagonally
						if (!mapFlags[x, y] && spots[x, y].filled == tileType) {
							mapFlags[x, y] = true;
							queue.Enqueue (new Coord (x, y));
						}
					}
				}
			}
		}

		return tiles;
	}

    public List<MapRegion> GetZones (List<MapRegion> regions, CAMethod clusteringAlgo) {
		int zoneNum;
		Cluster cl = new Cluster (regions);
		switch (clusteringAlgo) {
			default:
			case CAMethod.KMeans:
				LoggerTool.Post ("Using K-Means.");
				zoneNum = cl.ClusterLocationsKMeans (spots);
				break;
			case CAMethod.DBSCAN:
				LoggerTool.Post ("Using DBSCAN.");
				zoneNum = cl.ClusterLocationsDBSCAN (6, 20, spots);
				break;
			case CAMethod.KMedoidsPAM:
				LoggerTool.Post ("Using K-Medoids with PAM.");
				zoneNum = cl.ClusterLocationsKMedoidsPAM (spots);
				break;
			case CAMethod.KMedoidsVoronoi:
				LoggerTool.Post ("Using K-Medoids with voronoi iteration.");
				zoneNum = cl.ClusterLocationsKMedoidsVoronoi (spots);
				break;
		}


        // Initialize list to prepare zones
        List<List<Coord>> zoneTiles = new List<List<Coord>> ();
        for (int i = 0; i <= zoneNum; i++) {
            zoneTiles.Add (new List<Coord> ());
        }

		LoggerTool.Post ("ZoneNum: " + zoneNum);

        // Fill the lists
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < length; y++) {
                if (spots[x, y].filled) {
                    spots[x, y].areaValue = (spots[x, y].areaValue != -1) ? spots[x, y].areaValue : (zoneNum - 1);
                    zoneTiles[spots[x, y].areaValue].Add (new Coord (x, y));
                }
            }
        }

        List<MapRegion> zones = new List<MapRegion> ();
        for (int i = 0; i < zoneNum; i++) {
            zones.Add (new MapRegion (zoneTiles[i], width, length));
        }

        return zones;
    }
	
    public bool IsInMapRange (int x, int y) {
		return x >= 0 && y >= 0 && x < width && y < length;
	}

}

public struct MapPoint {
	public bool filled;
	public int areaValue;
}
