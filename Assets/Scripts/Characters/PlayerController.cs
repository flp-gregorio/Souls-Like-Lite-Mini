using Animation;
using Combat;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public float jumpImpulse = 10f;

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

        private Vector2 moveInput;

        public TouchingDirections touchingDirections;
        public CharacterStats characterStats;
        public Rigidbody2D rb;
        public Animator animator;
        public CombatManager combatManager;

        private bool IsAlive
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
                return isMoving;
            }
            private set
            {
                isMoving = value;
                animator.SetBool(AnimationStrings.IsMoving, value);
            }
        }

        public bool IsFacingRight
        {
            get
            {
                return isFacingRight;
            }
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
        public bool IsDashing
        {
            get
            {
                return animator.GetBool(AnimationStrings.IsDashing);
            }
        }

        bool IsAttacking
        {
            get
            {
                return animator.GetBool(AnimationStrings.IsAttacking);
            }
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
            if (!IsDodging && moveInput != Vector2.zero)
            {
                SetFacingDirection(moveInput);
            }
        }

        private void FixedUpdate()
        {
            if (!CanMove)
                return;

            float targetSpeed = moveInput.x * walkSpeed;
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? Acceleration : Deceleration;

            // Higher deceleration when changing directions
            if (!Mathf.Approximately(Mathf.Sign(targetSpeed), Mathf.Sign(rb.linearVelocity.x)) &&
                Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                rate = DirectionChangeDeceleration;
            }

            if (IsAttacking)
                rate = AttackingDeceleration;

            float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);

            // Apply movement dead zone
            if (Mathf.Abs(newSpeed) < 0.1f)
                newSpeed = 0f;

            rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
            animator.SetFloat(AnimationStrings.YVelocity, rb.linearVelocity.y);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
            IsMoving = IsAlive && moveInput != Vector2.zero;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed && CanMove && touchingDirections.IsGrounded)
            {
                animator.SetTrigger(AnimationStrings.JumpTrigger);
                rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
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
                    IsFacingRight = true;
                }
                else if (controllerInput.x < 0 && IsFacingRight)
                {
                    IsFacingRight = false;
                }
            }
        }
    }
}
