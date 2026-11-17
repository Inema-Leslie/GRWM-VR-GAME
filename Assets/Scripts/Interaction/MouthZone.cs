using UnityEngine;

// Where the player's mouth is: a little below and in front of the headset camera.
public static class MouthZone
{
    static Camera cam;

    public static Vector3 Point
    {
        get
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return Vector3.one * 9999f;
            var t = cam.transform;
            return t.position - t.up * 0.09f + t.forward * 0.08f;
        }
    }

    public static bool IsNear(Vector3 p, float radius) => (Point - p).sqrMagnitude < radius * radius;
}
