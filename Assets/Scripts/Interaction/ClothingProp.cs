using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// A piece of clothing you can pick up (on its hanger, or a pair of shoes).
// Its data lives in the ClothingItem asset. Remembers its spot so it can go back to the wardrobe.
public class ClothingProp : MonoBehaviour
{
    public ClothingItem item;

    public float LastReleased { get; private set; } = -10f;   // when a hand last let go of it

    Vector3 homePos;
    Quaternion homeRot;

    void Awake()
    {
        homePos = transform.position;
        homeRot = transform.rotation;
        grab = GetComponent<XRGrabInteractable>();
        if (grab) grab.selectExited.AddListener(a =>
        {
            if (a.interactorObject is XRSocketInteractor) return;
            LastReleased = Time.time;
            StartCoroutine(PutOnIfClose());
        });
    }

    XRGrabInteractable grab;

    // VR: let go of a piece of clothing against your body (or a hat near your head) and it goes on.
    // A trigger sphere alone is too fussy, because your hands are always in front of your chest.
    System.Collections.IEnumerator PutOnIfClose()
    {
        yield return null;   // let the hand finish letting go first
        var cam = Camera.main;
        if (item == null || cam == null || grab == null || grab.isSelected) yield break;

        Vector3 p = transform.position, eyes = cam.transform.position;
        bool close = item.slot == ClothingSlot.Head
            ? Vector3.Distance(p, eyes) < 0.45f
            : new Vector2(p.x - eyes.x, p.z - eyes.z).magnitude < 0.6f && p.y < eyes.y && p.y > eyes.y - 1.6f;
        if (!close) yield break;

        foreach (var socket in OutfitSocket.All)
        {
            if (socket == null || !socket.Fits(grab)) continue;
            var manager = grab.interactionManager;
            if (socket.hasSelection)   // swap: the old piece goes back where it came from
            {
                var old = socket.firstInteractableSelected;
                manager.SelectExit(socket, old);
                var oldProp = old.transform.GetComponent<ClothingProp>();
                if (oldProp) oldProp.ReturnHome();
            }
            manager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)grab);
            yield break;
        }
    }

    public void MarkReleased() => LastReleased = Time.time;   // desktop hands don't select, so they call this

    public void ReturnHome()
    {
        transform.SetPositionAndRotation(homePos, homeRot);
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
    }
}
