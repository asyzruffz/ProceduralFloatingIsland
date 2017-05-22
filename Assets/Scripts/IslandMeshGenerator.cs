using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandMeshGenerator : MonoBehaviour {

    SquareGrid squareGrid;
    List<Vector3> vertices = new List<Vector3> ();
    List<int> triangles = new List<int> ();
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines;
    HashSet<int> checkedVertices = new HashSet<int> ();

    public List<Mesh> GenerateMesh (int[,] map, IsleInfo info, float squareSize, float depth) {
        
        squareGrid = new SquareGrid (map, info.id, squareSize, info.offset);
        vertices.Clear ();
        triangles.Clear ();
        triangleDictionary.Clear ();
        outlines = new List<List<int>> ();
        checkedVertices.Clear ();

        for (int x = 0; x < squareGrid.squares.GetLength (0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength (1); y++) {
                TriangulateSquare (squareGrid.squares[x, y]);
            }
        }

        List<Mesh> meshList = new List<Mesh> ();

        Mesh surfaceMesh = new Mesh ();
        surfaceMesh.vertices = vertices.ToArray ();
        surfaceMesh.triangles = triangles.ToArray ();
        surfaceMesh.RecalculateNormals ();


        Mesh undersideMesh = new Mesh ();
        triangles.Reverse ();
        undersideMesh.vertices = vertices.ToArray ();
        undersideMesh.triangles = triangles.ToArray ();
        undersideMesh.RecalculateNormals ();

        meshList.Add (surfaceMesh);
        meshList.Add (CreateWallMesh (depth));
        meshList.Add (undersideMesh);

        info.outlines = outlines;
        return meshList;
    }

    Mesh CreateWallMesh(float wallHeight) {
        List<Vector3> wallVertices = new List<Vector3> ();
        List<int> wallTriangles = new List<int> ();
        Mesh wallMesh = new Mesh ();

        CalculateMeshOutlines ();

        foreach (List<int> outline in outlines) {
            for (int i = 0; i < outline.Count - 1; i++) {
                int startIndex = wallVertices.Count;
                wallVertices.Add (vertices[outline[i]]);        // left
                wallVertices.Add (vertices[outline[i + 1]]);    // right
                wallVertices.Add (vertices[outline[i]] - Vector3.up * wallHeight);        // bottom left
                wallVertices.Add (vertices[outline[i + 1]] - Vector3.up * wallHeight);    // bottom right

                wallTriangles.Add (startIndex + 0);
                wallTriangles.Add (startIndex + 2);
                wallTriangles.Add (startIndex + 3);

                wallTriangles.Add (startIndex + 3);
                wallTriangles.Add (startIndex + 1);
                wallTriangles.Add (startIndex + 0);
            }
        }

        wallMesh.vertices = wallVertices.ToArray ();
        wallMesh.triangles = wallTriangles.ToArray ();
        wallMesh.RecalculateNormals ();

        return wallMesh;
    }

    void TriangulateSquare(Square square) {
        // Create triangle vertex position from points on the square for different configuration
        switch(square.configuration) {
            case 0:
                break;

            // 1 point
            case 1:
                MeshFromPoints (square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints (square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints (square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points
            case 3:
                MeshFromPoints (square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints (square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints (square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints (square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 points
            case 7:
                MeshFromPoints (square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints (square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints (square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints (square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 points
            case 15:
                MeshFromPoints (square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add (square.topLeft.vertexIndex);
                checkedVertices.Add (square.topRight.vertexIndex);
                checkedVertices.Add (square.bottomRight.vertexIndex);
                checkedVertices.Add (square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points) {
        AssignVertices (points);

        if(points.Length >= 3) {
            CreateTriangle (points[0], points[1], points[2]);
        }
        if (points.Length >= 4) {
            CreateTriangle (points[0], points[2], points[3]);
        }
        if (points.Length >= 5) {
            CreateTriangle (points[0], points[3], points[4]);
        }
        if (points.Length >= 6) {
            CreateTriangle (points[0], points[4], points[5]);
        }
    }

    void AssignVertices (Node[] points) {
        for(int i = 0; i < points.Length; i++) {
            if(points[i].vertexIndex == -1) {
                points[i].vertexIndex = vertices.Count;
                vertices.Add (points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) {
        triangles.Add (a.vertexIndex);
        triangles.Add (b.vertexIndex);
        triangles.Add (c.vertexIndex);

        Triangle triangle = new Triangle (a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary (triangle.vertices[0], triangle);
        AddTriangleToDictionary (triangle.vertices[1], triangle);
        AddTriangleToDictionary (triangle.vertices[2], triangle);
    }

    void AddTriangleToDictionary (int vertexIndexKey, Triangle triangle) {
        if(triangleDictionary.ContainsKey(vertexIndexKey)) {
            triangleDictionary[vertexIndexKey].Add (triangle);
        } else {
            List<Triangle> triangleList = new List<Triangle> ();
            triangleList.Add (triangle);
            triangleDictionary.Add (vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines() {
        for(int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++) {
            if(!checkedVertices.Contains(vertexIndex)) {
                int nextOutlineVertex = GetConnectedOutlineVertex (vertexIndex);
                if(nextOutlineVertex != -1) {
                    checkedVertices.Add (vertexIndex);

                    List<int> newOutline = new List<int> ();
                    newOutline.Add (vertexIndex);
                    outlines.Add (newOutline);
                    FollowOutline (nextOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add (vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex) {
        outlines[outlineIndex].Add (vertexIndex);
        checkedVertices.Add (vertexIndex);

        int nextOutlineVertex = GetConnectedOutlineVertex (vertexIndex);
        if (nextOutlineVertex != -1) {
            FollowOutline (nextOutlineVertex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex) {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++) {
            Triangle triangle = trianglesContainingVertex[i];

            for(int vert = 0; vert < 3; vert++) {
                int nextVertex = triangle.vertices[vert];
                if (vertexIndex != nextVertex && !checkedVertices.Contains(nextVertex)) {
                    if (IsOutlineEdge (vertexIndex, nextVertex)) {
                        return nextVertex;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB) {
        // Check whether an edge from vertexA and vertexB shared more than one triangle
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for(int i = 0; i < trianglesContainingVertexA.Count; i++) {
            if(trianglesContainingVertexA[i].Contains(vertexB)) {
                sharedTriangleCount++;
                if(sharedTriangleCount > 1) {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }
}
