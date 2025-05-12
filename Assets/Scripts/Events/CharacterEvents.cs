using UnityEngine;
using UnityEngine.Events;

public class CharacterEvents
{
    // Character damaged and damage value
    static public UnityAction<GameObject, float> characterDamaged;

    // Character healed and amount healed
    static public UnityAction<GameObject, float> characterHealed;
}
