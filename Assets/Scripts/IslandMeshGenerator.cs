using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandMeshGenerator : MonoBehaviour {

    SquareGrid squareGrid;
    List<Vector3> vertices = new List<Vector3> ();
    List<int> triangles = new List<int> ();
    IsleMeshDetail meshDetail;

    public List<Mesh> GenerateIslandMesh (MapRegion region, IsleInfo info, float squareSize, float depth) {
        
        squareGrid = new SquareGrid (region, squareSize);
        vertices.Clear ();
        triangles.Clear ();

        meshDetail = new IsleMeshDetail ();

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
        
        info.surfaceMeshDetail = meshDetail;
        return meshList;
    }

    public Mesh GenerateRegionMesh (MapRegion region, float squareSize) {

        squareGrid = new SquareGrid (region, squareSize);
        vertices.Clear ();
        triangles.Clear ();

        for (int x = 0; x < squareGrid.squares.GetLength (0); x++) {
            for (int y = 0; y < squareGrid.squares.GetLength (1); y++) {
                TriangulateSquare (squareGrid.squares[x, y]);
            }
        }

        Mesh regionMesh = new Mesh ();
        regionMesh.vertices = vertices.ToArray ();
        regionMesh.triangles = triangles.ToArray ();
        regionMesh.RecalculateNormals ();

        return regionMesh;
    }

    Mesh CreateWallMesh(float wallHeight) {
        meshDetail.Vertices = vertices;
        meshDetail.CalculateGradientMap ();

        List<Vector3> wallVertices = new List<Vector3> ();
        List<int> wallTriangles = new List<int> ();
        Mesh wallMesh = new Mesh ();
        
        foreach (List<int> outline in meshDetail.outlines) {
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
        meshDetail.AddTriangleToDictionary (triangle.vertices[0], triangle);
        meshDetail.AddTriangleToDictionary (triangle.vertices[1], triangle);
        meshDetail.AddTriangleToDictionary (triangle.vertices[2], triangle);
    }
}
