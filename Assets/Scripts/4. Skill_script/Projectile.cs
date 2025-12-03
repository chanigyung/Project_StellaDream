using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject attacker;
    private SkillInstance skill;
    private Vector2 direction;

    private HashSet<GameObject> alreadyHit = new();

    private bool initialized = false; //투사체 초기화 전 오류 방어용 코드

    public void Initialize(GameObject attacker, SkillInstance skill, Vector2 direction)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.direction = direction.normalized;

        if (skill is IProjectileInfo info)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            if (direction.x < 0)
            {
                Vector3 scale = transform.localScale;
                scale.y *= -1;
                transform.localScale = scale;
            }

            initialized = true;
            Destroy(gameObject, info.Lifetime);
        }
        
    }

    void Update()
    {
        if (skill is IProjectileInfo info)
        {
            transform.Translate(Vector3.right * info.Speed * Time.deltaTime, Space.Self);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!initialized) return;

        Component damageableComp = collider.GetComponentInParent<IDamageable>() as Component;
        GameObject target = damageableComp?.gameObject;

        if (target == null || alreadyHit.Contains(target)) return;

        alreadyHit.Add(target);
        skill.OnHit(attacker, target);
        Destroy(gameObject);
    }
}
