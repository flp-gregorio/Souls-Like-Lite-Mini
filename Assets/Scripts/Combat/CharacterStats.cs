using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Animation;

namespace Combat
{
    public class CharacterStats : MonoBehaviour
    {
        // Common animator reference
        protected Animator Animator;

        [Header("Health")]
        // Health-related properties and events
        public UnityEvent<float, Vector2> damageableHit;
        public UnityEvent<float, float> healthChanged;

        [SerializeField]
        private HitStopManager hitStopManager;
        [SerializeField]
        private float maxHealth = 100;
        [SerializeField]
        private float health = 100;
        [SerializeField]
        private bool isAlive = true;
        [SerializeField]
        private bool isInvincible = false;
        [SerializeField]
        public float invincibilityTime = 0.25f;
        private float _timeSinceHit;

        [Header("Stamina")]
        // Stamina-related properties and events
        public UnityEvent<float, float> staminaChanged;
        [SerializeField]
        private float maxStamina = 100;
        [SerializeField]
        private float normalRegenRate = 15f;  // Renamed from staminaRegenRate
        [SerializeField]
        private float depletedRegenRate = 5f; // New slower regen rate
        private float _currentStamina;
        private bool _isRegenerating = true;
        [Header("Stamina Display")]
        [SerializeField]
        private float staminaDisplaySpeed = 2f;
        private float _displayStamina;
        private Coroutine _staminaDisplayCoroutine;
        
        [Header("Stamina Regeneration Control")]
        [SerializeField] 
        private float criticalStaminaThreshold = 1f; // When stamina is below this value, lock abilities until full
        [SerializeField]
        private bool requireFullRegenAfterDepletion = true; // If true, wait for full stamina before using abilities
        [SerializeField] 
        private float regenDelayAfterUse = 1f; // Delay before stamina starts regenerating
        [SerializeField]
        private bool debugStamina = false;
        
        private float _timeUntilRegenStarts = 0f;
        private bool _regenBlocked = false;
        private bool _waitingForFullRegen = false;

        #region Properties

        public float MaxHealth { get => maxHealth; set => maxHealth = value; }

        public float Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp(value, 0, maxHealth);
                healthChanged?.Invoke(health, maxHealth);

                //Death
                if (health <= 0)
                {
                    IsAlive = false;
                }
            }
        }

        public bool IsAlive
        {
            get => isAlive;
            private set
            {
                isAlive = value;
                Animator.SetBool(AnimationStrings.IsAlive, value);
            }
        }

        public bool LockVelocity
        {
            get => Animator.GetBool(AnimationStrings.LockVelocity);
            set => Animator.SetBool(AnimationStrings.LockVelocity, value);
        }

        public float MaxStamina => maxStamina;
        public float CurrentStamina => _currentStamina;
        
        // Properties to check stamina state
        public bool IsRegenerating => _isRegenerating && !_regenBlocked;
        public bool IsWaitingForFullRegen => _waitingForFullRegen;
        public bool IsStaminaDepleted => _currentStamina <= criticalStaminaThreshold;

        #endregion

        void Awake()
        {
            Animator = GetComponent<Animator>();
            _currentStamina = maxStamina;
            _displayStamina = maxStamina;
            _waitingForFullRegen = false;
        }

        void Update()
        {
            // Handle invincibility
            if (isInvincible)
            {
                if (_timeSinceHit > invincibilityTime)
                {
                    isInvincible = false;
                    _timeSinceHit = 0;
                }
                _timeSinceHit += Time.deltaTime;
            }

            // Handle stamina regeneration
            ManageStaminaRegeneration();
        }
        
        private void ManageStaminaRegeneration()
        {
            // If regeneration is blocked by state
            if (!_isRegenerating)
                return;
                
            // Handle regen timer countdown
            if (_regenBlocked)
            {
                _timeUntilRegenStarts -= Time.deltaTime;
                if (_timeUntilRegenStarts <= 0)
                {
                    _regenBlocked = false;
                    if (debugStamina) Debug.Log("Stamina regeneration resumed");
                }
                return; // Don't regenerate yet
            }
                
            // Actually regenerate stamina
            if (_currentStamina < maxStamina)
            {
                // Use appropriate regen rate based on stamina state
                float regenRate = IsStaminaDepleted ? depletedRegenRate : normalRegenRate;
                
                _currentStamina += regenRate * Time.deltaTime;
                _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
                UpdateDisplayStamina();
                
                // Check if we've fully regenerated
                if (_currentStamina >= maxStamina)
                {
                    _currentStamina = maxStamina;
                    _waitingForFullRegen = false;
                    if (debugStamina && _waitingForFullRegen) Debug.Log("Stamina fully regenerated, abilities available again");
                }
            }
        }

        #region Health Methods

        public bool Hit(float damage, Vector2 knockback)
        {
            if (IsAlive && !isInvincible)
            {
                if (hitStopManager != null)
                    hitStopManager.TriggerHitStop();

                Health -= damage;
                isInvincible = true;

                // Notify other scripts that this object was hit to handle knockback, etc.
                Animator.SetTrigger(AnimationStrings.HitTrigger);
                LockVelocity = true;
                damageableHit?.Invoke(damage, knockback);
                CharacterEvents.characterDamaged.Invoke(gameObject, damage);

                return true;
            }
            return false;
        }

        public bool Heal(float healedValue)
        {
            if (IsAlive)
            {
                Health += healedValue;
                CharacterEvents.characterHealed.Invoke(gameObject, healedValue);
                return true;
            }
            return false;
        }

        #endregion

        #region Stamina Methods

        /// <summary>
        /// Attempt to use stamina immediately - returns true if successful
        /// </summary>
        public bool TryUseStamina(float amount)
        {
            // If we're waiting for full regeneration and haven't reached it yet, block all stamina usage
            if (_waitingForFullRegen)
            {
                if (debugStamina) Debug.Log("Cannot use stamina: waiting for full regeneration");
                return false;
            }

            // Normal stamina check
            if (_currentStamina >= amount)
            {
                // Deduct stamina
                _currentStamina -= amount;
                UpdateDisplayStamina();
                
                // Block regeneration for a moment
                _timeUntilRegenStarts = regenDelayAfterUse;
                _regenBlocked = true;
                
                // Check if we hit critical threshold
                if (_currentStamina <= criticalStaminaThreshold && requireFullRegenAfterDepletion)
                {
                    _waitingForFullRegen = true;
                    if (debugStamina) Debug.Log("Stamina critically low: Must wait for full regeneration");
                }
                
                return true;
            }
            
            return false;
        }

        private void UpdateDisplayStamina()
        {
            if (_staminaDisplayCoroutine != null)
                StopCoroutine(_staminaDisplayCoroutine);

            _staminaDisplayCoroutine = StartCoroutine(SmoothStaminaDisplay());
        }

        private IEnumerator SmoothStaminaDisplay()
        {
            while (Mathf.Abs(_displayStamina - _currentStamina) > 0.1f)
            {
                _displayStamina = Mathf.Lerp(_displayStamina, _currentStamina, staminaDisplaySpeed * Time.deltaTime);
                staminaChanged?.Invoke(_displayStamina, maxStamina);
                yield return null;
            }

            _displayStamina = _currentStamina;
            staminaChanged?.Invoke(_displayStamina, maxStamina);
        }

        /// <summary>
        /// Enables or disables stamina regeneration globally (regardless of block status)
        /// </summary>
        public void SetStaminaRegeneration(bool state)
        {
            _isRegenerating = state;
            
            // If turning on regeneration, reset any block
            if (state)
            {
                _regenBlocked = false;
                _timeUntilRegenStarts = 0f;
            }
        }
        
        /// <summary>
        /// Force stamina regeneration to start immediately, bypassing any delays
        /// </summary>
        public void ForceStartRegeneration()
        {
            _regenBlocked = false;
            _timeUntilRegenStarts = 0f;
            _isRegenerating = true;
        }
        
        /// <summary>
        /// Force stamina regeneration to stop immediately
        /// </summary>
        public void ForceStopRegeneration()
        {
            _regenBlocked = true;
            _timeUntilRegenStarts = float.MaxValue; // Block until explicitly unblocked
        }
        
        /// <summary>
        /// Restore stamina to full and reset all stamina-related flags
        /// </summary>
        public void FullRestoreStamina()
        {
            _currentStamina = maxStamina;
            _displayStamina = maxStamina;
            _waitingForFullRegen = false;
            _regenBlocked = false;
            UpdateDisplayStamina();
        }

        #endregion

        // Method for hit stop that was previously in HitStopManager
        public void TriggerHitStop()
        {
            if (hitStopManager != null)
                hitStopManager.TriggerHitStop();
        }
    }
}