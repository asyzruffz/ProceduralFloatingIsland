﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareGrid {
	public Square[,] squares;

	public SquareGrid (MapRegion region, float squareSize, Vector3 gridCentre) {
		int nodeCountX = region.GetRangeInfo (Ranges.FullX) + 2; // +2 for padding empty node each side
		int nodeCountY = region.GetRangeInfo (Ranges.FullY) + 2;
        float mapWidth = nodeCountX * squareSize;
		float mapHeight = nodeCountY * squareSize;

		ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
		for (int x = 0; x < nodeCountX; x++) {
			for (int y = 0; y < nodeCountY; y++) {
				Vector3 pos = new Vector3 ((x * squareSize) - (nodeCountX - 1) / 2.0f,
											0,
										   (y * squareSize) - (nodeCountY - 1) / 2.0f);
				controlNodes[x, y] = new ControlNode (pos, squareSize);
			}
		}


        // Activate all nodes in region
        int minX = region.GetRangeInfo (Ranges.MinX) - 1; // -1 to ignore paddings
        int minY = region.GetRangeInfo (Ranges.MinY) - 1;
        foreach (Coord coord in region.turf) {
            controlNodes[coord.x - minX, coord.y - minY].active = true;
        }

        squares = new Square[nodeCountX - 1, nodeCountY - 1];
		for (int x = 0; x < nodeCountX - 1; x++) {
			for (int y = 0; y < nodeCountY - 1; y++) {
				squares[x, y] = new Square (controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
			}
		}
	}
}
