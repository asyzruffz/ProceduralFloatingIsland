using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMap {

    // The 2-dimensional array to store the map data as MapPoint struct
	public MapPoint[,] spots;

	int width;
	int length;
	int regionId = 1;

	public LandMap (int width, int length) {
		this.width = width;
		this.length = length;

		spots = new MapPoint[width, length];
	}

	public void RandomFillMap (ref System.Random randomizer, float fillPercent) {
		// Fill the map randomly with 0s and 1s based on percentage fill
		
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < length; y++) {
				if (x == 0 || y == 0 || x == width - 1 || y == length - 1) {
					spots[x, y].filled = false;
				} else {
					spots[x, y].filled = (randomizer.Next (0, 100) < fillPercent);
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
					newRegion.id = regionId;

					foreach (Coord tile in newRegion.turf) {
						mapFlags[tile.x, tile.y] = true;
						spots[tile.x, tile.y].areaValue = regionId;
					}

					regions.Add (newRegion);
					regionId++;
				}
			}
		}

		ClusterLocationsInRegions (ref regions);
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

	// Called by GetRegions ()
	void ClusterLocationsInRegions (ref List<MapRegion> regions) {
		// K-means cluster algorithm to separate locations in the regions
		int k = 3;

		foreach (MapRegion region in regions) {
			Vector2[] centroids = new Vector2[k];
			for (int i = 0; i < k; i++) {
				// Assign centroid to first three data points
				centroids[i] = region.turf[i].ToVector2 ();
			}

			// Loop until converged
			int changes = -1;
			while (changes != 0) {
				changes = 0;

				foreach (Coord tile in region.turf) {
					Vector2 tilePos = tile.ToVector2 ();

					int initialArea = spots[tile.x, tile.y].areaValue;
					float distanceToCentroid = float.MaxValue;

					for (int i = 0; i < k; i++) {
						float currDistToCentroid = Vector2.SqrMagnitude (centroids[i] - tilePos);
						if (currDistToCentroid < distanceToCentroid) {
							distanceToCentroid = currDistToCentroid;
							spots[tile.x, tile.y].areaValue = i;
						}
					}

					if (initialArea != spots[tile.x, tile.y].areaValue) {
						changes++;
					}
				}

				Vector2[] cumulativeCentroids = new Vector2[k];
				int[] frequency = new int[k];
				foreach (Coord tile in region.turf) {
					cumulativeCentroids[spots[tile.x, tile.y].areaValue] += tile.ToVector2 ();
					frequency[spots[tile.x, tile.y].areaValue]++;
				}

				for (int i = 0; i < k; i++) {
					centroids[i] = cumulativeCentroids[i] / frequency[i];
				}
			}
		}
	}
	
	public bool IsInMapRange (int x, int y) {
		return x >= 0 && y >= 0 && x < width && y < length;
	}

}

public struct MapPoint {
	public bool filled;
	public int areaValue;
}

public struct Coord {
	public int x, y;

	public Coord (int tileX, int tileY) {
		x = tileX;
		y = tileY;
	}

	public Vector2 ToVector2 () {
		return new Vector2 (x, y);
	}
}
