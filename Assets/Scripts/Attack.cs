using UnityEngine;

public class Attack : MonoBehaviour
{
    public float AttackDamage = 10f;
    public Vector2 knockback = Vector2.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // See if it can be damaged
        Damageable damageable = collision.gameObject.GetComponent<Damageable>();
        if (damageable != null)
        {
            Vector2 deliveredKnockback = transform.parent.localScale.x > 0 ? knockback : new Vector2(-knockback.x, knockback.y);
            // Damage it
            damageable.Hit(AttackDamage, deliveredKnockback);
        }
    }
}
