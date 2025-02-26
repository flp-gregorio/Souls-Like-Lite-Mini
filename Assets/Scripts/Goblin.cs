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

    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Animator animator;
    private Damageable damageable;
    private Rigidbody2D hit;

    public enum WalkableDirection
    {
        Left,
        Right
    }

    private WalkableDirection _walkDirection;
    // The walkDirectionVector is defined relative to the sprite's default orientation.
    private Vector2 walkDirectionVector = Vector2.right;

    public WalkableDirection WalkDirection
    {
        get
        {
            return _walkDirection;
        }
        set
        {
            if (_walkDirection != value)
            {
                // Flip the sprite horizontally by inverting the x-scale.
                transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
                // Due to the sprite's default orientation, the vector is inverted.
                walkDirectionVector = (value == WalkableDirection.Right) ? Vector2.left : Vector2.right;
            }
            _walkDirection = value;
        }
    }

    private bool _hasTarget = false;
    public bool HasTarget
    {
        get
        {
            return _hasTarget;
        }
        private set
        {
            _hasTarget = value;
            // Safely update the animator parameter if it exists.
            animator?.SetBool(AnimationStrings.HasTarget, value);
        }
    }

    public bool CanMove
    {
        get
        {
            return animator?.GetBool(AnimationStrings.CanMove) ?? false;
        }
    }

    public float AttackCooldown
    {
        get
        {
            return animator?.GetFloat(AnimationStrings.AttackCooldown) ?? 0f;
        }
        private set
        {
            if (animator)
            {
                animator.SetFloat(AnimationStrings.AttackCooldown, Mathf.Max(value, 0));
            }
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
        // If the attack cooldown is active, decrease it and ignore the target detection.
        if (AttackCooldown > 0)
        {
            AttackCooldown -= Time.deltaTime;
            // While cooling down, we clear the target flag so that the run animation isn't affected.
            HasTarget = false;

        }
        else
        {
            // Only update target detection if not in attack cooldown.
            HasTarget = attackZone.detectedColliders.Count > 0;
        }
    }

    private void FixedUpdate()
    {
        if (touchingDirections.IsGrounded && touchingDirections.IsOnWall && CanMove)
        {
            FlipDirection();
        }

        if (!damageable.LockVelocity)
        {
            if (CanMove)
            {
                // Apply horizontal acceleration and clamp the speed.
                rb.linearVelocity = new Vector2(Mathf.Clamp(
                    rb.linearVelocity.x + (walkAcceleration * walkDirectionVector.x * Time.fixedDeltaTime),
                    -maxSpeed, maxSpeed), rb.linearVelocity.y);
            }
            else
            {
                // Gradually reduce horizontal speed when movement is disabled.
                rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, walkStopRate), rb.linearVelocity.y);
            }
        }
    }

    private void FlipDirection()
    {
        // Toggle between Left and Right.
        WalkDirection = (WalkDirection == WalkableDirection.Left) ? WalkableDirection.Right : WalkableDirection.Left;
    }

    public void OnHit(float damage, Vector2 knockback)
    {
        // Apply knockback while preserving current vertical velocity.
        rb.linearVelocity = new Vector2(knockback.x, rb.linearVelocity.y + knockback.y);
    }

    public void OnCliffDetected()
    {
        // Flip direction when a cliff is detected, if grounded.
        if (touchingDirections.IsGrounded)
        {
            FlipDirection();
        }
    }
}
