using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(CombatManager), typeof(TouchingDirections))]
[RequireComponent(typeof(Damageable))]
public class PlayerController : MonoBehaviour
{

    #region Fields

    // Configuration fields
    [Header("Movement")]
    public float walkSpeed = 5f;
    private float acceleration = 80f;
    private float deceleration = 100f;

    [Header("Jump")]
    public float jumpImpulse = 10f;

    [Header("Dodge")]
    public float dodgeSpeed = 8f;
    private bool wasOnWall;
    Vector2 moveInput;

    [Header("Combat")]

    // Serialized fields
    [SerializeField]
    private bool _isMoving = false;
    public bool _IsFacingRight = true;

    // Component references
    public TouchingDirections touchingDirections;
    public Damageable damageable;
    public Rigidbody2D rb;
    public Animator animator;
    public CombatManager _combatManager;

    #endregion

    #region Properties

    public bool IsAlive
    {
        get
        {
            return animator.GetBool(AnimationStrings.IsAlive);
        }
    }

    public bool IsMoving
    {
        get
        {
            return _isMoving;
        }
        private set
        {
            _isMoving = value;
            animator.SetBool(AnimationStrings.IsMoving, value);
        }
    }

    public bool IsFacingRight
    {
        get { return _IsFacingRight; }
        private set
        {
            if (_IsFacingRight != value)
            {
                // Flip the sprite using the local scale 
                transform.localScale *= new Vector2(-1, 1);

            }

            _IsFacingRight = value;
        }
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool(AnimationStrings.CanMove);
        }
    }

    public bool IsDodging
    {
        get
        {
            return animator.GetBool(AnimationStrings.IsDodging);
        }
    }

    public bool IsAttacking
    {
        get
        {
            return animator.GetBool(AnimationStrings.IsAttacking);
        }
        private set
        {
            animator.SetBool(AnimationStrings.IsAttacking, value);
        }
    }

    #endregion

    #region Unity Lifecycle Methods

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
        _combatManager = GetComponent<CombatManager>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        // Update facing direction if not dodging and there's input
        if (!IsDodging && moveInput != Vector2.zero)
        {
            SetFacingDirection(moveInput);
        }
    }

    private void FixedUpdate()
    {
        if (!damageable.LockVelocity && CanMove)
        {
            float targetSpeed = moveInput.x * walkSpeed;

            float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

            float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
        }

        if (IsDodging)
        {
            float direction = IsFacingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * dodgeSpeed, rb.linearVelocity.y);
        }

        animator.SetFloat(AnimationStrings.yVelocity, rb.linearVelocity.y);
    }

    #endregion

    #region Input/Action Methods

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (IsAlive)
        {
            IsMoving = moveInput != Vector2.zero;
        }
        else
        {
            IsMoving = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (CanMove && touchingDirections.IsGrounded && !IsDodging)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
            }
        }
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (_combatManager != null)
        {
            _combatManager.OnAttack(context);
        }
    }

    public void LockMovementDuringAttack(int shouldLock)
    {
        animator.SetBool(AnimationStrings.lockVelocity, shouldLock == 1);
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (_combatManager != null)
        {
            _combatManager.OnDodge(context);
        }
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        if (animator.GetBool(AnimationStrings.IsDodging))
        {
            return;
        }
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);
    }

    public void OnHeal(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            float healAmount = 30f;

            bool healed = damageable.Heal(healAmount);

            if (healed)
            {
                Debug.Log("Healed for" + healAmount);
            }
            else
            {
                Debug.Log("Heal failed");
            }
        }
    }

    #endregion

    #region Helper Methods

    private void SetFacingDirection(Vector2 moveInput)
    {
        if (IsDodging || IsAttacking)
        {
            return;
        }

        if (moveInput.x > 0 && !IsFacingRight)
        {
            IsFacingRight = true;
        }
        else if (moveInput.x < 0 && IsFacingRight)
        {
            IsFacingRight = false;
        }
    }

    #endregion

}
