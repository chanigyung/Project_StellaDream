using System.Collections.Generic;
using UnityEngine;

public class SkillHitbox : SkillObjectBase
{
    private readonly HashSet<GameObject> alreadyHit = new();

    protected override void OnInitialize()
    {
        alreadyHit.Clear();

        if (skill != null && skill.RotateEffect)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (direction.x < 0 && skill.FlipSpriteY)
            {
                Vector3 scale = transform.localScale;
                scale.y *= -1;
                transform.localScale = scale;
            }
        }
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
