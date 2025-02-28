
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Stamina))]

public class CombatManager : MonoBehaviour
{
    static public CombatManager Instance;
    
    public bool canReceiveInput;
    public bool inputReceived;
    
    Stamina _stamina;
    Animator _animator;

    void Awake()
    {
        _stamina = GetComponent<Stamina>();
        _animator = GetComponent<Animator>();
        Instance = this;
    }
    
    /*
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (canReceiveInput && _stamina.TryUseStamina(20f))
            {
                inputReceived = true;
                canReceiveInput = false;
            }
            else
            {
                return;
            }
        }
    }
    
    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.started && _stamina.TryUseStamina(15f))
        {
            PlayerController player = GetComponent<PlayerController>();
            if (player != null && player.touchingDirections.IsGrounded && !player.IsDodging)
            {
                player.animator.SetTrigger(AnimationStrings.DodgeTrigger);
                player.animator.SetBool("IsDodging", true);
                player.animator.SetBool("CanMove", false);
            }
        }
    }


    public void InputManager()
    {
        canReceiveInput = !canReceiveInput;
    }
}
