using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapRegion {
    
	public List<Coord> turf;

	int horizontalMaxRange;
	int verticalMaxRange;

	public MapRegion (List<Coord> coords, int xRange, int yRange) {
		turf = coords;
		horizontalMaxRange = xRange;
		verticalMaxRange = yRange;
	}

	public Vector3 GetCentre () {
		if (!turf.Any ()) {
			return Vector3.zero;
		}

		float minX = turf.Min (coord => coord.x);
		float minY = turf.Min (coord => coord.y);
		float maxX = turf.Max (coord => coord.x);
		float maxY = turf.Max (coord => coord.y);

		return new Vector3 ((minX + maxX - horizontalMaxRange) / 2, 0, (minY + maxY - verticalMaxRange) / 2);
	}
	
	public int GetRangeInfo (Ranges rangeType) {
        if (!turf.Any ()) {
            throw new System.Exception ("Region is empty!");
        }

        int minX = turf.Min (coord => coord.x);
		int maxX = turf.Max (coord => coord.x);
		int minY = turf.Min (coord => coord.y);
		int maxY = turf.Max (coord => coord.y);

		switch (rangeType) {
			default:
			case Ranges.MinX:
				return minX;
			case Ranges.MinY:
				return minY;
			case Ranges.MaxX:
				return maxX;
			case Ranges.MaxY:
				return maxY;
			case Ranges.FullX:
				return maxX - minX + 1;
			case Ranges.FullY:
				return maxY - minY + 1;
		}
	}
}

public enum Ranges {
	MinX,
	MinY,
	MaxX,
	MaxY,
	FullX,
	FullY
}