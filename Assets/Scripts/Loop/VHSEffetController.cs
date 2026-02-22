using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VHSEffectController : MonoBehaviour
{
    [SerializeField] private Image vhsImage; 

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Header("Intensités")]
    [SerializeField] private float maxGlitchIntensity = 0.6f;
    [SerializeField] private float maxGrainIntensity = 0.35f;
    [SerializeField] private float maxScanlineIntensity = 0.4f;

    private Material mat;

    private static readonly int PropIntensity = Shader.PropertyToID("_Intensity");
    private static readonly int PropGlitchIntensity = Shader.PropertyToID("_GlitchIntensity");
    private static readonly int PropGrainIntensity = Shader.PropertyToID("_GrainIntensity");
    private static readonly int PropScanlineIntensity = Shader.PropertyToID("_ScanlineIntensity");

    private Coroutine effectCoroutine;

    private void Start()
    {
        mat = new Material(vhsImage.material);
        vhsImage.material = mat;

        SetIntensity(0f);
        vhsImage.gameObject.SetActive(false);

        var recorder = FindFirstObjectByType<PlayerRecorder>();
        if (recorder != null)
            recorder.OnRewindComplete += OnRewindComplete;
    }

    public void OnRewindStart()
    {
        vhsImage.gameObject.SetActive(true);
        if (effectCoroutine != null) StopCoroutine(effectCoroutine);
        effectCoroutine = StartCoroutine(FadeIn());
    }

    private void OnRewindComplete()
    {
        if (effectCoroutine != null) StopCoroutine(effectCoroutine);
        effectCoroutine = StartCoroutine(FadeOut());
        FindFirstObjectByType<PlayerController>().StopBeingInvincible();
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            SetIntensity(Mathf.Clamp01(t / fadeInDuration));
            yield return null;
        }
        SetIntensity(1f);
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        float start = mat.GetFloat(PropIntensity);
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            SetIntensity(Mathf.Lerp(start, 0f, t / fadeOutDuration));
            yield return null;
        }
        SetIntensity(0f);
        vhsImage.gameObject.SetActive(false);
    }

    private void SetIntensity(float t)
    {
        mat.SetFloat(PropIntensity, t);
        mat.SetFloat(PropGlitchIntensity, maxGlitchIntensity * t);
        mat.SetFloat(PropGrainIntensity, maxGrainIntensity * t);
        mat.SetFloat(PropScanlineIntensity, maxScanlineIntensity * t);
    }
}