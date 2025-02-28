using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public TMP_Text healthBarText;
    Damageable _playerDamageable;

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

        if (!player.TryGetComponent(out _playerDamageable))
        {
            //Debug.LogError("Player missing Damageable component!");
        }
    }

    void InitializeUIValues()
    {
        if (_playerDamageable == null) return;

        // Initialize values immediately
        UpdateHealthUI(_playerDamageable.Health, _playerDamageable.MaxHealth);
        
        // Register listener for future changes
        _playerDamageable.healthChanged.AddListener(UpdateHealthUI);
    }

    void OnDisable()
    {
        if (_playerDamageable != null)
            _playerDamageable.healthChanged.RemoveListener(UpdateHealthUI);
    }

    void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        healthSlider.value = currentHealth / maxHealth;
        healthBarText.text = $"HP:  {currentHealth}/{maxHealth}";
    }
}