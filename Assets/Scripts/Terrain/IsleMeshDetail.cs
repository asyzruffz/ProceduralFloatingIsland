using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IsleMeshDetail {

    public List<List<int>> outlines = new List<List<int>> ();
    public Dictionary<int, float> gradientMap = new Dictionary<int, float> ();
    public int localPeak { get { return peak; } }

    List<Vector3> vertices;
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>> ();
    int peak;

    public List<Vector3> Vertices {
        get { return vertices; }
        set { vertices = new List<Vector3> (value); }
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
    
    public void CalculateGradientMap () {
        HashSet<int> checkedGradientVertices = new HashSet<int> ();
        
        // Loop each gradient gradient ring until all vertices are done
        int gradient;
		int mappingProgress = -1;
		for (gradient = 0; checkedGradientVertices.Count < vertices.Count && mappingProgress != gradientMap.Count; gradient++) {
            mappingProgress = gradientMap.Count;
            
            for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
                // Iterate all vertices that has not been checked
                if (!checkedGradientVertices.Contains (vertexIndex)) {
                    
                    if (gradient == 0) {
                        // outermost ring, also an outline
                        
                        int nextOutlineVertex = GetConnectedRingVertex (vertexIndex, ref checkedGradientVertices, gradient);
                        if (nextOutlineVertex != -1) {
                            checkedGradientVertices.Add (vertexIndex);
                            gradientMap.Add (vertexIndex, gradient);
                            
                            List<int> newOutline = new List<int> ();
                            newOutline.Add (vertexIndex);
                            outlines.Add (newOutline);
                            FollowRing (nextOutlineVertex, gradient, ref checkedGradientVertices);
                            outlines.Last ().Add (vertexIndex);
                        }

                    } else {
                        // inner rings

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
        }

        peak = gradient;
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
                    if (ringNum == 0) {
                        if (IsOutlineEdge (vertexIndex, nextVertex)) {
                            return nextVertex;
                        }
                    } else {
                        if (IsRingEdge (nextVertex, ringNum)) {
                            return nextVertex;
                        }
                    }
                }
            }
        }
        
        return -1;
    }

    void FollowRing (int vertexIndex, int gradientValue, ref HashSet<int> check) {
        // Recursively fill the gradient value along the ring

        // Fill in the outlines list
        if (gradientValue == 0) {
            outlines.Last ().Add (vertexIndex);
        }

        if (!check.Contains (vertexIndex)) {
            gradientMap.Add (vertexIndex, gradientValue);
            check.Add (vertexIndex);
        }

        int nextOutlineVertex = GetConnectedRingVertex (vertexIndex, ref check, gradientValue);
        if (nextOutlineVertex != -1) {
            FollowRing (nextOutlineVertex, gradientValue, ref check);
        }
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

	public void NormalizeGradientMap (float maxHeight) {
		// Normalize the gradient to (0 - 1)
		for (int i = 0; i < gradientMap.Count; i++) {
			if (gradientMap.ContainsKey (i)) {
				gradientMap[i] = Mathf.InverseLerp (0, maxHeight, gradientMap[i]);
			}
		}
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
