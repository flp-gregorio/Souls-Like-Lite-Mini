using UnityEngine;
using UnityEngine.Events;

public class CharacterEvents
{
    // Character damaged and damage value
    public static UnityAction<GameObject, float> characterDamaged;

    // Character healed and amount healed
    public static UnityAction<GameObject, float> characterHealed;
}
