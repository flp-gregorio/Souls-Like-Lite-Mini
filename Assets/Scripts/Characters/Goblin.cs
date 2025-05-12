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
        private float _lastFlipTime;

        private Rigidbody2D _rb;
        private TouchingDirections _touchingDirections;
        private Animator _animator;
        private CharacterStats _characterStats;

        private Coroutine _patrolCoroutine;
        private bool _isPatrolling = false;

        // Using a single float for direction: 1 = right, -1 = left.
        private float _moveDirection = 1f;

        // Cached animator parameter hashes (requires AnimationStrings to define these names)
        private static readonly int IsMovingHash = Animator.StringToHash(AnimationStrings.IsMoving);
        private static readonly int HasTargetHash = Animator.StringToHash(AnimationStrings.HasTarget);
        private static readonly int CanMoveHash = Animator.StringToHash(AnimationStrings.CanMove);
        private static readonly int AttackCooldownHash = Animator.StringToHash(AnimationStrings.AttackCooldown);
        private int _hitCount = 0; // Tracks how many times the Goblin has been hit
        const float BaseAttackChance = 0.1f; // Initial chance to attack (10%)
        const float AttackChanceIncreasePerHit = 0.1f; // Chance increment per hit (10%)

        private static readonly int TryAttack1 = Animator.StringToHash("TryAttack");
        private bool _isInAttackCooldown = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _touchingDirections = GetComponent<TouchingDirections>();
            _animator = GetComponent<Animator>();
            _characterStats = GetComponent<CharacterStats>();
        }

        private void Start()
        {
            StartPatrolling();
        }

        private void Update()
        {
            // Update attack cooldown and target detection.
            float attackCooldown = _animator.GetFloat(AttackCooldownHash);
            if (attackCooldown > 0)
            {
                _animator.SetFloat(AttackCooldownHash, Mathf.Max(attackCooldown - Time.deltaTime, 0f));
                _animator.SetBool(HasTargetHash, false);
                _isInAttackCooldown = true;
            }
            else
            {
                _animator.SetBool(HasTargetHash, attackZone.detectedColliders.Count > 0);
                _isInAttackCooldown = false;
            }

            // Set animator "IsMoving" based on horizontal velocity.
            _animator.SetBool(IsMovingHash, Mathf.Abs(_rb.linearVelocity.x) > 0.01f);
        }

        private void FixedUpdate()
        {
            bool shouldChase = player && Vector2.Distance(player.position, _rb.position) <= chaseDistance;
            float distanceToPlayer = player ? Vector2.Distance(player.position, _rb.position) : float.MaxValue;
            
            // Flip if the goblin is grounded but stuck on a wall.
            if (_touchingDirections.IsGrounded && _touchingDirections.IsOnWall && CanMove())
            {
                FlipDirection();
            }

            // Determine if we should stop moving due to being in attack cooldown and close to player
            bool shouldStopForCooldown = _isInAttackCooldown && distanceToPlayer <= attackIdleDistance;

            if (shouldStopForCooldown)
            {
                // Stay in place but face the player
                StopPatrolling();
                _moveDirection = (player.position.x - _rb.position.x) > 0 ? 1f : -1f;
                UpdateSpriteDirection();
                SetVelocityToZero();
            }
            else if (shouldChase)
            {
                StopPatrolling();
                // Face toward the player.
                _moveDirection = (player.position.x - _rb.position.x) > 0 ? 1f : -1f;
                UpdateSpriteDirection();
                
                // Only move if not in cooldown or far enough from player
                UpdateMovement();
            }
            else if (!_isPatrolling)
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
            if (!_characterStats.LockVelocity)
            {
                Vector2 velocity = _rb.linearVelocity;
                velocity.x = Mathf.Lerp(velocity.x, 0, walkStopRate * 3); // Faster stop rate
                _rb.linearVelocity = velocity;
            }
        }
        
        private void UpdateMovement()
        {
            // Only update velocity if not locked.
            if (!_characterStats.LockVelocity)
            {
                Vector2 velocity = _rb.linearVelocity;
                if (CanMove())
                {
                    velocity.x = Mathf.Clamp(
                        velocity.x + walkAcceleration * _moveDirection * Time.fixedDeltaTime,
                        -maxSpeed,
                        maxSpeed);
                }
                else
                {
                    velocity.x = Mathf.Lerp(velocity.x, 0, walkStopRate);
                }
                _rb.linearVelocity = velocity;
            }
        }

        private bool CanMove()
        {
            return _animator.GetBool(CanMoveHash);
        }

        private void StartPatrolling()
        {
            if (!_isPatrolling)
            {
                _isPatrolling = true;
                _patrolCoroutine = StartCoroutine(PatrolRoutine());
            }
        }

        private void StopPatrolling()
        {
            if (_isPatrolling && _patrolCoroutine != null)
            {
                StopCoroutine(_patrolCoroutine);
                _isPatrolling = false;
            }
        }

        private IEnumerator PatrolRoutine()
        {
            // Simplified patrol: move for a period, then stop, then flip.
            while (true)
            {
                _animator.SetBool(IsMovingHash, true);
                yield return new WaitForSeconds(patrolDuration);
                _animator.SetBool(IsMovingHash, false);
                yield return new WaitForSeconds(patrolDuration);
                FlipDirection();
            }
        }

        private void FlipDirection()
        {
            if (Time.time - _lastFlipTime < FlipCooldown)
                return;

            _lastFlipTime = Time.time;
            _moveDirection *= -1;
            UpdateSpriteDirection();
        }

        private void UpdateSpriteDirection()
        {
            // Flip sprite horizontally by adjusting the local scale.
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (_moveDirection > 0 ? 1 : -1);
            transform.localScale = scale;
        }

        public void OnHit(float damage, Vector2 knockback)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(knockback, ForceMode2D.Impulse);

            // Increment hit count and calculate attack chance
            _hitCount++;
            float currentAttackChance = BaseAttackChance + (_hitCount * AttackChanceIncreasePerHit);

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
            _animator.SetTrigger(TryAttack1);
        }

        public void OnCliffDetected()
        {
            if (_touchingDirections.IsGrounded)
            {
                //Debug.Log("Flip due to cliff");
                FlipDirection();
            }
        }

        private void OnDisable()
        {
            StopPatrolling();
        }
    }
}