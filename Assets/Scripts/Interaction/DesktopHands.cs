using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Desktop stand-in for VR hands (added by DesktopPlayer).
//   Left click = press a button / pick up / put down (snaps into a socket if one is close)
//   Hold Space = bring what you're holding to your face (brushing, towel), or lean in with empty hands
//   E = use what you're holding (pump the bottle, take the selfie)
public class DesktopHands : MonoBehaviour
{
    public float reach = 3f;
    public float snapRadius = 1.0f;
    public Vector3 holdOffset = new Vector3(0.1f, -0.18f, 0.45f);
    public Vector3 faceOffset = new Vector3(0f, -0.09f, 0.14f);

    Camera cam;
    XRGrabInteractable held;

    public bool Holding => held != null;
    Rigidbody heldBody;
    bool wasKinematic;
    InteractionLayerMask heldLayers;
    Quaternion relRot;
    Vector3 localCenter;

    void Start() => cam = GetComponent<Camera>();

    void Update()
    {
        var mouse = Mouse.current;
        var keys = Keyboard.current;
        if (mouse == null || keys == null || cam == null) return;

        if (held) Carry(keys.spaceKey.isPressed);
        if (held && keys.eKey.wasPressedThisFrame)          // "use" what you hold (pump, phone shutter)
            foreach (var a in held.GetComponentsInChildren<IActivatable>()) a.Activate();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (held) Release();
            else Click(mouse.position.ReadValue());
        }
    }

    void Click(Vector2 screen)
    {
        if (UIKit.ClickPanel(screen, cam)) return;         // a menu button, not something behind it
        var ray = cam.ScreenPointToRay(screen);
        if (!FirstHit(ray, reach, out var hit)) return;

        var press = hit.collider.GetComponentInParent<IPressable>();
        if (press != null) { press.Press(); return; }

        var grab = hit.collider.GetComponentInParent<XRGrabInteractable>();
        if (grab) PickUp(grab);
    }

    void PickUp(XRGrabInteractable grab)
    {
        // take it out of a socket first (toothpaste on the brush, clothes on the mannequin)
        if (grab.isSelected && grab.interactionManager)
            foreach (var i in new List<IXRSelectInteractor>(grab.interactorsSelecting))
                grab.interactionManager.SelectExit(i, (IXRSelectInteractable)grab);

        held = grab;
        heldBody = grab.GetComponent<Rigidbody>();
        if (heldBody) { wasKinematic = heldBody.isKinematic; heldBody.isKinematic = true; }
        heldLayers = grab.interactionLayers;
        grab.interactionLayers = 0;              // sockets leave it alone while we carry it
        SetColliders(false);

        var upright = grab.GetComponent<HoldUpright>();
        relRot = upright ? Quaternion.Euler(upright.euler)
                         : Quaternion.Inverse(cam.transform.rotation) * grab.transform.rotation;
        localCenter = grab.transform.InverseTransformPoint(Center(grab.transform));
    }

    void Carry(bool toFace)
    {
        var t = held.transform;
        Vector3 target = cam.transform.TransformPoint(toFace ? faceOffset : holdOffset);
        if (toFace) target += cam.transform.right * Mathf.Sin(Time.time * 14f) * 0.03f;  // scrub scrub
        t.rotation = cam.transform.rotation * relRot;
        t.position += target - t.TransformPoint(localCenter);
    }

    void Release()
    {
        var grab = held;
        held = null;
        Vector3 c = Center(grab.transform);

        // nearest free socket that wants this item (and isn't on the item itself)
        XRSocketInteractor best = null;
        float bestD = snapRadius;
        grab.interactionLayers = heldLayers;
        var released = grab.GetComponent<ClothingProp>();
        if (released) released.MarkReleased();
        foreach (var s in FindObjectsByType<XRSocketInteractor>())
        {
            if (!s.isActiveAndEnabled || s.transform.IsChildOf(grab.transform)) continue;
            // body sockets take a new piece even when full: the old one goes back to the wardrobe
            bool ok = s is OutfitSocket body ? body.Fits(grab) : !s.hasSelection && s.CanSelect((IXRSelectInteractable)grab);
            if (!ok) continue;
            // flat distance, so shoes still reach the socket at her feet
            var a = s.GetAttachTransform(grab).position;
            float d = Vector2.Distance(new Vector2(a.x, a.z), new Vector2(c.x, c.z));
            if (d < bestD) { bestD = d; best = s; }
        }

        if (best == null) DropOnSurface(grab, c);
        SetColliders(true, grab);
        if (heldBody) heldBody.isKinematic = wasKinematic;

        if (best && grab.interactionManager)
        {
            if (best.hasSelection)
            {
                var old = best.firstInteractableSelected;
                grab.interactionManager.SelectExit((IXRSelectInteractor)best, old);
                var prop = old.transform.GetComponent<ClothingProp>();
                if (prop) prop.ReturnHome();
            }
            grab.transform.position = best.GetAttachTransform(grab).position;
            grab.interactionManager.SelectEnter((IXRSelectInteractor)best, (IXRSelectInteractable)grab);
        }
    }

    // put it down on whatever is underneath (counter, table, floor)
    void DropOnSurface(XRGrabInteractable grab, Vector3 c)
    {
        if (!FirstHit(new Ray(c, Vector3.down), 3f, out var hit)) return;
        var b = Bounds(grab.transform);
        grab.transform.position += Vector3.up * (hit.point.y - b.min.y + 0.002f);
    }

    // nearest hit that isn't the player's own body (looking down, the ray starts inside it)
    bool FirstHit(Ray ray, float dist, out RaycastHit best)
    {
        best = default;
        var me = GetComponentInParent<CharacterController>();
        bool found = false;
        foreach (var h in Physics.RaycastAll(ray, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            if (me && h.collider.transform.IsChildOf(me.transform)) continue;
            if (!found || h.distance < best.distance) { best = h; found = true; }
        }
        return found;
    }

    void SetColliders(bool on, XRGrabInteractable g = null)
    {
        g = g ? g : held;
        if (g == null) return;
        foreach (var col in g.colliders) if (col) col.enabled = on;
    }

    static Vector3 Center(Transform t) => Bounds(t).center;

    static Bounds Bounds(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }
}
