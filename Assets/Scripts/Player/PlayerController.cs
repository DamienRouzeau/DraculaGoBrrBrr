using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    //  INSPECTOR 

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float maxHorizontalSpeed = 28f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 16f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Dash")]
    [SerializeField] private float dashAddedSpeed = 12f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.6f;
    [SerializeField] private float dashInvincibilityDurations = 0.12f;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private float invincibilityOpacity = 0.1f;


    [Header("Slide")]
    [SerializeField] private float slideAddedSpeed = 8f;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private float slideDeceleration = 18f;

    [Header("Crouch")]
    [SerializeField] private Vector2 crouchSize = new Vector2(0.8f, 0.8f);
    [SerializeField] private Vector2 crouchOffset = new Vector2(0f, -0.4f);
    [SerializeField] private float crouchSpeed = 4f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Rewind")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameObject deadBody;

    [Header("Audio")]
    [SerializeField] private float stepInterval = 0.4f;

    [Header("Squash Stretch")]
    public bool IsDashing => isDashing;
    public bool IsSliding => isSliding;
    public bool IsGrounded => isGrounded;

    //  COMPONENTS 

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private SpriteRenderer sprite;

    //  INPUT 

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool dashPressed;
    private bool crouchHeld;

    //  STATE 

    private bool isGrounded;
    private bool wasGrounded;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float dashCooldownTimer;
    private float dashTimer;
    private float slideTimer;
    private float invincibilityTimer;
    private float stepTimer = 0.4f;

    private bool isDashing;
    private bool isSliding;
    private bool isCrouching;

    private bool slideEndedNaturally;

    private float dashDirection;
    private bool isInvincible;

    private Vector2 defaultSize;
    private Vector2 defaultOffset;

    // MISCELLANEOUS
    private InterractibleObject interractibleObject;


    #region Unity lifecycle

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        sprite = GetComponent<SpriteRenderer>();

        defaultSize = col.size;
        defaultOffset = col.offset;

        inputActions = new PlayerInputActions();
    }

    private void Start()
    {
        GameManager.instance.SubscribeRewind(Rewind);
        inputActions?.Enable();
    }

    private void OnEnable()
    {
        inputActions?.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => { jumpPressed = true; jumpHeld = true; };
        inputActions.Player.Jump.canceled += ctx => jumpHeld = false;

        inputActions.Player.Dash.performed += ctx => dashPressed = true;

        inputActions.Player.Crouch.performed += ctx => crouchHeld = true;
        inputActions.Player.Crouch.canceled += ctx => OnCrouchReleased();

        inputActions.Player.Interact.performed += ctx => Interract();

    }

    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        TickTimers();
        CheckGround();
        HandleCrouchAndSlide();
        HandleDash();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (!isDashing)
            ApplyMovement();

        ApplyGravity();
        ClampSpeed();
    }

    #endregion

    #region Timers

    private void TickTimers()
    {
        float dt = Time.deltaTime;

        if (coyoteTimer > 0) coyoteTimer -= dt;
        if (jumpBufferTimer > 0) jumpBufferTimer -= dt;
        if (dashCooldownTimer > 0) dashCooldownTimer -= dt;
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= dt;
        }
        else
        {
            isInvincible = false;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1);
            dashParticles.Stop();

        }
        if(stepTimer > 0 && isGrounded && (moveInput.x > 0.1f || moveInput.x < -0.1f) && !isDashing && !isSliding) stepTimer -= dt;
        if (stepTimer <= 0)
        {
            AudioManager.instance.PlayAudio(transform, "BetonStep", 0.4f, Random.Range(0.9f, 1.1f));
            stepTimer = stepInterval;
        }


        if (isDashing)
        {
            dashTimer -= dt;
            if (dashTimer <= 0) EndDash();
        }

        if (isSliding)
        {
            slideTimer -= dt;
            if (slideTimer <= 0) EndSlideNaturally();
        }
    }

    #endregion

    #region Ground check

    private void CheckGround()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (wasGrounded && !isGrounded)
            coyoteTimer = coyoteTime;
        if (!wasGrounded && isGrounded)
        {
            AudioManager.instance.PlayAudio(transform, "BetonStep", 0.55f, Random.Range(0.8f, 0.9f));
        }

    }

    #endregion

    #region Crouch & Slide

    private void OnCrouchReleased()
    {
        crouchHeld = false;

        if (isCrouching && !isSliding)
        {
            TryStandUp();
            return;
        }

    }

    private void HandleCrouchAndSlide()
    {
        if (isDashing) return;

        if (crouchHeld && isGrounded && !isCrouching)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.5f)
                StartSlide();
            else
                StartCrouch();
        }

        if (isSliding)
        {
            bool holdingDir = Mathf.Abs(moveInput.x) > 0.01f
                              && Mathf.Sign(moveInput.x) == Mathf.Sign(rb.linearVelocity.x);

            float decel = holdingDir ? slideDeceleration * 0.3f : slideDeceleration;

            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, decel * Time.deltaTime),
                rb.linearVelocity.y);
        }
    }

    private void StartCrouch()
    {
        isCrouching = false; 
        slideEndedNaturally = false;
        isCrouching = true;
        SetCrouchCollider();
    }

    private void StartSlide()
    {
        isSliding = true;
        isCrouching = true;
        slideEndedNaturally = false;
        slideTimer = slideDuration;

        float dir = Mathf.Sign(rb.linearVelocity.x);
        if (Mathf.Approximately(dir, 0f)) dir = transform.localScale.x;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x + dir * slideAddedSpeed,
            rb.linearVelocity.y);
        AudioManager.instance.PlayAudio(transform, "Slide");


        SetCrouchCollider();
    }

    private void EndSlideNaturally()
    {
        isSliding = false;
        slideTimer = 0f;
        slideEndedNaturally = true;

        if (crouchHeld)
        {

        }
        else
        {
            TryStandUp();
        }
    }

    private void InterruptSlide()
    {
        isSliding = false;
        slideTimer = 0f;
        slideEndedNaturally = false;
        SetStandCollider();
    }

    private void TryStandUp()
    {
        if (CanStandUp())
            SetStandCollider();
    }

    private void SetCrouchCollider()
    {
        col.size = crouchSize;
        col.offset = crouchOffset;
    }

    private void SetStandCollider()
    {
        isCrouching = false;
        col.size = defaultSize;
        col.offset = defaultOffset;
    }

    private bool CanStandUp()
    {
        Vector2 origin = (Vector2)transform.position + defaultOffset;
        return Physics2D.BoxCast(origin, defaultSize * 0.9f, 0f, Vector2.up, 0.05f, groundLayer).collider == null;
    }

    #endregion

    #region Jump

    private void HandleJumpInput()
    {
        if (jumpPressed)
        {
            jumpBufferTimer = jumpBufferTime;
            jumpPressed = false;
        }

        bool canJump = (isGrounded || coyoteTimer > 0) && !isCrouching;

        if (jumpBufferTimer > 0 && isSliding)
        {
            InterruptSlide();
            isCrouching = false; 
        }

        if (jumpBufferTimer > 0 && (isGrounded || coyoteTimer > 0) && !isCrouching)
        {
            GetComponent<SquashStretch>()?.OnJump();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferTimer = 0;
            AudioManager.instance.PlayAudio(transform, "Jump");
            coyoteTimer = 0;
        }
    }

    #endregion

    #region Dash

    private void HandleDash()
    {
        if (!dashPressed) return;
        dashPressed = false;

        if (isDashing || dashCooldownTimer > 0) return;

        if (isSliding) InterruptSlide();

        dashDirection = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : transform.localScale.x;

        float newVx = rb.linearVelocity.x + dashDirection * dashAddedSpeed;

        isDashing = true;
        isInvincible = true;
        invincibilityTimer = dashInvincibilityDurations;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(newVx, 0f);
        dashParticles.Play();
        AudioManager.instance.PlayAudio(transform, "Dash");
        sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, invincibilityOpacity);

    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = 1f;
    }

    #endregion

    #region Movement

    private void ApplyMovement()
    {
        if (isSliding) return;

        float currentVx = rb.linearVelocity.x;
        float targetVx = isCrouching ? moveInput.x * crouchSpeed : moveInput.x * moveSpeed;

        bool pushingSameDir = Mathf.Abs(moveInput.x) > 0.01f
                              && Mathf.Sign(moveInput.x) == Mathf.Sign(currentVx);

        if (pushingSameDir && Mathf.Abs(currentVx) > Mathf.Abs(targetVx))
        {
            float friction = isGrounded ? deceleration * 0.15f : deceleration * 0.05f;
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(currentVx, targetVx, friction * Time.fixedDeltaTime),
                rb.linearVelocity.y);
            return;
        }

        float rate = Mathf.Abs(targetVx) > 0.01f ? acceleration : deceleration;
        rb.linearVelocity = new Vector2(
            Mathf.MoveTowards(currentVx, targetVx, rate * Time.fixedDeltaTime),
            rb.linearVelocity.y);

        if (moveInput.x > 0.01f) sprite.flipX = false;
        else if (moveInput.x < -0.01f) sprite.flipX = true;
    }

    #endregion

    #region Gravity

    private void ApplyGravity()
    {
        if (isDashing) return;

        float g = Physics2D.gravity.y;

        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * g * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
            rb.linearVelocity += Vector2.up * g * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    #endregion

    #region Speed cap

    private void ClampSpeed()
    {
        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) > maxHorizontalSpeed)
            rb.linearVelocity = new Vector2(Mathf.Sign(vx) * maxHorizontalSpeed, rb.linearVelocity.y);
    }

    #endregion

    #region Collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("trap") && !isInvincible)
        {
            GameObject _deadBody = Instantiate(deadBody, transform.position, deadBody.transform.rotation);
            GameManager.instance.Rewind();
            AudioManager.instance.PlayAudio(transform, "DeathSlash");

        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("trap") && !isInvincible)
        {
            GameObject _deadBody = Instantiate(deadBody, transform.position, deadBody.transform.rotation);
            GameManager.instance.Rewind();
            AudioManager.instance.PlayAudio(transform, "DeathSlash");
            isInvincible = true;
        }
        if (collision.CompareTag("object"))
        {
            ObjectBehaviour _obj = collision.gameObject.GetComponent<ObjectBehaviour>();
            _obj.Collect();
            AudioManager.instance.PlayAudio(transform, "Item");

        }
        if (collision.CompareTag("interractibleObject"))
        {
            interractibleObject = collision.gameObject.GetComponent<InterractibleObject>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("interractibleObject"))
        {
            interractibleObject = null;
        }
    }
    #endregion

    #region Rewind
    private void Rewind()
    {
        transform.position = respawnPoint.position;
    }

    public void StopBeingInvincible()
    {
        isInvincible = false;
    }
    #endregion

    #region Interract
    private void Interract()
    {
        interractibleObject?.LeverTrigger();
    }
    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    #endregion
}