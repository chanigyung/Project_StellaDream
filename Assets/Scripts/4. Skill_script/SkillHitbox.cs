using System.Collections.Generic;
using UnityEngine;

public class SkillHitbox : MonoBehaviour
{
    private GameObject attacker;
    private SkillInstance skill;
    private Vector2 direction;

    private readonly HashSet<GameObject> alreadyHit = new();
    private bool initialized;

    public void Initialize(GameObject attacker, SkillInstance skill, Vector2 direction)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.direction = direction.normalized;

        //기존 사양: 방향 기반 회전
        if (skill.RotateEffect)
        {
            float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (this.direction.x < 0 && skill.FlipSpriteY)
            {
                Vector3 scale = transform.localScale;
                scale.y *= -1;
                transform.localScale = scale;
            }
        }

        initialized = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return;

        GameObject target = damageable.gameObject;
        if (alreadyHit.Contains(target)) return;

        alreadyHit.Add(target);

        skill.OnHit(attacker, target);
    }
}
