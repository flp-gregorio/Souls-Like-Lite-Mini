using System.Collections;
using Animation;
using Combat;
using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(Rigidbody2D), typeof(TouchingDirections), typeof(CharacterStats))]
    public class Goblin : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkAcceleration = 30f;
        public float maxSpeed = 3f;
        public float walkStopRate = 0.05f;

        [Header("Detection & Chase")]
        public DetectionZone attackZone;
        public DetectionZone cliffDetectionZone;
        public Transform player;
        public float chaseDistance = 5f;
        public float patrolDuration = 4f;
        public float attackIdleDistance = 2f; // Distance at which to stay idle when in attack cooldown

        private const float FlipCooldown = 0.2f;
        private float lastFlipTime;

        private Rigidbody2D rb;
        private TouchingDirections touchingDirections;
        private Animator animator;
        private CharacterStats characterStats;

        private Coroutine patrolCoroutine;
        private bool isPatrolling = false;

        // Using a single float for direction: 1 = right, -1 = left.
        private float moveDirection = 1f;

        // Cached animator parameter hashes (requires AnimationStrings to define these names)
        private static readonly int IsMovingHash = Animator.StringToHash(AnimationStrings.IsMoving);
        private static readonly int HasTargetHash = Animator.StringToHash(AnimationStrings.HasTarget);
        private static readonly int CanMoveHash = Animator.StringToHash(AnimationStrings.CanMove);
        private static readonly int AttackCooldownHash = Animator.StringToHash(AnimationStrings.AttackCooldown);
        private int hitCount = 0; // Tracks how many times the Goblin has been hit
        const float BaseAttackChance = 0.1f; // Initial chance to attack (10%)
        const float AttackChanceIncreasePerHit = 0.1f; // Chance increment per hit (10%)

        private static readonly int TryAttack1 = Animator.StringToHash("TryAttack");
        private bool isInAttackCooldown = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            touchingDirections = GetComponent<TouchingDirections>();
            animator = GetComponent<Animator>();
            characterStats = GetComponent<CharacterStats>();
        }

        private void Start()
        {
            StartPatrolling();
        }

        private void Update()
        {
            // Update attack cooldown and target detection.
            float attackCooldown = animator.GetFloat(AttackCooldownHash);
            if (attackCooldown > 0)
            {
                animator.SetFloat(AttackCooldownHash, Mathf.Max(attackCooldown - Time.deltaTime, 0f));
                animator.SetBool(HasTargetHash, false);
                isInAttackCooldown = true;
            }
            else
            {
                animator.SetBool(HasTargetHash, attackZone.detectedColliders.Count > 0);
                isInAttackCooldown = false;
            }

            // Set animator "IsMoving" based on horizontal velocity.
            animator.SetBool(IsMovingHash, Mathf.Abs(rb.linearVelocity.x) > 0.01f);
        }

        private void FixedUpdate()
        {
            bool shouldChase = player && Vector2.Distance(player.position, rb.position) <= chaseDistance;
            float distanceToPlayer = player ? Vector2.Distance(player.position, rb.position) : float.MaxValue;
            
            // Flip if the goblin is grounded but stuck on a wall.
            if (touchingDirections.IsGrounded && touchingDirections.IsOnWall && CanMove())
            {
                FlipDirection();
            }

            // Determine if we should stop moving due to being in attack cooldown and close to player
            bool shouldStopForCooldown = isInAttackCooldown && distanceToPlayer <= attackIdleDistance;

            if (shouldStopForCooldown)
            {
                // Stay in place but face the player
                StopPatrolling();
                moveDirection = (player.position.x - rb.position.x) > 0 ? 1f : -1f;
                UpdateSpriteDirection();
                SetVelocityToZero();
            }
            else if (shouldChase)
            {
                StopPatrolling();
                // Face toward the player.
                moveDirection = (player.position.x - rb.position.x) > 0 ? 1f : -1f;
                UpdateSpriteDirection();
                
                // Only move if not in cooldown or far enough from player
                UpdateMovement();
            }
            else if (!isPatrolling)
            {
                StartPatrolling();
                UpdateMovement();
            }
            else
            {
                UpdateMovement();
            }
        }

        private void SetVelocityToZero()
        {
            if (!characterStats.LockVelocity)
            {
                Vector2 velocity = rb.linearVelocity;
                velocity.x = Mathf.Lerp(velocity.x, 0, walkStopRate * 3); // Faster stop rate
                rb.linearVelocity = velocity;
            }
        }
        
        private void UpdateMovement()
        {
            // Only update velocity if not locked.
            if (!characterStats.LockVelocity)
            {
                Vector2 velocity = rb.linearVelocity;
                if (CanMove())
                {
                    velocity.x = Mathf.Clamp(
                        velocity.x + walkAcceleration * moveDirection * Time.fixedDeltaTime,
                        -maxSpeed,
                        maxSpeed);
                }
                else
                {
                    velocity.x = Mathf.Lerp(velocity.x, 0, walkStopRate);
                }
                rb.linearVelocity = velocity;
            }
        }

        private bool CanMove()
        {
            return animator.GetBool(CanMoveHash);
        }

        private void StartPatrolling()
        {
            if (!isPatrolling)
            {
                isPatrolling = true;
                patrolCoroutine = StartCoroutine(PatrolRoutine());
            }
        }

        private void StopPatrolling()
        {
            if (isPatrolling && patrolCoroutine != null)
            {
                StopCoroutine(patrolCoroutine);
                isPatrolling = false;
            }
        }

        private IEnumerator PatrolRoutine()
        {
            // Simplified patrol: move for a period, then stop, then flip.
            while (true)
            {
                animator.SetBool(IsMovingHash, true);
                yield return new WaitForSeconds(patrolDuration);
                animator.SetBool(IsMovingHash, false);
                yield return new WaitForSeconds(patrolDuration);
                FlipDirection();
            }
        }

        private void FlipDirection()
        {
            if (Time.time - lastFlipTime < FlipCooldown)
                return;

            lastFlipTime = Time.time;
            moveDirection *= -1;
            UpdateSpriteDirection();
        }

        private void UpdateSpriteDirection()
        {
            // Flip sprite horizontally by adjusting the local scale.
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (moveDirection > 0 ? 1 : -1);
            transform.localScale = scale;
        }

        public void OnHit(float damage, Vector2 knockback)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);

            // Increment hit count and calculate attack chance
            hitCount++;
            float currentAttackChance = BaseAttackChance + (hitCount * AttackChanceIncreasePerHit);

            // Attempt to trigger an attack based on the calculated chance
            if (Random.value <= currentAttackChance)
            {
                Debug.Log($"Trying to retaliate, {currentAttackChance}");
                currentAttackChance = 0f;
                TryAttack();
            }
        }
        
        void TryAttack()
        {
            animator.SetTrigger(TryAttack1);
        }

        public void OnCliffDetected()
        {
            if (touchingDirections.IsGrounded)
            {
                Debug.Log("Flip due to cliff");
                FlipDirection();
            }
        }

        private void OnDisable()
        {
            StopPatrolling();
        }
    }
}