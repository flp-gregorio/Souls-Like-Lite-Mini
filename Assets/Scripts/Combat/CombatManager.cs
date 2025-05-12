using System.Collections;
using Animation;
using Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Combat
{
    [RequireComponent(typeof(CharacterStats))]
    public class CombatManager : MonoBehaviour
    {
        static public CombatManager instance;

        [SerializeField]
        private bool debugging = false; // Toggle this in the inspector

        public bool canReceiveInput = true;
        public bool inputReceived = false;

        private CharacterStats _stats;
        private Animator _animator;
        private PlayerController _player;
        private Rigidbody2D _rb;

        private bool _inAttackCombo = false; // Flag to track if we're in a combo

        void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _animator = GetComponent<Animator>();
            _player = GetComponent<PlayerController>();
            _rb = GetComponent<Rigidbody2D>();
            instance = this;
            canReceiveInput = true;
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            if (debugging)
                Debug.Log($"Attack input received. canReceiveInput: {canReceiveInput}, inAttackCombo: {_inAttackCombo}");

            if (!canReceiveInput || !_stats.TryUseStamina(20f))
                return;

            if (debugging)
                Debug.Log("Attack input valid, processing attack");

            inputReceived = true;

            if (!_inAttackCombo)
            {
                _animator.SetTrigger(AnimationStrings.AttackTrigger);
                _inAttackCombo = true;
            }
            else
            {
                _animator.SetTrigger(AnimationStrings.Attack2Trigger);
                _inAttackCombo = false;
            }

            canReceiveInput = false;
            inputReceived = false;
        }

        public bool OnDodge(InputAction.CallbackContext context)
        {
            if (!context.started)
                return false;

            if (_player.touchingDirections.IsGrounded && !_player.IsDodging)
            {
                if (_stats.TryUseStamina(15f))
                {
                    _animator.SetTrigger(AnimationStrings.DodgeTrigger);
                    _animator.SetBool(AnimationStrings.IsDodging, true);
                    _animator.SetBool(AnimationStrings.CanMove, false);
                    StartCoroutine(DodgeCoroutine());
                    return true;
                }
            }
            else if (!_player.touchingDirections.IsGrounded && !_player.IsDashing &&
                     _animator.GetBool(AnimationStrings.CanDash))
            {
                if (_stats.TryUseStamina(15f))
                {
                    _animator.SetTrigger(AnimationStrings.DashTrigger);
                    _animator.SetBool(AnimationStrings.IsDashing, true);
                    _animator.SetBool(AnimationStrings.CanMove, false);
                    StartCoroutine(DashCoroutine());
                    return true;
                }
            }

            return false;
        }

        private IEnumerator DodgeCoroutine()
        {
            float direction = _player.IsFacingRight ? 1f : -1f;
            float timer = 0f;

            while (timer < _player.dodgeDuration)
            {
                Vector2 dodgeVector = new Vector2(direction * _player.dodgeSpeed, 0);
                _rb.linearVelocity = new Vector2(dodgeVector.x, 0);
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            _animator.SetBool(AnimationStrings.IsDodging, false);
            _animator.SetBool(AnimationStrings.CanMove, true);
        }

        private IEnumerator DashCoroutine()
        {
            float direction = _player.IsFacingRight ? 1f : -1f;
            float timer = 0f;

            while (timer < _player.dashDuration)
            {
                Vector2 dashVector = new Vector2(direction * _player.dashSpeed, 0);
                _rb.linearVelocity = new Vector2(dashVector.x, 0);
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            _animator.SetBool(AnimationStrings.IsDashing, false);
            _animator.SetBool(AnimationStrings.CanMove, true);
        }

        public void HandleCombatState(string currentState)
        {
            if (debugging)
                Debug.Log($"HandleCombatState: {currentState}, inputReceived: {inputReceived}");

            switch (currentState)
            {
                case "Idle":
                    canReceiveInput = true;
                    _inAttackCombo = false;
                    _stats.SetStaminaRegeneration(true);
                    break;

                case "Walk":
                    canReceiveInput = true;
                    _inAttackCombo = false;
                    _stats.SetStaminaRegeneration(true);
                    break;

                case "Attack1":
                    canReceiveInput = true;
                    _stats.SetStaminaRegeneration(false);
                    break;

                case "Attack2":
                    _stats.SetStaminaRegeneration(false);
                    // Optionally handle specific logic for Attack2 here
                    break;

                case "Transition":
                    _stats.SetStaminaRegeneration(false);
                    // Can receive input during transition
                    canReceiveInput = true;
                    break;

                case "Dodge":
                    _stats.SetStaminaRegeneration(false);
                    break;
                case "Dash":
                    _stats.SetStaminaRegeneration(false);
                    break;
            }
        }

        public void ToggleInputReception(bool canReceive)
        {
            canReceiveInput = canReceive;
            if (debugging)
                Debug.Log($"Input reception set to: {canReceiveInput}");
        }
    }
}