using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Small toolkit for the pastel UI. Positions are in Figma pixels from the top-left of the parent,
// so screens can copy numbers straight from the design.
public static class UIKit
{
    public static readonly Color Bg = Hex("FFF2F7"), Card = Hex("FFFBFD"), Border = Hex("EDB4CA"),
        Pink = Hex("CF5B88"), PinkEdge = Hex("B94C7A"), Plum = Hex("75435E"), Mauve = Hex("96647F"),
        Lilac = Hex("EEE4F7"), LilacEdge = Hex("DEC9EA"), Blush = Hex("FFE3ED"), Divider = Hex("F3DFE8"),
        PaleHeart = Hex("E9AEC6"), SoftLilac = Hex("D6B8E4"), Orchid = Hex("A66B9B"), Off = Hex("D8C8D2"),
        Shadow = new Color(185f / 255f, 76f / 255f, 122f / 255f, 0.22f);

    public enum Face { Title, Medium, Body, Caps }

    static UISkin Skin => UISkin.Get();

    // world-space panels currently showing (the desktop hands ignore clicks that land on them)
    static readonly List<RectTransform> panels = new List<RectTransform>();

    // desktop testing: press whatever button is under the mouse (controller rays go through XRUIInputModule)
    public static bool ClickPanel(Vector2 screen, Camera cam)
    {
        if (!PointerOverPanel(screen, cam)) return false;
        var es = EventSystem.current;
        if (es == null) return true;
        var ped = new PointerEventData(es) { position = screen, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        es.RaycastAll(ped, hits);
        foreach (var h in hits)
        {
            ped.pointerCurrentRaycast = h;
            if (ExecuteEvents.ExecuteHierarchy(h.gameObject, ped, ExecuteEvents.pointerClickHandler)) break;
        }
        return true;
    }

    public static bool PointerOverPanel(Vector2 screen, Camera cam)
    {
        foreach (var p in panels)
            if (p && p.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(p, screen, cam)) return true;
        return false;
    }

    public static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var c);
        return c;
    }

    // ---------- canvases ----------

    // A world-space canvas hung under a plain holder (a root world canvas snaps back to the origin).
    public static RectTransform WorldPanel(string name, Vector2 size, float metersPerPixel, out Transform holder, bool interactive = true)
    {
        EnsureEventSystem();
        holder = new GameObject(name).transform;
        holder.gameObject.layer = 5;                       // UI: mirrors don't reflect it
        var go = new GameObject("Canvas", typeof(RectTransform));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(holder, false);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        rt.sizeDelta = size;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one * metersPerPixel;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 3f;
        if (interactive)
        {
            go.AddComponent<GraphicRaycaster>();               // mouse (desktop testing)
            go.AddComponent<TrackedDeviceGraphicRaycaster>();  // VR controller rays
            panels.Add(rt);
        }
        return rt;
    }

    // one EventSystem driven by XRUIInputModule, so both controller rays and the mouse can press buttons
    public static void EnsureEventSystem()
    {
        var es = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
        if (es == null) es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        if (es.GetComponent<XRUIInputModule>()) return;
        foreach (var m in es.GetComponents<BaseInputModule>()) UnityEngine.Object.Destroy(m);
        var module = es.gameObject.AddComponent<XRUIInputModule>();
        module.enableMouseInput = false;                  // desktop clicks go through ClickPanel
    }

    // ---------- building blocks ----------

    public static RectTransform Node(string name, RectTransform parent, float x, float y, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, x, y, w, h);
        return rt;
    }

    public static void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, -y);
        rt.sizeDelta = new Vector2(w, h);
    }

    // rounded box; with a border it's two layers (edge colour behind, fill inset by the border width)
    public static Image Box(RectTransform parent, string name, float x, float y, float w, float h,
                            Color fill, float radius, Color? border = null, float borderWidth = 1f)
    {
        var root = Node(name, parent, x, y, w, h);
        if (border.HasValue)
        {
            Rounded(root.gameObject, border.Value, radius);
            var inner = Node("Fill", root, borderWidth, borderWidth, w - 2f * borderWidth, h - 2f * borderWidth);
            return Rounded(inner.gameObject, fill, Mathf.Max(0.5f, radius - borderWidth));
        }
        return Rounded(root.gameObject, fill, radius);
    }

    static Image Rounded(GameObject go, Color c, float radius)
    {
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        if (Skin.round && radius > 0f)
        {
            img.sprite = Skin.round;
            img.type = Image.Type.Sliced;
            float spriteRadius = Skin.round.border.x;      // corner size baked into the sprite
            img.pixelsPerUnitMultiplier = spriteRadius / radius;
        }
        return img;
    }

    public static TMP_Text Label(RectTransform parent, string text, Face face, float size, Color c,
                                 float x, float y, float w, float h, TextAlignmentOptions align = TextAlignmentOptions.TopLeft)
    {
        var rt = Node("Text", parent, x, y, w, h);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        var font = FontFor(face);
        if (font) t.font = font;
        else if (face == Face.Title || face == Face.Caps) t.fontStyle = FontStyles.Bold;   // no Fredoka/Nunito yet
        t.text = text;
        t.fontSize = size;
        t.color = c;
        t.alignment = align;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.overflowMode = TextOverflowModes.Overflow;
        t.raycastTarget = false;
        t.richText = true;
        return t;
    }

    static TMP_FontAsset FontFor(Face f) => f switch
    {
        Face.Title => Skin.title ? Skin.title : Skin.medium,
        Face.Medium => Skin.medium ? Skin.medium : Skin.title,
        Face.Body => Skin.body ? Skin.body : Skin.caps,
        _ => Skin.caps ? Skin.caps : Skin.body,
    };

    public static Image Icon(RectTransform parent, string icon, float x, float y, float size, Color c)
    {
        var rt = Node("icon_" + icon, parent, x, y, size, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = Skin.Icon(icon);
        img.color = c;
        img.preserveAspect = true;
        img.raycastTarget = false;
        if (img.sprite == null) img.enabled = false;
        return img;
    }

    public static Image Picture(RectTransform parent, Sprite s, float x, float y, float w, float h)
    {
        var rt = Node("Picture", parent, x, y, w, h);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = s;
        img.preserveAspect = true;
        img.raycastTarget = false;
        if (s == null) img.enabled = false;
        return img;
    }

    // Pink primary button (with its little drop shadow) or white secondary one.
    public static Button Button(RectTransform parent, string name, float x, float y, float w, float h,
                                bool primary, string text, string icon, float textSize, Action onClick)
    {
        float r = h >= 50f ? 20f : 18f;
        if (primary) Box(parent, name + "Shadow", x, y + 4f, w, h, Shadow, r);
        var fill = Box(parent, name, x, y, w, h, primary ? Pink : Card, r, primary ? PinkEdge : Border, 1.5f);
        var root = (RectTransform)(fill.transform.parent);
        fill.raycastTarget = true;
        var b = root.gameObject.AddComponent<Button>();
        b.targetGraphic = fill;
        var colors = b.colors;
        colors.highlightedColor = new Color(0.94f, 0.94f, 0.94f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        colors.fadeDuration = 0.08f;
        b.colors = colors;
        b.onClick.AddListener(() =>
        {
            var cam = Camera.main;
            if (cam) AudioFX.Play(AudioFX.Pop, cam.transform.position, 0.35f);
            onClick?.Invoke();
        });

        var fg = primary ? Color.white : Plum;
        Label(root, text, Face.Medium, textSize, fg, 0f, 0f, w, h, TextAlignmentOptions.Center);
        if (!string.IsNullOrEmpty(icon))
            Icon(root, icon, 20f, (h - 21f) * 0.5f, 21f, primary ? Color.white : Pink);
        return b;
    }

    // small square icon button from the top bars (back, home, settings)
    public static Button IconButton(RectTransform parent, string name, float x, float y, string icon, Action onClick)
    {
        var fill = Box(parent, name, x, y, 44f, 44f, Card, 16f, Border, 1f);
        fill.raycastTarget = true;
        var root = (RectTransform)fill.transform.parent;
        var b = root.gameObject.AddComponent<Button>();
        b.targetGraphic = fill;
        b.onClick.AddListener(() =>
        {
            var cam = Camera.main;
            if (cam) AudioFX.Play(AudioFX.Pop, cam.transform.position, 0.35f);
            onClick?.Invoke();
        });
        Icon(root, icon, 10f, 10f, 24f, Pink);
        return b;
    }

    // the cozy on/off switch: pink track with a tick, or grey with a dash
    public static void Switch(RectTransform parent, float x, float y, bool on, Action<bool> changed)
    {
        var track = Box(parent, "Switch", x, y, 56f, 32f, on ? Pink : Off, 16f);
        track.raycastTarget = true;
        var thumb = (RectTransform)Box((RectTransform)track.transform, "Thumb", 0f, 4f, 24f, 24f, Card, 12f).transform;
        var mark = Icon((RectTransform)track.transform, "tick", 0f, 6f, 20f, Color.white);
        void Show()
        {
            track.color = on ? Pink : Off;
            thumb.anchoredPosition = new Vector2(on ? 28f : 4f, -4f);
            mark.sprite = Skin.Icon(on ? "tick" : "dash");
            mark.enabled = mark.sprite;
            mark.color = on ? Color.white : Plum;
            ((RectTransform)mark.transform).anchoredPosition = new Vector2(on ? 4f : 32f, -6f);
        }
        Show();
        var b = track.gameObject.AddComponent<Button>();
        b.targetGraphic = track;
        b.onClick.AddListener(() =>
        {
            on = !on;
            Show();
            var cam = Camera.main;
            if (cam) AudioFX.Play(AudioFX.Pop, cam.transform.position, 0.3f);
            changed?.Invoke(on);
        });
    }

    // a row of 5 hearts: filled pink for earned, pale for the rest
    public static void Hearts(RectTransform parent, float x, float y, int filled, float size = 21f, float gap = 7f)
    {
        for (int i = 0; i < 5; i++)
            Icon(parent, "heart_fill", x + i * (size + gap), y, size, i < filled ? Pink : PaleHeart);
    }
}
