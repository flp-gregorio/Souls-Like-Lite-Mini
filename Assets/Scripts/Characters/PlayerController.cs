using Animation;
using Cameras;
using Combat;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CombatManager), typeof(TouchingDirections))]
    [RequireComponent(typeof(CharacterStats))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5f;
        private const float Acceleration = 120f;
        private const float Deceleration = 120f;
        private const float DirectionChangeDeceleration = 200;
        private const float AttackingDeceleration = 5000;

        [Header("Jump")]
        public float jumpImpulse = 14f;
        public float initialJumpBoostDuration = 0.1f;
        public float coyoteTime = 0.2f;
        public float jumpBufferTime = 0.2f;
        public float jumpCutMultiplier = 0.5f;
        public float fallGravityMult = 2.0f;
        public float fastFallGravityMult = 2.5f;
        public float jumpHangTimeThreshold = 1f;
        public float jumpHangGravityMult = 0.4f;
        public float jumpHangAccelerationMult = 1.1f;
        
        private float _lastGroundedTime;
        private float _lastJumpPressedTime;
        private bool _isJumping;
        private bool _jumpCut;
        private float _defaultGravityScale;
        private float _jumpStartTime;
        private bool _inJumpBoostPhase;
        private Vector2 _moveInput;

        [Header("Dodge")]
        public float dodgeSpeed = 8f;
        public float dodgeDuration;

        [Header("Dash")]
        public float dashSpeed = 18f;
        public float dashDuration = 0.2f;

        [Header("Combat")]
        [SerializeField]
        private bool isMoving;
        public bool isFacingRight = true;

        [FormerlySerializedAs("_cameraFollowGO")]
        [Header("Camera Stuff")]
        [SerializeField]
        private GameObject cameraFollowGo;

        [Header("References")]
        public TouchingDirections touchingDirections;
        public CharacterStats characterStats;
        public Rigidbody2D rb;
        public Animator animator;
        public CombatManager combatManager;
        private CameraFollowObject _cameraFollowObject;

        private bool IsAlive
        {
            get { return animator.GetBool(AnimationStrings.IsAlive); }
        }

        public bool IsMoving
        {
            get { return isMoving; }
            private set
            {
                isMoving = value;
                animator.SetBool(AnimationStrings.IsMoving, value);
            }
        }

        public bool IsFacingRight
        {
            get { return isFacingRight; }
            private set
            {
                if (isFacingRight != value)
                {
                    transform.localScale *= new Vector2(-1, 1);
                }
                isFacingRight = value;
            }
        }

        private bool CanMove
        {
            get { return animator.GetBool(AnimationStrings.CanMove); }
        }

        public bool IsDodging
        {
            get { return animator.GetBool(AnimationStrings.IsDodging); }
        }

        public bool IsDashing
        {
            get { return animator.GetBool(AnimationStrings.IsDashing); }
        }

        bool IsAttacking
        {
            get { return animator.GetBool(AnimationStrings.IsAttacking); }
        }

        private void Start()
        {
            _cameraFollowObject = cameraFollowGo.GetComponent<CameraFollowObject>();
            _defaultGravityScale = rb.gravityScale;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            touchingDirections = GetComponent<TouchingDirections>();
            characterStats = GetComponent<CharacterStats>();
            combatManager = GetComponent<CombatManager>();

            // Find dodge animation duration
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "RollPlayer")
                {
                    dodgeDuration = clip.length;
                    break;
                }
            }
        }

        private void Update()
        {
            // Update jump timers
            _lastGroundedTime -= Time.deltaTime;
            _lastJumpPressedTime -= Time.deltaTime;

            // Update grounded status
            if (touchingDirections.IsGrounded)
            {
                _lastGroundedTime = coyoteTime;

                // Reset jump state when landing
                if (!_isJumping)
                {
                    _jumpCut = false;
                }
            }

            // Jump checks
            if (_isJumping && rb.linearVelocity.y < 0)
            {
                _isJumping = false;
            }

            // Process jump if conditions are met
            if (!IsDashing && !IsDodging && CanMove &&
                _lastGroundedTime > 0 && _lastJumpPressedTime > 0)
            {
                _lastGroundedTime = 0;
                _lastJumpPressedTime = 0;

                // Calculate jump force
                float force = jumpImpulse;

                // Add extra force if falling to ensure consistent jump height
                if (rb.linearVelocity.y < 0)
                    force -= rb.linearVelocity.y;

                // Apply jump force
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset Y velocity
                rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

                _isJumping = true;
                _jumpCut = false;
                _inJumpBoostPhase = true;
                _jumpStartTime = Time.time;

                // Trigger jump animation
                animator.SetTrigger(AnimationStrings.JumpTrigger);
            }

            // Apply variable gravity
            UpdateGravity();

            // Update animation parameters
            animator.SetFloat(AnimationStrings.YVelocity, rb.linearVelocity.y);
        }


        private void UpdateGravity()
        {
            // Skip gravity adjustments when dashing
            if (IsDashing) return;

            // Check if initial jump boost phase is active
            if (_inJumpBoostPhase)
            {
                if (Time.time - _jumpStartTime > initialJumpBoostDuration)
                {
                    _inJumpBoostPhase = false;
                }
                else
                {
                    // Apply reduced gravity during initial boost phase
                    rb.gravityScale = _defaultGravityScale * 0.03f;
                    return;
                }
            }

            if (touchingDirections.IsGrounded && !_isJumping)
            {
                // Normal gravity on ground
                rb.gravityScale = _defaultGravityScale;
            }
            else if (rb.linearVelocity.y < 0)
            {
                // Increased gravity when falling
                if (_moveInput.y < 0)
                {
                    // Fast fall when holding down
                    rb.gravityScale = _defaultGravityScale * fastFallGravityMult;
                }
                else
                {
                    // Normal fall
                    rb.gravityScale = _defaultGravityScale * fallGravityMult;
                }
            }
            else if (_jumpCut)
            {
                // Increased gravity when jump button released early
                rb.gravityScale = _defaultGravityScale * jumpCutMultiplier;
            }
            else if (_isJumping && Mathf.Abs(rb.linearVelocity.y) < jumpHangTimeThreshold)
            {
                // Reduced gravity at apex of jump for better feel
                rb.gravityScale = _defaultGravityScale * jumpHangGravityMult;
            }
            else
            {
                // Default gravity otherwise
                rb.gravityScale = _defaultGravityScale;
            }
        }

        private void FixedUpdate()
        {
            if (!IsDodging && _moveInput != Vector2.zero)
            {
                SetFacingDirection(_moveInput);
            }

            if (!CanMove)
                return;

            float targetSpeed = _moveInput.x * walkSpeed;
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? Acceleration : Deceleration;

            // Higher deceleration when changing directions
            if (!Mathf.Approximately(Mathf.Sign(targetSpeed), Mathf.Sign(rb.linearVelocity.x)) &&
                Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                rate = DirectionChangeDeceleration;
            }

            // Slower deceleration during attacks
            if (IsAttacking)
                rate = AttackingDeceleration;

            // Apply jump hang time acceleration boost
            if (_isJumping && Mathf.Abs(rb.linearVelocity.y) < jumpHangTimeThreshold)
            {
                rate *= jumpHangAccelerationMult;
                targetSpeed *= 1.1f; // Slight speed boost at apex
            }

            float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);

            // Apply movement dead zone
            if (Mathf.Abs(newSpeed) < 0.1f)
                newSpeed = 0f;

            rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
            IsMoving = IsAlive && _moveInput != Vector2.zero;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            // Jump button pressed
            if (context.performed)
            {
                _lastJumpPressedTime = jumpBufferTime;
            }
            // Jump button released - enable jump cut for variable height
            else if (context.canceled && _isJumping && rb.linearVelocity.y > 0)
            {
                _jumpCut = true;
            }
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (combatManager != null)
            {
                combatManager.OnAttack(context);
            }
        }

        public void LockMovementDuringAttack(int shouldLock)
        {
            animator.SetBool(AnimationStrings.LockVelocity, shouldLock == 1);
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            if (combatManager != null)
            {
                combatManager.OnDodge(context);
            }
        }

        public void OnHit(float damage, Vector2 knockback)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        public void OnHeal(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                characterStats.Heal(30f);
            }
        }

        private void SetFacingDirection(Vector2 controllerInput)
        {
            if (CanMove && !IsDodging && !IsAttacking)
            {
                if (controllerInput.x > 0 && !IsFacingRight)
                {
                    Turn();
                }
                else if (controllerInput.x < 0 && IsFacingRight)
                {
                    Turn();
                }
            }
        }

        private void Turn()
        {
            // Flip the facing direction bool first
            isFacingRight = !isFacingRight;

            // Update player rotation
            if (!isFacingRight)
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
            }
            else
            {
                Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                transform.rotation = Quaternion.Euler(rotator);
            }

            // Tell the camera follow object to update its direction
            _cameraFollowObject.CallTurn();
        }
    }
}
