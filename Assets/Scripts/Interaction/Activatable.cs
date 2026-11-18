using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Anything you "use" while holding it: the pump bottle, the phone's shutter.
// VR: trigger button while grabbed (XRI Activate). Desktop: E while holding it.
public interface IActivatable
{
    void Activate();
}

public class Activatable : MonoBehaviour, IActivatable
{
    public event Action Activated;
    XRBaseInteractable xr;

    void OnEnable()
    {
        xr = GetComponent<XRBaseInteractable>();
        if (xr) xr.activated.AddListener(OnActivate);
    }

    void OnDisable()
    {
        if (xr) xr.activated.RemoveListener(OnActivate);
    }

    void OnActivate(ActivateEventArgs args) => Activate();

    public void Activate() => Activated?.Invoke();
}
