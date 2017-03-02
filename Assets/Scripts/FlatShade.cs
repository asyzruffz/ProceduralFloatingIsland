using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class FlatShade {

	public static Mesh DuplicateSharedVertex (Mesh mesh) {
        List<Vector3> vertices = mesh.vertices.ToList();
        int[] triangles = mesh.triangles;
        bool[] checkedVertices = new bool[vertices.Count];

        for(int verIndex = 0; verIndex < triangles.Length; verIndex++) {
            if(checkedVertices[triangles[verIndex]]) {
                vertices.Add (vertices[triangles[verIndex]]);
                triangles[verIndex] = vertices.Count - 1;
            } else {
                checkedVertices[triangles[verIndex]] = true;
            }
        }

        mesh.vertices = vertices.ToArray ();
        mesh.triangles = triangles;
        mesh.RecalculateNormals ();
        return mesh;
    }
}
