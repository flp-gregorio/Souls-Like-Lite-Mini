using UnityEngine;
using System.Globalization;
using TMPro;

public class UiManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;

    public Canvas gameCanvas;

    void Awake()
    {
        gameCanvas = FindFirstObjectByType<Canvas>();
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += CharacterTookDamage;
        CharacterEvents.characterHealed += CharacterHealed;
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= CharacterTookDamage;
        CharacterEvents.characterHealed -= CharacterHealed;
    }

    void CharacterTookDamage(GameObject character, float damageReceived)
    {
        // Create text at character hit
        if (!Camera.main)
            return;
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
            .GetComponent<TMP_Text>();
        tmpText.text = damageReceived.ToString(CultureInfo.InvariantCulture);
    }

     void CharacterHealed(GameObject character, float healthRestored)
     {
         if (!Camera.main)
             return;
         Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

         TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
             .GetComponent<TMP_Text>();
         tmpText.text = healthRestored.ToString(CultureInfo.CurrentCulture);
     }
}
