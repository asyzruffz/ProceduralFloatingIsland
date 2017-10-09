using UnityEngine;

public struct Coord {
    public int x, y;

    public Coord (int tileX, int tileY) {
        x = tileX;
        y = tileY;
    }

    public Vector2 ToVector2 () {
        return new Vector2 (x, y);
    }

    public float Dist (Coord other) {
        return Mathf.Sqrt (Mathf.Pow (other.x - x, 2) + Mathf.Pow (other.y - y, 2));
    }

    public float ManhattanDist (Coord other) {
        return (Mathf.Abs (other.x - x) + Mathf.Abs (other.y - y));
    }

    public static bool operator ==(Coord a, Coord b) {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals (a, b)) {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null)) {
            return false;
        }

        // Return true if the fields match:
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Coord a, Coord b) {
        return !(a == b);
    }

    public override bool Equals(object other) {
        // If parameter is null return false:
        if (other == null) {
            return false;
        }

        // Return true if the fields match:
        return other is Coord && this == (Coord)other;
    }

    public override int GetHashCode () {
        return x ^ y;
    }
}
