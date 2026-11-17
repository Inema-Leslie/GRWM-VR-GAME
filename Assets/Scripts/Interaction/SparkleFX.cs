using UnityEngine;

// Pink and white sparkle burst when a step is done. Also makes the sink's water.
public class SparkleFX : MonoBehaviour
{
    public Material particleMaterial;
    static SparkleFX instance;

    void Awake() => instance = this;

    public static void Burst(Vector3 at)
    {
        if (instance == null) return;
        var ps = instance.Make("Sparkles", at, null);
        var main = ps.main;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.04f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.55f, 0.8f), Color.white);
        main.gravityModifier = -0.05f;
        main.stopAction = ParticleSystemStopAction.Destroy;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;
        ps.Play();
    }

    // thin stream of water falling from the tap; caller plays/stops it
    public static ParticleSystem MakeWater(Transform spout)
    {
        if (instance == null) return null;
        var ps = instance.Make("Water", spout.position, spout);
        var main = ps.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = 0.35f;
        main.startSpeed = 0.2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.01f, 0.02f);
        main.startColor = new Color(0.7f, 0.88f, 1f, 0.8f);
        main.gravityModifier = 1f;
        main.maxParticles = 200;
        var em = ps.emission;
        em.rateOverTime = 150f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.006f;
        ps.transform.rotation = Quaternion.LookRotation(Vector3.down);
        return ps;
    }

    ParticleSystem Make(string name, Vector3 at, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, true);
        go.transform.position = at;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var r = go.GetComponent<ParticleSystemRenderer>();
        if (particleMaterial) r.sharedMaterial = particleMaterial;
        return ps;
    }
}
