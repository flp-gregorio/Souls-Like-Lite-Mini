using UnityEngine;

namespace Combat
{
    public class Attack : MonoBehaviour
    {
        public float attackDamage = 10f;
        public Vector2 knockback = Vector2.zero;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // See if it can be damaged
            CharacterStats damageable = collision.gameObject.GetComponent<CharacterStats>();
            if (damageable != null)
            {
                Vector2 deliveredKnockback =
                    transform.parent.localScale.x > 0 ? knockback : new Vector2(-knockback.x, knockback.y);
                // Damage it
                damageable.Hit(attackDamage, deliveredKnockback);
            }
        }
    }
}
