using UnityEngine;
using UnityEngine.Events;

public class Stamina : MonoBehaviour
{
    public UnityEvent<float, float> StaminaChanged;

    [SerializeField] private float _maxStamina = 100;
    [SerializeField] private float _staminaRegenRate = 5f;

    private float _currentStamina;
    private bool _isRegenerating = true;

    public float MaxStamina => _maxStamina;
    public float CurrentStamina => _currentStamina;

    void Start() => _currentStamina = _maxStamina;

    void Update()
    {
        if (_isRegenerating && _currentStamina < _maxStamina)
        {
            _currentStamina += _staminaRegenRate * Time.deltaTime;
            StaminaChanged?.Invoke(_currentStamina, _maxStamina);
        }
    }

    public bool TryUseStamina(float amount)
    {
        if (_currentStamina >= amount)
        {
            _currentStamina -= amount;
            StaminaChanged?.Invoke(_currentStamina, _maxStamina);
            return true;
        }
        return false;
    }

    // Call this to toggle regeneration (e.g., during attacks)
    public void SetRegeneration(bool state) => _isRegenerating = state;
}
