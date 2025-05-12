using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public TMP_Text healthBarText;
    CharacterStats _characterStats;

    void Start()
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

        if (!player.TryGetComponent(out _characterStats))
        {
            //Debug.LogError("Player missing Damageable component!");
        }
    }

    void InitializeUIValues()
    {
        if (_characterStats == null) return;

        // Initialize values immediately
        UpdateHealthUI(_characterStats.Health, _characterStats.MaxHealth);
        
        // Register listener for future changes
        _characterStats.healthChanged.AddListener(UpdateHealthUI);
    }

    void OnDisable()
    {
        if (_characterStats != null)
            _characterStats.healthChanged.RemoveListener(UpdateHealthUI);
    }

    void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        healthSlider.value = currentHealth / maxHealth;
        healthBarText.text = $"HP:  {currentHealth}/{maxHealth}";
    }
}