using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(Damageable))]
public class Goblin : MonoBehaviour
{
    public float walkAcceleration = 30f;
    public float maxSpeed = 3f;
    public float walkStopRate = 0.05f;
    public DetectionZone attackZone;
    public DetectionZone cliffDetectionZone;
    public Transform player;
    public float chaseDistance = 5f;
    public float initialPatrolTimerValue = 5f;
    public float patrolTimer = 5f;

    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Animator animator;
    private Damageable damageable;

    public enum WalkableDirection
    {
        Left,
        Right
    }

    private WalkableDirection _walkDirection;
    private Vector2 walkDirectionVector = Vector2.right;

    public WalkableDirection WalkDirection
    {
        get => _walkDirection;
        set
        {
            if (_walkDirection != value)
            {
                transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                walkDirectionVector = value == WalkableDirection.Right ? Vector2.left : Vector2.right;
            }
            _walkDirection = value;
        }
    }

    private bool _hasTarget = false;
    public bool HasTarget
    {
        get => _hasTarget;
        private set
        {
            _hasTarget = value;
            animator?.SetBool(AnimationStrings.HasTarget, value);
        }
    }

    public bool CanMove => animator?.GetBool(AnimationStrings.CanMove) ?? false;

    public float AttackCooldown
    {
        get => animator?.GetFloat(AnimationStrings.AttackCooldown) ?? 0f;
        private set
        {
            if (animator)
                animator.SetFloat(AnimationStrings.AttackCooldown, Mathf.Max(value, 0));
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        animator = GetComponent<Animator>();
        damageable = GetComponent<Damageable>();
    }

    private void Update()
    {
        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
            HasTarget = false;
        }
        else
        {
            HasTarget = attackZone.detectedColliders.Count > 0;
        }
    }

    private void FixedUpdate()
    {
        // Determine if Goblin should chase the player
        bool shouldChase = player != null &&
                           Vector2.Distance(player.position, rb.position) <= chaseDistance;
        animator.SetBool(AnimationStrings.IsChasing, shouldChase);

        // Always perform obstacle detection
        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall && CanMove)
        {
            FlipDirection();
        }

        // State-specific logic
        if (shouldChase)
        {
            // Update direction based on player position
            float direction = Mathf.Sign(player.position.x - rb.position.x);
            WalkDirection = (direction > 0) ? WalkableDirection.Left : WalkableDirection.Right;
        }
        else
        {
            // Update patrol state (e.g., timer)
            patrolTimer -= Time.fixedDeltaTime;
            if (patrolTimer <= 0)
            {
                FlipDirection();
                patrolTimer = initialPatrolTimerValue; // Reset timer as needed
            }
        }

        // Movement logic
        if (!damageable.LockVelocity)
        {
            if (CanMove)
            {
                rb.linearVelocity = new Vector2(
                    Mathf.Clamp(rb.linearVelocity.x + (walkAcceleration * walkDirectionVector.x * Time.fixedDeltaTime),
                        -maxSpeed, maxSpeed),
                    rb.linearVelocity.y
                );
            }
            else
            {
                rb.linearVelocity = new Vector2(
                    Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate),
                    rb.linearVelocity.y
                );
            }
        }
    }


    private void FlipDirection()
    {
        WalkDirection = WalkDirection == WalkableDirection.Left
            ? WalkableDirection.Right
            : WalkableDirection.Left;
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);
        if (!HasTarget)
        {
            FlipDirection();
        }
    }

    public void OnCliffDetected()
    {
        if (touchingDirections.IsGrounded)
            FlipDirection();
    }
}
