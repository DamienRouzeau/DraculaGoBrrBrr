using UnityEngine;


public class GroundParticles : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody2D rb;

    [Header("Position")]
    [Tooltip("Offset depuis le centre du player pour spawner les particules (ex: 0, -0.9)")]
    [SerializeField] private Vector2 particleOffset = new Vector2(0f, -0.9f);

    [Header("Landing")]
    [SerializeField] private float landingMinSpeed = 3f; 
    [SerializeField] private int landingBurstCount = 12;
    [SerializeField] private float landingSpreadSpeed = 4f;
    [SerializeField] private Color landingColor = new Color(0.8f, 0.7f, 0.6f, 1f);

    [Header("Running")]
    [SerializeField] private float runMinSpeed = 2f;   
    [SerializeField] private float runEmissionRate = 18f;
    [SerializeField] private Color runColor = new Color(0.75f, 0.65f, 0.55f, 0.8f);

    [Header("Slide")]
    [SerializeField] private float slideEmissionRate = 35f;
    [SerializeField] private float slideSpreadSpeed = 6f;
    [SerializeField] private Color slideColor = new Color(0.9f, 0.75f, 0.6f, 1f);

    [Header("Particle Size")]
    [SerializeField] private float minSize = 0.05f;
    [SerializeField] private float maxSize = 0.18f;


    private ParticleSystem landingPS;
    private ParticleSystem runningPS;
    private ParticleSystem slidePS;

    private bool wasGrounded;




    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (playerController == null) playerController = GetComponent<PlayerController>();

        landingPS = CreateParticleSystem("LandingDust", landingColor, false);
        runningPS = CreateParticleSystem("RunningDust", runColor, true);
        slidePS = CreateParticleSystem("SlideDust", slideColor, true);

        ConfigureLanding();
        ConfigureRunning();
        ConfigureSlide();
    }

    private void Update()
    {
        bool grounded = playerController.IsGrounded;

        if (!wasGrounded && grounded)
        {
            float impactSpeed = Mathf.Abs(rb.linearVelocity.y);
            if (impactSpeed >= landingMinSpeed)
            {
                int count = Mathf.RoundToInt(Mathf.Lerp(
                    landingBurstCount * 0.5f,
                    landingBurstCount,
                    Mathf.Clamp01(impactSpeed / 20f)));

                MoveToFeet();
                landingPS.Emit(count);
            }
        }

        wasGrounded = grounded;

        bool shouldRunEmit = grounded
                             && !playerController.IsSliding
                             && !playerController.IsDashing
                             && Mathf.Abs(rb.linearVelocity.x) > runMinSpeed;

        var runEmission = runningPS.emission;
        runEmission.enabled = shouldRunEmit;

        if (shouldRunEmit)
        {
            MoveToFeet();
            float dir = Mathf.Sign(rb.linearVelocity.x);
            var runMain = runningPS.main;
            var runShape = runningPS.shape;
            runShape.rotation = new Vector3(0f, 0f, dir > 0 ? 180f : 0f);
        }

        bool shouldSlideEmit = grounded && playerController.IsSliding;

        var slideEmission = slidePS.emission;
        slideEmission.enabled = shouldSlideEmit;

        if (shouldSlideEmit)
        {
            MoveToFeet();
            float dir = Mathf.Sign(rb.linearVelocity.x);
            var slideShape = slidePS.shape;
            slideShape.rotation = new Vector3(0f, 0f, dir > 0 ? 180f : 0f);
        }
    }


    private void MoveToFeet()
    {
        Vector3 pos = transform.position + (Vector3)particleOffset;
        landingPS.transform.position = pos;
        runningPS.transform.position = pos;
        slidePS.transform.position = pos;
    }

    #region Particle System factory

    private ParticleSystem CreateParticleSystem(string psName, Color color, bool looping)
    {
        var go = new GameObject(psName);
        go.transform.SetParent(transform);
        go.transform.localPosition = particleOffset;

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;

        main.loop = looping;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = color;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
        main.gravityModifier = 0.4f;
        main.maxParticles = 60;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingLayerName = GetComponent<SpriteRenderer>() != null
            ? GetComponent<SpriteRenderer>().sortingLayerName
            : "Default";
        renderer.sortingOrder = -1;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        mat.SetFloat("_Surface", 1f);       
        mat.SetFloat("_BlendMode", 0f);     
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        renderer.material = mat;
        renderer.minParticleSize = 0f;
        renderer.maxParticleSize = 0.5f;

        var emission = ps.emission;
        emission.enabled = false;

        return ps;
    }

    private void ConfigureLanding()
    {
        var main = landingPS.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(landingSpreadSpeed * 0.5f, landingSpreadSpeed);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize * 1.5f, maxSize * 1.5f);

        var shape = landingPS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 60f;
        shape.radius = 0.3f;
        shape.rotation = new Vector3(-90f, 0f, 0f);
        var sizeOverLifetime = landingPS.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));

        var emission = landingPS.emission;
        emission.enabled = false;

    }

    private void ConfigureRunning()
    {
        var main = runningPS.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize * 0.8f);

        var shape = runningPS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.1f;
        shape.rotation = new Vector3(-75f, 0f, 0f);

        var emission = runningPS.emission;
        emission.rateOverTime = runEmissionRate;
        emission.enabled = false;

        var sizeOverLifetime = runningPS.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
        runningPS.Play();

    }

    private void ConfigureSlide()
    {
        var main = slidePS.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(slideSpreadSpeed * 0.5f, slideSpreadSpeed);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize * 1.2f);

        var shape = slidePS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.15f;
        shape.rotation = new Vector3(-80f, 0f, 0f);

        var emission = slidePS.emission;
        emission.rateOverTime = slideEmissionRate;
        emission.enabled = false;

        var sizeOverLifetime = slidePS.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0f));
        slidePS.Play();

    }

    #endregion
}