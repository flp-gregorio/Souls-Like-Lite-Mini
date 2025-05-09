using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider staminaSlider;
    public TMP_Text staminaBarText;
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
            return;
        }
    }

    void InitializeUIValues()
    {
        if (characterStats == null) return;

        // Initialize values immediately
        UpdateStaminaUI(characterStats.CurrentStamina, characterStats.MaxStamina);
        
        // Register listener for future changes
        characterStats.staminaChanged.AddListener(UpdateStaminaUI);
    }

    void OnDisable()
    {
        if (characterStats != null)
            characterStats.staminaChanged.RemoveListener(UpdateStaminaUI);
    }

    void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        staminaSlider.value = currentStamina / maxStamina;
        staminaBarText.text = $"AP:  {Mathf.Floor(currentStamina)}/{maxStamina}";
    }
}
