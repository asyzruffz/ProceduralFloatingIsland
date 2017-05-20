using System.Collections;
using System.Collections.Generic;

public class LandMap {

	public MapPoint[,] spots;

	int width;
	int length;
	int[,] regionMap;

	public LandMap (int width, int length) {
		this.width = width;
		this.length = length;

		spots = new MapPoint[width, length];
		regionMap = new int[width, length];
	}

	public void RandomFillMap (ref System.Random randomizer, float fillPercent) {
		// Fill the map randomly with 0s and 1s based on percentage fill
		
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < length; y++) {
				if (x == 0 || y == 0 || x == width - 1 || y == length - 1) {
					spots[x, y].fillValue = 0;
				} else {
					spots[x, y].fillValue = randomizer.Next (0, 100) < fillPercent ? 1 : 0;
				}
			}
		}
	}

	public void SmoothMap () {
		// Change the state in each cell within the cellular automaton based on its neighbour

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < length; y++) {
				int neighbourLandTile = GetSurroundingLandCount (x, y);

				if (neighbourLandTile > 4) {
					spots[x, y].fillValue = 1;
				} else if (neighbourLandTile < 4) {
					spots[x, y].fillValue = 0;
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
						landCount += spots[neighbourX, neighbourY].fillValue;
					}
				}
			}
		}

		return landCount;
	}

	public List<MapRegion> GetRegions (int tileType) {
		// Get all regions of a tile type

		List<MapRegion> regions = new List<MapRegion> ();
		int[,] mapFlags = new int[width, length]; // marked tiles that's already visited

		int regionId = 1;
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < length; y++) {
				if (mapFlags[x, y] == 0 && spots[x, y].fillValue == tileType) {
					MapRegion newRegion = new MapRegion(GetRegionTiles (x, y), width, length);
					newRegion.id = regionId;

					foreach (Coord tile in newRegion.turf) {
						mapFlags[tile.x, tile.y] = 1;
						regionMap[tile.x, tile.y] = regionId;
					}

					regions.Add (newRegion);
					regionId++;
				}
			}
		}

		return regions;
	}

	// Called by GetRegions ()
	List<Coord> GetRegionTiles (int startX, int startY) {
		// Flood fill algorithm to find the coord encompassing a region

		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[width, length]; // marked tiles that's already visited
		int tileType = spots[startX, startY].fillValue; // start tile determine the other tiles to be checked

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags[startX, startY] = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue ();
			tiles.Add (tile);

			for (int x = tile.x - 1; x <= tile.x + 1; x++) {
				for (int y = tile.y - 1; y <= tile.y + 1; y++) {

					if (IsInMapRange (x, y) && (x == tile.x || y == tile.y)) { // ignore diagonally
						if (mapFlags[x, y] == 0 && spots[x, y].fillValue == tileType) {
							mapFlags[x, y] = 1;
							queue.Enqueue (new Coord (x, y));
						}
					}
				}
			}
		}

		return tiles;
	}

	public int[,] GetRegionMap () {
		return regionMap;
	}

	public bool IsInMapRange (int x, int y) {
		return x >= 0 && y >= 0 && x < width && y < length;
	}

}

public struct MapPoint {
	public int fillValue;
}

public struct Coord {
	public int x, y;

	public Coord (int tileX, int tileY) {
		x = tileX;
		y = tileY;
	}
}
