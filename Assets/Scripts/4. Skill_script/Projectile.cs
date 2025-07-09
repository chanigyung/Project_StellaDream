using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject attacker;
    private ProjectileSkillInstance skill;
    private Vector2 direction;

    private HashSet<GameObject> alreadyHit = new();

    private bool initialized = false; //투사체 초기화 전 오류 방어용 코드

    public void Initialize(GameObject attacker, ProjectileSkillInstance skill, Vector2 direction)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.direction = direction.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        if (direction.x < 0)
        {
            transform.localScale = new Vector2(1, -1);
        }

        initialized = true;
        Destroy(gameObject, skill.lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.right * skill.speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!initialized) return;
        var other = collider.transform.parent;
        if (alreadyHit.Contains(other.gameObject)) return;
        alreadyHit.Add(other.gameObject);

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(skill.damage);
            if (skill.baseData is ProjectileSkillData proj && proj.useBasicKnockback)
            {
                if (other.TryGetComponent<IKnockbackable>(out var knockbackable))
                {
                    float xDir = Mathf.Sign(other.transform.position.x - attacker.transform.position.x);
                    Vector2 knockbackForce = new Vector2(proj.knockbackX * xDir, proj.knockbackY);
                    knockbackable.ApplyKnockback(knockbackForce);
                }

                if (other.TryGetComponent<StatusEffectManager>(out var eManager))
                {
                    foreach (var effect in skill.statusEffects)
                    {
                        var instance = StatusEffectFactory.CreateEffectInstance(effect, other.gameObject, attacker, eManager);
                        if (instance != null)
                            eManager.ApplyEffect(instance);
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
