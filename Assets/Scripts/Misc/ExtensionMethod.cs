using UnityEngine;

public static class ExtensionMethod {

	public static Vector2 ToXZ (this Vector3 v3) {
        return new Vector2 (v3.x, v3.z);
    }

    public static float ManhattanDist (this Vector3 v3, Vector3 other) {
        return (Mathf.Abs (other.x - v3.x) + Mathf.Abs (other.y - v3.y) + Mathf.Abs (other.z - v3.z));
    }

	public static float ManhattanDist (this Vector2 v2, Vector2 other) {
		return (Mathf.Abs (other.x - v2.x) + Mathf.Abs (other.y - v2.y));
	}

}
