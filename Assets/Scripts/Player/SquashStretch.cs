using System.Collections;
using UnityEngine;


public class SquashStretch : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Rigidbody2D rb;

    [Header("Squash & Stretch en vol")]
    [Tooltip("À quelle vitesse verticale le stretch max est atteint")]
    [SerializeField] private float maxVelocityY = 16f;
    [SerializeField] private float maxStretchY = 1.35f; 
    [SerializeField] private float maxSquashY = 0.7f;   
    [SerializeField] private float airLerpSpeed = 8f;    

    [Header("Impact à l'atterrissage")]
    [SerializeField] private float landSquashY = 0.55f;  
    [SerializeField] private float landSquashDuration = 0.08f;
    [SerializeField] private float landRecoverDuration = 0.18f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Anticipation du saut")]
    [SerializeField] private float jumpAnticipateSquash = 0.75f;
    [SerializeField] private float jumpAnticipateStretch = 1.3f;
    [SerializeField] private float jumpAnticipateTime = 0.06f;

    [Header("Dash")]
    [SerializeField] private float dashStretchX = 1.4f;  
    [SerializeField] private float dashSquashY = 0.7f;
    [SerializeField] private float dashLerpSpeed = 20f;

    [Header("Squash au slide")]
    [SerializeField] private float slideSquashY = 0.65f;
    [SerializeField] private float slideLerpSpeed = 12f;

    [Header("Retour au neutre")]
    [SerializeField] private float returnLerpSpeed = 10f;


    private Vector3 targetScale = Vector3.one;
    private bool wasGrounded;
    private bool isGrounded;
    private Coroutine landCoroutine;
    private Coroutine jumpCoroutine;


    private bool IsDashing => playerController.IsDashing;
    private bool IsSliding => playerController.IsSliding;
    private bool IsGrounded => playerController.IsGrounded;


    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (visualTransform == null) visualTransform = transform;
    }

    private void Update()
    {
        Debug.Log($"isGrounded:{IsGrounded} | localScale:{visualTransform.localScale} | targetScale:{targetScale} | landCoroutine:{landCoroutine != null}");

        wasGrounded = isGrounded;
        isGrounded = IsGrounded;
         
        if (!wasGrounded && isGrounded)
            OnLand();

        ComputeTargetScale();
        ApplyScale();
    }


    private void ComputeTargetScale()
    {
        if (landCoroutine != null || jumpCoroutine != null) return;

        if (IsDashing)
        {
            targetScale = new Vector3(dashStretchX, dashSquashY, 1f);
            return;
        }

        if (IsSliding)
        {
            float t = Mathf.Abs(rb.linearVelocity.x) / 10f;
            targetScale = new Vector3(
                Mathf.Lerp(1f, 1.2f, t),
                Mathf.Lerp(1f, slideSquashY, t),
                1f);
            return;
        }

        if (isGrounded)
        {
            float vx = Mathf.Abs(rb.linearVelocity.x);
            float runT = Mathf.Clamp01(vx / moveSpeed);
            targetScale = new Vector3(
                Mathf.Lerp(1f, 1.08f, runT),
                Mathf.Lerp(1f, 0.94f, runT),
                1f);
            return;
        }

        float vy = rb.linearVelocity.y;
        float normalizedVy = Mathf.Clamp(vy / maxVelocityY, -1f, 1f);

        float scaleY = normalizedVy > 0
            ? Mathf.Lerp(1f, maxStretchY, normalizedVy)
            : Mathf.Lerp(1f, maxSquashY, -normalizedVy);

        float scaleX = Mathf.Clamp(1f / Mathf.Max(0.1f, scaleY), 0.8f, 1.2f);
        targetScale = new Vector3(scaleX, scaleY, 1f);
    }

    private void ApplyScale()
    {
        if (landCoroutine != null || jumpCoroutine != null) return;

        if (isGrounded && !IsDashing && !IsSliding)
        {
            visualTransform.localScale = Vector3.MoveTowards(
                visualTransform.localScale,
                targetScale,
                returnLerpSpeed * Time.deltaTime);
            return;
        }

        float speed = IsDashing ? dashLerpSpeed
                    : IsSliding ? slideLerpSpeed
                    : airLerpSpeed;

        Vector3 result = Vector3.Lerp(visualTransform.localScale, targetScale, speed * Time.deltaTime);

        if (Vector3.Distance(result, targetScale) < 0.005f)
            result = targetScale;

        visualTransform.localScale = result;
    }


    private void OnLand()
    {
        if (landCoroutine != null) StopCoroutine(landCoroutine);
        landCoroutine = StartCoroutine(LandRoutine());
    }

    private IEnumerator LandRoutine()
    {
        float t = 0f;
        while (t < landSquashDuration)
        {
            t += Time.deltaTime;
            float p = t / landSquashDuration;
            visualTransform.localScale = Vector3.Lerp(
                Vector3.one,
                new Vector3(1f / landSquashY, landSquashY, 1f),
                p);
            yield return null;
        }

        t = 0f;
        Vector3 squashedScale = visualTransform.localScale;
        while (t < landRecoverDuration)
        {
            t += Time.deltaTime;
            float p = t / landRecoverDuration;
            float ease = 1f - Mathf.Pow(1f - p, 3f);
            visualTransform.localScale = Vector3.Lerp(squashedScale, Vector3.one, ease);
            yield return null;
        }

        visualTransform.localScale = Vector3.one;
        landCoroutine = null;
    }

    public void OnJump()
    {
        if (jumpCoroutine != null) StopCoroutine(jumpCoroutine);
        jumpCoroutine = StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        float t = 0f;
        while (t < jumpAnticipateTime * 0.5f)
        {
            t += Time.deltaTime;
            float p = t / (jumpAnticipateTime * 0.5f);
            visualTransform.localScale = Vector3.Lerp(
                Vector3.one,
                new Vector3(1f / jumpAnticipateSquash, jumpAnticipateSquash, 1f),
                p);
            yield return null;
        }

        t = 0f;
        while (t < jumpAnticipateTime * 0.5f)
        {
            t += Time.deltaTime;
            float p = t / (jumpAnticipateTime * 0.5f);
            visualTransform.localScale = Vector3.Lerp(
                new Vector3(1f / jumpAnticipateSquash, jumpAnticipateSquash, 1f),
                new Vector3(1f / jumpAnticipateStretch, jumpAnticipateStretch, 1f),
                p);
            yield return null;
        }

        jumpCoroutine = null;
    }
}
