using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public TMP_Text healthBarText;
    CharacterStats characterStats;

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

        if (!player.TryGetComponent(out characterStats))
        {
            //Debug.LogError("Player missing Damageable component!");
        }
    }

    void InitializeUIValues()
    {
        if (characterStats == null) return;

        // Initialize values immediately
        UpdateHealthUI(characterStats.Health, characterStats.MaxHealth);
        
        // Register listener for future changes
        characterStats.healthChanged.AddListener(UpdateHealthUI);
    }

    void OnDisable()
    {
        if (characterStats != null)
            characterStats.healthChanged.RemoveListener(UpdateHealthUI);
    }

    void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        healthSlider.value = currentHealth / maxHealth;
        healthBarText.text = $"HP:  {currentHealth}/{maxHealth}";
    }
}