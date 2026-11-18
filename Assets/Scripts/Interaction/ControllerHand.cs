using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// A hand that sits where the controller is. Squeeze grip and the middle, ring and little
// fingers curl; pull the trigger and the index finger curls; the thumb follows whichever is stronger.
[ExecuteAlways]
public class ControllerHand : MonoBehaviour
{
    public bool isLeft = true;
    [Tooltip("Local axis the finger joints bend around.")]
    public Vector3 curlAxis = Vector3.right;
    public float fingerCurl = 65f;   // degrees per joint at a full squeeze
    public float thumbCurl = 30f;
    [Range(0f, 1f)] public float previewGrip, previewTrigger;   // edit mode only, to check the pose

    struct Joint { public Transform t; public Quaternion rest; public int kind; }   // 0 grip finger, 1 index, 2 thumb
    readonly List<Joint> joints = new List<Joint>();
    InputAction grip, trigger;
    float g, tr;

    void OnEnable()
    {
        joints.Clear();
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            bool bends = n.EndsWith("Proximal") || n.EndsWith("Intermediate") || n.EndsWith("Distal");
            if (!bends) continue;
            int kind = n.Contains("Thumb") ? 2 : n.Contains("Index") ? 1 : 0;
            joints.Add(new Joint { t = t, rest = t.localRotation, kind = kind });
        }

        if (!Application.isPlaying) return;
        string hand = isLeft ? "{LeftHand}" : "{RightHand}";
        grip = new InputAction("Grip", InputActionType.Value);
        grip.AddBinding($"<XRController>{hand}/grip");
        grip.AddBinding($"<XRController>{hand}/{{Grip}}");
        trigger = new InputAction("Trigger", InputActionType.Value);
        trigger.AddBinding($"<XRController>{hand}/trigger");
        trigger.AddBinding($"<XRController>{hand}/{{Trigger}}");
        grip.Enable();
        trigger.Enable();
    }

    void OnDisable()
    {
        // back to the open pose, so edit mode never saves a curled hand
        foreach (var j in joints) if (j.t) j.t.localRotation = j.rest;
        grip?.Dispose(); trigger?.Dispose();
        grip = trigger = null;
    }

    void Update()
    {
        float wantG = previewGrip, wantT = previewTrigger;
        if (Application.isPlaying)
        {
            wantG = Read(grip);
            wantT = Read(trigger);
            float k = 1f - Mathf.Exp(-Time.deltaTime * 20f);   // quick but not jittery
            g = Mathf.Lerp(g, wantG, k);
            tr = Mathf.Lerp(tr, wantT, k);
        }
        else { g = wantG; tr = wantT; }

        foreach (var j in joints)
        {
            float amount = j.kind == 1 ? tr : j.kind == 2 ? Mathf.Max(g, tr) : g;
            float angle = amount * (j.kind == 2 ? thumbCurl : fingerCurl);
            j.t.localRotation = j.rest * Quaternion.AngleAxis(angle, curlAxis);
        }
    }

    static float Read(InputAction a)
    {
        if (a == null) return 0f;
        try { return Mathf.Clamp01(a.ReadValue<float>()); }
        catch { return a.IsPressed() ? 1f : 0f; }   // a button-only binding
    }
}
