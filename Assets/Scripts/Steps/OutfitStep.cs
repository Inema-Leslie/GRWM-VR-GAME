using System.Collections.Generic;
using UnityEngine;

// Day 4: get dressed (top, bottom, shoes) - also used for the accessory step (head).
public class OutfitStep : RoutineStep
{
    public List<OutfitSocket> sockets = new List<OutfitSocket>();
    public string hintPrefix = "Put on: ";
    [Tooltip("Add the missing slots after the prefix (off for a plain sentence).")]
    public bool listSlots = true;

    public List<ClothingItem> Outfit
    {
        get
        {
            var list = new List<ClothingItem>();
            foreach (var s in sockets) if (s && s.Worn) list.Add(s.Worn);
            return list;
        }
    }

    public override float Progress => sockets.Count == 0 ? 0f : (float)Outfit.Count / sockets.Count;

    public override string Hint
    {
        get
        {
            var missing = new List<string>();
            foreach (var s in sockets) if (s && !s.Worn) missing.Add(s.slot.ToString().ToLower());
            return missing.Count == 0 ? "Looking cute!" : (listSlots ? hintPrefix + string.Join(", ", missing) : hintPrefix);
        }
    }

    protected override void OnBegin() { }

    void Update()
    {
        if (IsActive && sockets.Count > 0 && Outfit.Count == sockets.Count)
        {
            SparkleFX.Burst(sockets[0].transform.position);
            CompleteStep();
        }
    }
}
