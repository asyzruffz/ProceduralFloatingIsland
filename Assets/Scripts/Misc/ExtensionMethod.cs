using UnityEngine;

public static class ExtensionMethod {

	public static Vector2 ToXZ (this Vector3 v3) {
        return new Vector2 (v3.x, v3.z);
    }

}
