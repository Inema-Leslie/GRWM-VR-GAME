// Glow Up Morning - real-time planar mirror for URP (works with the XR camera).
// Put it on a Quad: the quad's front (the side you see) is the mirror surface.
// A hidden camera renders the room from the reflected eye position into a
// RenderTexture shown on the quad. It only renders while the mirror is on screen
// and the player is close, to keep the frame rate up on Quest.

using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class PlanarMirror : MonoBehaviour
{
    [Tooltip("Resolution of the reflection texture. 1024 looks good, 512 is cheaper.")]
    public int textureSize = 1024;

    [Tooltip("Stop rendering the reflection when the player is further away than this (metres).")]
    public float maxRenderDistance = 6f;

    [Tooltip("Layers the mirror can see. The mirror's own layer is removed automatically.")]
    public LayerMask reflectLayers = ~0;

    Camera mirrorCam;
    RenderTexture rt;
    MeshRenderer rend;
    MaterialPropertyBlock mpb;
    static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    void OnEnable()
    {
        rend = GetComponent<MeshRenderer>();
        CreateResources();
    }

    void OnDisable()
    {
        if (mirrorCam) DestroyImmediate(mirrorCam.gameObject);
        if (rt) { rt.Release(); DestroyImmediate(rt); }
        if (rend) rend.SetPropertyBlock(null);
    }

    void CreateResources()
    {
        // match the texture to the glass shape so the reflection stays sharp on tall mirrors
        Vector3 s = transform.lossyScale;
        float aspect = Mathf.Abs(s.x) / Mathf.Max(0.001f, Mathf.Abs(s.y));
        int w = aspect >= 1f ? textureSize : Mathf.RoundToInt(textureSize * aspect);
        int h = aspect >= 1f ? Mathf.RoundToInt(textureSize / aspect) : textureSize;
        rt = new RenderTexture(Mathf.Max(64, w), Mathf.Max(64, h), 24) { name = "MirrorRT", antiAliasing = 4 };

        var go = new GameObject("MirrorCamera") { hideFlags = HideFlags.HideAndDontSave };
        mirrorCam = go.AddComponent<Camera>();
        mirrorCam.enabled = false;
        mirrorCam.targetTexture = rt;
        mirrorCam.cullingMask = reflectLayers & ~(1 << gameObject.layer);
        mirrorCam.clearFlags = CameraClearFlags.Skybox;
        var data = mirrorCam.GetUniversalAdditionalCameraData();
        data.renderShadows = false;            // cheaper, hardly visible in a mirror
        data.renderPostProcessing = false;

        // show the reflection on the quad without touching the material asset
        mpb = new MaterialPropertyBlock();
        mpb.SetTexture(BaseMap, rt);
        rend.SetPropertyBlock(mpb);
    }

    void LateUpdate()
    {
        var eyeCam = Camera.main;
#if UNITY_EDITOR
        if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView)
            eyeCam = UnityEditor.SceneView.lastActiveSceneView.camera;
#endif
        if (eyeCam == null || mirrorCam == null) return;

        Vector3 n = -transform.forward;              // mirror normal, pointing into the room
        Vector3 p = transform.position;
        Vector3 eye = eyeCam.transform.position;
        float side = Vector3.Dot(eye - p, n);

        bool show = rend.isVisible && side > 0.01f &&
                    Vector3.Distance(eye, p) < maxRenderDistance;
        mirrorCam.enabled = show;
        if (!show) return;

        // reflected eye behind the mirror
        Vector3 e = eye - 2f * side * n;

        // off-axis projection so the image lines up exactly with the quad
        Vector3 vr = transform.right, vu = transform.up, vn = -n;
        float hw = transform.lossyScale.x * 0.5f, hh = transform.lossyScale.y * 0.5f;
        Vector3 pa = p - vr * hw - vu * hh;          // bottom-left
        Vector3 pb = p + vr * hw - vu * hh;          // bottom-right
        Vector3 pc = p - vr * hw + vu * hh;          // top-left

        Vector3 va = pa - e, vb = pb - e, vc = pc - e;
        float d = -Vector3.Dot(va, vn);              // distance from reflected eye to the mirror
        float near = Mathf.Max(0.01f, d);            // near plane on the glass: nothing behind it shows
        float far = eyeCam.farClipPlane;
        float k = near / d;
        float l = Vector3.Dot(vr, va) * k, r = Vector3.Dot(vr, vb) * k;
        float b = Vector3.Dot(vu, va) * k, t = Vector3.Dot(vu, vc) * k;

        var view = Matrix4x4.identity;
        view.SetRow(0, new Vector4(vr.x, vr.y, vr.z, -Vector3.Dot(vr, e)));
        view.SetRow(1, new Vector4(vu.x, vu.y, vu.z, -Vector3.Dot(vu, e)));
        view.SetRow(2, new Vector4(vn.x, vn.y, vn.z, -Vector3.Dot(vn, e)));

        mirrorCam.transform.SetPositionAndRotation(e, Quaternion.LookRotation(-vn, vu));
        mirrorCam.nearClipPlane = near;
        mirrorCam.farClipPlane = far;
        mirrorCam.worldToCameraMatrix = view;
        mirrorCam.projectionMatrix = Matrix4x4.Frustum(l, r, b, t, near, far);
    }
}
