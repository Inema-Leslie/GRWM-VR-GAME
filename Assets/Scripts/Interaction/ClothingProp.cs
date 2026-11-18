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
        var grab = GetComponent<XRGrabInteractable>();
        if (grab) grab.selectExited.AddListener(a =>
        {
            if (!(a.interactorObject is XRSocketInteractor)) LastReleased = Time.time;
        });
    }

    public void MarkReleased() => LastReleased = Time.time;   // desktop hands don't select, so they call this

    public void ReturnHome()
    {
        transform.SetPositionAndRotation(homePos, homeRot);
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
    }
}
