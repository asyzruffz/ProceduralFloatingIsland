using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRegion {

    public List<List<int>> outlines = new List<List<int>> ();
    public Dictionary<int, float> gradientMap = new Dictionary<int, float> ();

    List<Vector3> vertices;
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();
    HashSet<int> checkedVertices = new HashSet<int> ();

    public List<Vector3> Vertices {
        get { return vertices; }
        set { vertices = new List<Vector3> (value); }
    }

    public void FillTriangleDictionary (int[] tris) {
        for (int i = 0; i < tris.Length; i += 3) {
            Triangle triangle = new Triangle (i, i + 1, i + 2);
            AddTriangleToDictionary (triangle.vertices[0], triangle);
            AddTriangleToDictionary (triangle.vertices[1], triangle);
            AddTriangleToDictionary (triangle.vertices[2], triangle);
        }

    }

    #region Outline functions
    public void CalculateMeshOutlines () {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if (!checkedVertices.Contains (vertexIndex)) {
                int nextOutlineVertex = GetConnectedOutlineVertex (vertexIndex);
                if (nextOutlineVertex != -1) {
                    CheckVertex(vertexIndex);

                    List<int> newOutline = new List<int> ();
                    newOutline.Add (vertexIndex);
                    outlines.Add (newOutline);
                    FollowOutline (nextOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add (vertexIndex);
                }
            }
        }
    }

    void FollowOutline (int vertexIndex, int outlineIndex) {
        outlines[outlineIndex].Add (vertexIndex);
        CheckVertex(vertexIndex);

        int nextOutlineVertex = GetConnectedOutlineVertex (vertexIndex);
        if (nextOutlineVertex != -1) {
            FollowOutline (nextOutlineVertex, outlineIndex);
        }
    }

    public void CheckVertex (int vertexIndex) { // made public so it can be called in IslandMeshGenerator.cs for optimization
        checkedVertices.Add (vertexIndex);
    }

    public void AddTriangleToDictionary (int vertexIndexKey, Triangle triangle) {
        if (triangleDictionary.ContainsKey (vertexIndexKey)) {
            triangleDictionary[vertexIndexKey].Add (triangle);
        } else {
            List<Triangle> triangleList = new List<Triangle> ();
            triangleList.Add (triangle);
            triangleDictionary.Add (vertexIndexKey, triangleList);
        }
    }

    int GetConnectedOutlineVertex (int vertexIndex) {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++) {
            Triangle triangle = trianglesContainingVertex[i];

            for (int vert = 0; vert < 3; vert++) {
                int nextVertex = triangle.vertices[vert];
                if (vertexIndex != nextVertex && !checkedVertices.Contains (nextVertex)) {
                    if (IsOutlineEdge (vertexIndex, nextVertex)) {
                        return nextVertex;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge (int vertexA, int vertexB) {
        // Check whether an edge from vertexA and vertexB shared more than one triangle
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++) {
            if (trianglesContainingVertexA[i].Contains (vertexB)) {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }
    #endregion

    public void CalculateGradientMap () {
        HashSet<int> checkedGradientVertices = new HashSet<int> ();

        // Start with the gradient being zero at the outlines
        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count; i++) {
                if (!checkedGradientVertices.Contains (outline[i])) {
                    gradientMap.Add (outline[i], 0);
                    checkedGradientVertices.Add (outline[i]);
                }
            }
        }

		// Loop each gradient gradient ring until all vertices are done
		int gradient;
		int mappingProgress = 0;
		for (gradient = 1; checkedGradientVertices.Count < vertices.Count && mappingProgress != gradientMap.Count; gradient++) {
            mappingProgress = gradientMap.Count;

            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
                // Iterate all vertices that has not been checked
                if (!checkedGradientVertices.Contains (vertexIndex)) {

					if (IsRingEdge (vertexIndex, gradient)) {
						checkedGradientVertices.Add (vertexIndex);
						gradientMap.Add (vertexIndex, gradient);

						// Check if there is a connected vertex forming outer ring
						int nextRingVertex = GetConnectedRingVertex (vertexIndex, ref checkedGradientVertices, gradient);
						if (nextRingVertex != -1) {
							// Follow the ring and fill the gradient
							FollowRing (nextRingVertex, gradient, ref checkedGradientVertices);
						}
					}
                }
            }
        }

		// Normalize the gradient to (0 - 1)
		for (int i = 0; i < gradientMap.Count; i++) {
			if (gradientMap.ContainsKey (i)) {
				gradientMap[i] = Mathf.InverseLerp (0, gradient, gradientMap[i]);
			}
		}
	}

    int GetConnectedRingVertex (int vertexIndex, ref HashSet<int> check, int ringNum) {
        // Get all triangles made up off this vertex
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        // Iterate all the vertices in said triangles
        for (int i = 0; i < trianglesContainingVertex.Count; i++) {
            Triangle triangle = trianglesContainingVertex[i];

            for (int vert = 0; vert < 3; vert++) {
                int nextVertex = triangle.vertices[vert];

                // Exclude this vertex and vertices that has been checked
                if (vertexIndex != nextVertex && !check.Contains (nextVertex)) {
                    if (IsRingEdge (nextVertex, ringNum)) {
                        return nextVertex;
                    }
                }
            }
        }
        
        return -1;
    }

    void FollowRing (int vertexIndex, int gradientValue, ref HashSet<int> check) {
        // Recursively fill the grafient value along the ring

        if (!check.Contains (vertexIndex)) {
            gradientMap.Add (vertexIndex, gradientValue);
            check.Add (vertexIndex);
        }

        int nextOutlineVertex = GetConnectedRingVertex (vertexIndex, ref check, gradientValue);
        if (nextOutlineVertex != -1) {
            FollowRing (nextOutlineVertex, gradientValue, ref check);
        }
    }

    bool IsRingEdge (int vertex, int ringNum) {
        bool isOnTheRing = false;

        // Check vertices connected to param vertex ...
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertex];
        for (int i = 0; !isOnTheRing && i < trianglesContainingVertex.Count; i++) {
            for (int vert = 0; !isOnTheRing && vert < 3; vert++) {
				// ... whether already gradient-mapped ...
				if (gradientMap.ContainsKey(trianglesContainingVertex[i].vertices[vert])) {
					// ... and on the previous ring
					isOnTheRing = gradientMap[trianglesContainingVertex[i].vertices[vert]] == (ringNum - 1);
                }
            }
        }
		
        return isOnTheRing;
    }

    // Get the rectangle surround the vertices region
    public Rect GetRectContainingVertices () {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < vertices.Count; i++) {
            if (vertices[i].x > maxX) {
                maxX = vertices[i].x;
            } else if (vertices[i].x < minX) {
                minX = vertices[i].x;
            }

            if (vertices[i].z > maxY) {
                maxY = vertices[i].z;
            } else if (vertices[i].z < minY) {
                minY = vertices[i].z;
            }
        }

        return new Rect (minX, minY, maxX - minX, maxY - minY);
    }
}
