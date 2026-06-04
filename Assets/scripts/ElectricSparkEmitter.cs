using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ElectricSparkEmitter : MonoBehaviour
{
    [Header("Burst")]
    public int sparksPerBurst = 80;
    public float burstInterval = 0.3f;
    public bool randomizeBurstTiming = true;

    [Header("Spark Look")]
    public float sparkSpeedMin = 1f;
    public float sparkSpeedMax = 3.5f;
    public float sparkLifetimeMin = 0.12f;
    public float sparkLifetimeMax = 0.45f;
    [ColorUsage(true, true)] public Color sparkColor = new Color(0.6f, 0.85f, 2.5f, 1f);
    [Tooltip("How much velocity stretches each spark into a streak. Small = short fine sparks.")]
    public float streakLength = 0.04f;
    [Tooltip("Max thickness of each spark in world units. Keep tiny for hair-thin sparks.")]
    public float startSize = 0.014f;
    public bool useGravity = true;
    public float gravity = 1.5f;

    [Header("Trail")]
    public bool enableTrails = true;
    [Range(0f, 1f)] public float trailRatio = 0.4f;
    public float trailWidth = 0.05f;

    [Header("Flicker Light")]
    public bool enableLight = true;
    public float lightIntensity = 800f;
    public float lightRange = 3f;

    ParticleSystem _ps;
    Light _flickerLight;
    float _nextBurstTime;

    void OnEnable()
    {
        _ps = GetComponent<ParticleSystem>();
        Configure();
        if (enableLight) EnsureLight();
        _nextBurstTime = Time.time + 0.05f;
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        _ps = GetComponent<ParticleSystem>();
        Configure();
        if (enableLight) EnsureLight();
        else RemoveLight();
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        if (_flickerLight == null) return;

        // Pulse the light in sync with the burst cadence; the ParticleSystem's
        // own burst module drives the actual spark emission.
        if (Time.time >= _nextBurstTime)
        {
            float jitter = randomizeBurstTiming ? Random.Range(0.5f, 1.5f) : 1f;
            _nextBurstTime = Time.time + burstInterval * jitter;
            _flickerLight.intensity = lightIntensity * Random.Range(0.5f, 1.2f);
        }
        else
        {
            _flickerLight.intensity = Mathf.Lerp(_flickerLight.intensity, 0f, Time.deltaTime * 18f);
        }
    }

    void Configure()
    {
        // duration/loop can only be changed while the system is stopped.
        bool wasPlaying = _ps.isPlaying;
        _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = _ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(sparkLifetimeMin, sparkLifetimeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(sparkSpeedMin, sparkSpeedMax);
        main.startSize = new ParticleSystem.MinMaxCurve(startSize * 0.45f, startSize);
        main.startColor = sparkColor;
        main.gravityModifier = useGravity ? gravity : 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 2000;
        main.scalingMode = ParticleSystemScalingMode.Local;

        var emission = _ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        // Self-driven repeating bursts so the effect plays in the Scene view
        // (edit mode) as well as in Play mode, without relying on Update().
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)sparksPerBurst, (short)sparksPerBurst, 0, burstInterval)
        });

        var shape = _ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.02f;

        var vol = _ps.velocityOverLifetime;
        vol.enabled = true;
        vol.space = ParticleSystemSimulationSpace.Local;
        vol.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
        vol.y = new ParticleSystem.MinMaxCurve(-1f, 1f);
        vol.z = new ParticleSystem.MinMaxCurve(-1f, 1f);

        var col = _ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(sparkColor, 0.25f),
                new GradientColorKey(new Color(0.2f, 0.4f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var sol = _ps.sizeOverLifetime;
        sol.enabled = true;
        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.7f, 0.6f),
            new Keyframe(1f, 0f));
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var noise = _ps.noise;
        noise.enabled = true;
        noise.strength = 1.2f;
        noise.frequency = 2f;
        noise.scrollSpeed = 1.5f;
        noise.quality = ParticleSystemNoiseQuality.High;

        var trails = _ps.trails;
        trails.enabled = enableTrails;
        trails.mode = ParticleSystemTrailMode.PerParticle;
        trails.ratio = trailRatio;
        trails.lifetime = new ParticleSystem.MinMaxCurve(0.03f);
        trails.minVertexDistance = 0.01f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(trailWidth);
        trails.dieWithParticles = true;
        trails.sizeAffectsWidth = true;
        trails.inheritParticleColor = true;
        trails.colorOverTrail = new ParticleSystem.MinMaxGradient(Color.white);

        var renderer = _ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 1.5f;
        renderer.velocityScale = streakLength;
        renderer.alignment = ParticleSystemRenderSpace.Velocity;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        if (renderer.sharedMaterial == null || renderer.sharedMaterial.name == "Default-Material")
        {
            var mat = FindOrCreateSparkMaterial();
            if (mat != null) renderer.sharedMaterial = mat;
        }
        if (enableTrails && renderer.trailMaterial == null)
        {
            renderer.trailMaterial = renderer.sharedMaterial;
        }

        if (wasPlaying || Application.isPlaying)
            _ps.Play();
    }

    Material FindOrCreateSparkMaterial()
    {
#if UNITY_EDITOR
        const string path = "Assets/scripts/ElectricSpark.mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("HDRP/Unlit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) return null;

        var mat = new Material(shader) { name = "ElectricSpark" };
        mat.enableInstancing = true;

        if (mat.HasProperty("_SurfaceType")) mat.SetFloat("_SurfaceType", 1f);
        if (mat.HasProperty("_BlendMode")) mat.SetFloat("_BlendMode", 1f);
        if (mat.HasProperty("_EmissiveColor"))
            mat.SetColor("_EmissiveColor", sparkColor * 8f);
        if (mat.HasProperty("_UseEmissiveIntensity"))
            mat.SetFloat("_UseEmissiveIntensity", 0f);
        if (mat.HasProperty("_UnlitColor"))
            mat.SetColor("_UnlitColor", sparkColor);
        mat.color = sparkColor;

        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
#else
        return null;
#endif
    }

    void EnsureLight()
    {
        var t = transform.Find("SparkLight");
        if (t == null)
        {
            var go = new GameObject("SparkLight");
            go.transform.SetParent(transform, false);
            _flickerLight = go.AddComponent<Light>();
        }
        else
        {
            _flickerLight = t.GetComponent<Light>();
            if (_flickerLight == null) _flickerLight = t.gameObject.AddComponent<Light>();
        }
        _flickerLight.type = LightType.Point;
        _flickerLight.color = sparkColor;
        _flickerLight.range = lightRange;
        _flickerLight.intensity = 0f;
        _flickerLight.shadows = LightShadows.None;
    }

    void RemoveLight()
    {
        var t = transform.Find("SparkLight");
        if (t == null) return;
        if (Application.isPlaying) Destroy(t.gameObject);
        else DestroyImmediate(t.gameObject);
        _flickerLight = null;
    }

    public void EmitOnce(int count = -1)
    {
        if (_ps == null) _ps = GetComponent<ParticleSystem>();
        if (_ps == null) return;
        _ps.Emit(count > 0 ? count : sparksPerBurst);
        if (_flickerLight != null)
            _flickerLight.intensity = lightIntensity * Random.Range(0.7f, 1.3f);
    }

#if UNITY_EDITOR
    [MenuItem("GameObject/Effects/Electric Spark Emitter", false, 10)]
    static void CreateInScene(MenuCommand cmd)
    {
        var go = new GameObject("ElectricSparkEmitter");
        GameObjectUtility.SetParentAndAlign(go, cmd.context as GameObject);
        go.AddComponent<ParticleSystem>();
        go.AddComponent<ElectricSparkEmitter>();
        Undo.RegisterCreatedObjectUndo(go, "Create Electric Spark Emitter");
        Selection.activeObject = go;
    }
#endif
}
