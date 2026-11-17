using UnityEngine;

// The girl is you. She stands where you stand and faces where you look.
// Your own camera skips her layer, so you only see her in the mirrors.
[DefaultExecutionOrder(100)]
public class AvatarFollow : MonoBehaviour
{
    [Tooltip("How far behind the eyes her body sits, so the camera isn't inside her face.")]
    public float backOffset = 0.07f;

    Camera cam;
    float floorY;

    void Start()
    {
        floorY = transform.position.y;
        Hook();
    }

    void Hook()
    {
        cam = Camera.main;
        if (cam && gameObject.layer != 0) cam.cullingMask &= ~(1 << gameObject.layer);
    }

    void LateUpdate()
    {
        if (cam == null) { Hook(); if (cam == null) return; }
        var t = cam.transform;
        var f = t.forward; f.y = 0f;
        if (f.sqrMagnitude < 0.0001f) f = Vector3.ProjectOnPlane(t.up, Vector3.up);
        f.Normalize();
        var p = t.position - f * backOffset;
        transform.SetPositionAndRotation(new Vector3(p.x, floorY, p.z), Quaternion.LookRotation(f));
    }
}
