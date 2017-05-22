using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle {
	public int[] vertices;

	public Triangle (int a, int b, int c) {
		vertices = new int[3];
		vertices[0] = a;
		vertices[1] = b;
		vertices[2] = c;
	}

	public bool Contains (int vertexIndex) {
		return vertexIndex == vertices[0] || vertexIndex == vertices[1] || vertexIndex == vertices[2];
	}
}
