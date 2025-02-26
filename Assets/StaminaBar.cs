using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider staminaSlider;
    public TMP_Text staminaBarText;
    Stamina _playerStamina;

    void Awake()
    {
        InitializePlayerReference();
        InitializeUIValues();
    }

    void InitializePlayerReference()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            //Debug.LogError("Player not found!");
            return;
        }

        if (!player.TryGetComponent(out _playerStamina))
        {
            //Debug.LogError("Player missing Damageable component!");
        }
    }

    void InitializeUIValues()
    {
        if (_playerStamina == null) return;

        // Initialize values immediately
        UpdateStaminaUI(_playerStamina.CurrentStamina, _playerStamina.MaxStamina);
        
        // Register listener for future changes
        _playerStamina.StaminaChanged.AddListener(UpdateStaminaUI);
    }

    void OnDisable()
    {
        if (_playerStamina != null)
            _playerStamina.StaminaChanged.RemoveListener(UpdateStaminaUI);
    }

    void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        staminaSlider.value = currentStamina / maxStamina;
        staminaBarText.text = $"AP:  {Mathf.Floor(currentStamina)}/{maxStamina}";
    }
}
