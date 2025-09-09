using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterBodyAttack : MonoBehaviour
{
    [SerializeField] private float damage = 5f;
    private Vector2 knockbackForce = new Vector2(15f, 7f);
    private float hitCooldown = 0.8f;

    private Dictionary<GameObject, float> lastHitTime = new();

    void Start()
    {
        BoxCollider2D parentCollider = transform.parent.GetComponent<BoxCollider2D>();
        BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();

        if (parentCollider != null && triggerCollider != null)
        {
            triggerCollider.size = parentCollider.size;
            triggerCollider.offset = parentCollider.offset;
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        GameObject target = collider.gameObject;

        if (lastHitTime.TryGetValue(target, out float lastTime))
        {
            if (Time.time < lastTime + hitCooldown)
                return; // 아직 쿨타임임
        }
        var other = collider.transform.parent;
        IDamageable damageable = other.GetComponent<IDamageable>();
        IKnockbackable knockbackable = other.GetComponent<IKnockbackable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }

        if (knockbackable != null)
        {
            Vector2 dir = (other.transform.position.x < transform.position.x) ? Vector2.left : Vector2.right;
            knockbackable.ApplyKnockback(new Vector2(knockbackForce.x * dir.x, knockbackForce.y));
            // Debug.Log("x 넉백 벡터 : ( "+knockbackForce.x+", "+ knockbackForce.y+" )");
        }

        lastHitTime[target] = Time.time;
    }
}