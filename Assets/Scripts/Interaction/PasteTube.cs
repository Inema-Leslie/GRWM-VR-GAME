using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Hold the toothpaste tube and touch it to the bristles: a blob of paste lands on the brush.
public class PasteTube : MonoBehaviour
{
    public XRSocketInteractor socket;   // the paste spot on the brush
    public XRGrabInteractable blob;     // the paste that goes into it
    public float reach = 0.08f;

    XRGrabInteractable tube;
    Renderer look;

    void Awake()
    {
        tube = GetComponent<XRGrabInteractable>();
        look = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        if (socket == null || blob == null || tube == null || !tube.isSelected) return;
        if (socket.hasSelection || blob.isSelected) return;

        Vector3 spot = socket.transform.position;
        Vector3 nearest = look ? look.bounds.ClosestPoint(spot) : transform.position;
        if (Vector3.Distance(nearest, spot) > reach) return;

        blob.transform.position = spot;
        foreach (var r in blob.GetComponentsInChildren<Renderer>()) r.enabled = true;
        socket.interactionManager.SelectEnter((IXRSelectInteractor)socket, (IXRSelectInteractable)blob);
        AudioFX.Play(AudioFX.Pop, spot, 0.6f);
    }
}
