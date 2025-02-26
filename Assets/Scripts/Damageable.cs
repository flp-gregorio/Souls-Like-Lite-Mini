using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    public UnityEvent<float, Vector2> damageableHit;
    public UnityEvent<float, float> healthChanged;
    Animator animator;
    
    [SerializeField] private HitStopManager hitStop;

    [SerializeField]
    private float _maxHealth = 100;
    public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = value;
        }
    }

    [SerializeField]
    private float _health = 100;

    public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            healthChanged ?.Invoke(_health, _maxHealth);

            //Death
            if (_health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    [SerializeField]
    private bool _isAlive = true;

    public bool IsAlive
    {
        get
        {
            return _isAlive;
        }
        private set
        {
            _isAlive = value;
            animator.SetBool(AnimationStrings.IsAlive, value);
        }
    }

    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(AnimationStrings.lockVelocity);
        }
        set
        {
            animator.SetBool(AnimationStrings.lockVelocity, value);
        }
    }

    [SerializeField]
    private bool IsInvencible = false;
    private float timeSinceHit;
    public float InvencibilityTime = 0.25f;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (IsInvencible)
        {
            if (timeSinceHit > InvencibilityTime)
            {
                IsInvencible = false;
                timeSinceHit = 0;
            }
            timeSinceHit += Time.deltaTime;
        }
    }

    public bool Hit(float damage, Vector2 knockback)
    {
        if (IsAlive && !IsInvencible)
        {
            hitStop.TriggerHitStop();
            
            Health -= damage;
            IsInvencible = true;

            // Notify other scripts that this object was hit to handle knockback, etc.
            animator.SetTrigger(AnimationStrings.hitTrigger);
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
}
