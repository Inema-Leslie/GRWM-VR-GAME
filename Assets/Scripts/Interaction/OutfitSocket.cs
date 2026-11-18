using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Socket on your body (chest, hips, feet). Only takes clothes for its slot; when filled it hides
// the hanger and shows the fitted mesh in the item's colour. Empty top/bottom = pyjamas.
public class OutfitSocket : XRSocketInteractor
{
    public ClothingSlot slot;
    public Renderer wornMesh;
    public Color pajamaColor = new Color(0.80f, 0.82f, 0.98f);
    [Tooltip("Accessories have no fitted mesh: the prop itself sits on your head, on the mirror-only layer.")]
    public bool keepPropVisible;
    [Tooltip("Tops fitted to her body in Blender; the one matching the worn item is shown.")]
    public System.Collections.Generic.List<Renderer> fitted = new System.Collections.Generic.List<Renderer>();

    public ClothingItem Worn { get; private set; }

    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock block;

    // every body socket, so a released piece of clothing can find where it goes
    public static readonly System.Collections.Generic.List<OutfitSocket> All = new System.Collections.Generic.List<OutfitSocket>();

    protected override void OnEnable() { base.OnEnable(); if (!All.Contains(this)) All.Add(this); }
    protected override void OnDisable() { All.Remove(this); base.OnDisable(); }

    protected override void Awake()
    {
        base.Awake();
        block = new MaterialPropertyBlock();
        ShowEmpty();
    }

    // pyjamas on top and bottom, bare feet
    void ShowEmpty()
    {
        if (!wornMesh) return;
        if (slot == ClothingSlot.Shoes) { wornMesh.enabled = false; return; }
        Tint(pajamaColor);
        wornMesh.enabled = true;
    }

    Renderer FittedFor(ClothingItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.fitName)) return null;
        foreach (var f in fitted) if (f && f.name == "Fit_" + item.fitName) return f;
        return null;
    }

    void Tint(Color c)
    {
        wornMesh.GetPropertyBlock(block);
        block.SetColor(BaseColor, c);
        wornMesh.SetPropertyBlock(block);
    }

    // only clothes you're holding (or just let go of) go on, so walking past the rail doesn't dress you
    public override bool CanHover(IXRHoverInteractable i) => base.CanHover(i) && Fits(i) && InHand(i);
    public override bool CanSelect(IXRSelectInteractable i) => base.CanSelect(i) && Fits(i) && (IsSelecting(i) || InHand(i));

    static bool InHand(IXRInteractable i)
    {
        if (i is IXRSelectInteractable s)
            foreach (var x in s.interactorsSelecting)
                if (!(x is XRSocketInteractor)) return true;
        var p = i.transform.GetComponent<ClothingProp>();
        return p && Time.time - p.LastReleased < 0.75f;
    }

    public bool Fits(IXRInteractable i)
    {
        var p = i.transform.GetComponent<ClothingProp>();
        return p && p.item && p.item.slot == slot;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        var p = args.interactableObject.transform.GetComponent<ClothingProp>();
        if (p == null) return;
        Worn = p.item;
        ShowProp(p, false, keepPropVisible);
        var fit = FittedFor(p.item);
        foreach (var f in fitted) if (f) f.enabled = f == fit;
        if (fit) { if (wornMesh) wornMesh.enabled = false; }   // the real top, fitted to her
        else if (wornMesh) { Tint(p.item.color); wornMesh.enabled = true; }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        var p = args.interactableObject.transform.GetComponent<ClothingProp>();
        if (p) ShowProp(p, true, keepPropVisible);
        Worn = null;
        foreach (var f in fitted) if (f) f.enabled = false;
        ShowEmpty();
    }

    // while worn the hanger rides along invisibly; its colliders turn into triggers so they
    // don't bump your body, but a VR hand can still grab it to take it off
    static void ShowProp(ClothingProp p, bool show, bool keepVisible)
    {
        foreach (var c in p.GetComponentsInChildren<Collider>()) c.isTrigger = !show;
        if (keepVisible)
        {
            // on your head: only mirrors see it (same layer as your body)
            int layer = show ? 0 : LayerMask.NameToLayer("Avatar");
            if (layer < 0) layer = 0;
            foreach (var t in p.GetComponentsInChildren<Transform>()) t.gameObject.layer = layer;
            return;
        }
        foreach (var r in p.GetComponentsInChildren<Renderer>()) r.enabled = show;
    }
}
