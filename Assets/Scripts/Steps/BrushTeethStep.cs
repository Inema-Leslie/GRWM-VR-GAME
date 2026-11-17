using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Day 2: toothpaste goes in the socket on the bristles, then scrub near your mouth.
// Scrubbing faster fills the sparkle bar faster.
public class BrushTeethStep : RoutineStep
{
    public XRGrabInteractable brush;
    public Transform brushHead;
    public XRSocketInteractor pasteSocket;
    public float brushSeconds = 8f;
    public float mouthRadius = 0.22f;

    float amount, buzz;
    Vector3 lastHead;

    bool HasPaste => pasteSocket && pasteSocket.hasSelection;

    public override float Progress => IsComplete ? 1f : amount;

    public override string Hint => HasPaste
        ? "Brush near your mouth! (desktop: hold Space)"
        : "Pick up the toothpaste and put it on the brush";

    protected override void OnBegin()
    {
        amount = 0f;
        lastHead = brushHead.position;
    }

    void Update()
    {
        if (!IsActive) return;

        Vector3 head = brushHead.position;
        float speed = (head - lastHead).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastHead = head;

        if (!HasPaste || !MouthZone.IsNear(head, mouthRadius)) return;

        float rate = 0.4f + 1.6f * Mathf.Clamp01(speed / 0.6f);
        amount += rate * Time.deltaTime / brushSeconds;

        buzz -= Time.deltaTime;
        if (buzz <= 0f) { buzz = 0.1f; Buzz(0.25f, 0.05f); }

        if (amount < 1f) return;
        amount = 1f;
        Buzz(0.8f, 0.25f);
        SparkleFX.Burst(head);
        // paste is used up
        var paste = pasteSocket.firstInteractableSelected;
        if (paste != null)
            foreach (var r in paste.transform.GetComponentsInChildren<Renderer>()) r.enabled = false;
        CompleteStep();
    }

    void Buzz(float strength, float seconds)
    {
        foreach (var i in brush.interactorsSelecting)
            if (i is XRBaseInputInteractor hand) hand.SendHapticImpulse(strength, seconds);
    }
}
