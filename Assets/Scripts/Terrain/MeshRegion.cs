using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRegion {

    public List<List<int>> outlines = new List<List<int>> ();

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

    // Get the rectangle surround the vertices
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
