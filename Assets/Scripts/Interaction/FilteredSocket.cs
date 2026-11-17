using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Socket that only takes props with a matching PropTag (the toothpaste on the brush).
public class FilteredSocket : XRSocketInteractor
{
    public string acceptId = "paste";

    public override bool CanHover(IXRHoverInteractable i) => base.CanHover(i) && Fits(i);
    public override bool CanSelect(IXRSelectInteractable i) => base.CanSelect(i) && Fits(i);

    bool Fits(IXRInteractable i)
    {
        var t = i.transform.GetComponent<PropTag>();
        return t && t.id == acceptId;
    }
}
