using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject attacker;
    private SkillInstance skill;
    private Vector2 direction;
    private float speed;

    private readonly HashSet<GameObject> alreadyHit = new();
    private bool initialized;

    public void Initialize(GameObject attacker,SkillInstance skill,Vector2 direction,float speed,float lifetime)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.direction = direction.normalized;
        this.speed = speed;

        float angle = Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (this.direction.x < 0 && skill.FlipSpriteY)
        {
            Vector3 scale = transform.localScale;
            scale.y *= -1;
            transform.localScale = scale;
        }

        initialized = true;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized) return;

        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return;

        GameObject target = damageable.gameObject;
        if (alreadyHit.Contains(target)) return;

        alreadyHit.Add(target);

        // 히트 처리 위임
        skill.OnHit(attacker, target);

        Destroy(gameObject);
    }
}
