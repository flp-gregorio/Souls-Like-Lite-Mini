using Combat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider staminaSlider;
    public TMP_Text staminaBarText;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image staminaFill;
    [SerializeField] private Color normalColor = new Color(0.196f, 0.784f, 0.314f); // Green
    [SerializeField] private Color depletedColor = new Color(0.784f, 0.196f, 0.196f); // Red
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f); // Gray
    
    [Header("Animation")]
    [SerializeField] private bool usePulseForLock = true;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseMinAlpha = 0.6f;
    
    private CharacterStats _characterStats;
    private bool _lastIsWaitingForFullRegen = false;
    private bool _lastIsDepleted = false;

    void Start()
    {
        InitializePlayerReference();
        InitializeUIValues();
    }
    
    void Update()
    {
        // No need for update if we don't have stats reference or visual feedback
        if (!_characterStats || !staminaFill) return;
        
        // Check for status changes to update visuals
        bool isWaiting = _characterStats.IsWaitingForFullRegen;
        bool isDepleted = _characterStats.IsStaminaDepleted;
        
        // Update color if status changed
        if (isWaiting != _lastIsWaitingForFullRegen || isDepleted != _lastIsDepleted)
        {
            _lastIsWaitingForFullRegen = isWaiting;
            _lastIsDepleted = isDepleted;
            
            if (isWaiting)
            {
                staminaFill.color = lockedColor;
            }
            else if (isDepleted)
            {
                staminaFill.color = depletedColor;
            }
            else
            {
                staminaFill.color = normalColor;
            }
        }
        
        // Pulse animation when locked
        if (usePulseForLock && isWaiting)
        {
            // Create a pulsing effect to show regenerating but locked
            float alpha = Mathf.Lerp(pulseMinAlpha, 1f, (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2);
            Color currentColor = staminaFill.color;
            staminaFill.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }

    void InitializePlayerReference()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null)
        {
            Debug.LogWarning("Player not found for StaminaBar!");
            return;
        }

        if (!player.TryGetComponent(out _characterStats))
        {
            Debug.LogWarning("Player missing CharacterStats component!");
            return;
        }
        
        // If the slider has an image component, get a reference to it for color changes
        if (staminaFill == null && staminaSlider != null)
        {
            staminaFill = staminaSlider.fillRect.GetComponent<Image>();
        }
    }

    void InitializeUIValues()
    {
        if (_characterStats == null) return;

        // Initialize values immediately
        UpdateStaminaUI(_characterStats.CurrentStamina, _characterStats.MaxStamina);
        
        // Register listener for future changes
        _characterStats.staminaChanged.AddListener(UpdateStaminaUI);
    }

    void OnDisable()
    {
        if (_characterStats != null)
            _characterStats.staminaChanged.RemoveListener(UpdateStaminaUI);
    }

    void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        if (staminaSlider != null)
            staminaSlider.value = currentStamina / maxStamina;
            
        if (staminaBarText != null)
            staminaBarText.text = $"AP:  {Mathf.Floor(currentStamina)}/{maxStamina}";
    }
}