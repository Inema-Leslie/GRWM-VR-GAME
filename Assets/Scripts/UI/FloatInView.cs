using UnityEngine;

// Keeps a panel in front of you without glueing it to your face: it only drifts back
// when you look well away from it, and never ends up inside a wall.
public class FloatInView : MonoBehaviour
{
    public float distance = 1.3f;
    public float below = 0.08f;          // a touch under eye level reads comfortably
    public float lazyAngle = 40f;

    Transform cam;
    Vector3 target;
    bool placed;

    void Start() => Snap();

    public void Snap()
    {
        cam = Camera.main ? Camera.main.transform : null;
        if (cam == null) return;
        target = Spot();
        transform.position = target;
        Face();
        placed = true;
    }

    Vector3 Spot()
    {
        var f = cam.forward; f.y = 0f;
        if (f.sqrMagnitude < 0.01f) f = cam.up;
        f.Normalize();
        float d = MiraHints.ClearDistance(cam.position, f, distance);
        var p = cam.position + f * d;
        p.y = cam.position.y - below;
        return p;
    }

    void Face()
    {
        var away = transform.position - cam.position; away.y = 0f;
        if (away.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(away);
    }

    void LateUpdate()
    {
        if (!placed) { Snap(); return; }
        if (cam == null) return;
        var to = transform.position - cam.position; to.y = 0f;
        var f = cam.forward; f.y = 0f;
        // in VR the head only gets tracked a moment after start; also re-center if you walked far off
        float dy = Mathf.Abs(transform.position.y - (cam.position.y - below));
        if (Vector3.Angle(to, f) > lazyAngle || to.magnitude > distance * 2.2f || to.magnitude < 0.35f || dy > 0.25f) target = Spot();
        float k = UIPrefs.GentleMotion ? 2f : 5f;
        transform.position = Vector3.Lerp(transform.position, target, 1f - Mathf.Exp(-Time.deltaTime * k));
        Face();
    }
}
