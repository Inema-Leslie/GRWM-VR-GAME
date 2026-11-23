using UnityEngine;

// Puts a panel in front of you and leaves it there, like a real sign in the room.
// It never glides after your head (that reads as the world drifting and makes people sick).
// Only if you turn right away from it, or walk off, does it pop back in front of you.
public class FloatInView : MonoBehaviour
{
    public float distance = 1.3f;
    public float below = 0.08f;          // a touch under eye level reads comfortably
    public float lostAngle = 75f;        // how far you turn before it reappears in front of you
    public float settleSeconds = 1.5f;   // VR tracking takes a moment to start; keep re-placing until then

    Transform cam;
    float born;
    Vector3 snappedFrom;

    void OnEnable()
    {
        born = Time.time;
        Snap();
    }

    public void Snap()
    {
        if (cam == null) cam = Camera.main ? Camera.main.transform : null;
        if (cam == null) return;
        var f = cam.forward; f.y = 0f;
        if (f.sqrMagnitude < 0.01f) f = cam.up;
        f.Normalize();
        float d = MiraHints.ClearDistance(cam.position, f, distance);
        var p = cam.position + f * d;
        p.y = cam.position.y - below;
        transform.SetPositionAndRotation(p, Quaternion.LookRotation(f));
        snappedFrom = cam.position;
    }

    void LateUpdate()
    {
        if (cam == null) { Snap(); return; }
        // at start the headset's tracking can kick the camera to its real height a moment later
        if (Time.time - born < settleSeconds && (cam.position - snappedFrom).sqrMagnitude > 0.04f) { Snap(); return; }

        var to = transform.position - cam.position; to.y = 0f;
        var f = cam.forward; f.y = 0f;
        bool lost = Vector3.Angle(to, f) > lostAngle || to.magnitude > distance * 2.5f || to.magnitude < 0.3f;
        if (lost) Snap();   // a clean pop, no sliding
    }
}
