using System.Collections.Generic;
using UnityEngine;

public class SkillHitbox : MonoBehaviour
{
    private GameObject attacker;
    private MeleeSkillInstance skill;
    private Vector2 direction;

    private HashSet<GameObject> alreadyHit = new(); // 중복 타격 방지용 변수

    private bool initialized = false; //히트박스 초기화 전 오류 방어용 코드

    //스킬 히트박스 초기화 함수. SkillExecutor.cs에서 데미지 및 수치 받아옴
    public void Initialize(GameObject attacker, MeleeSkillInstance skill, Vector2 direction)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.direction = direction.normalized;

        if (skill.rotateEffect)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (direction.x < 0)
            {
                Vector3 scale = transform.localScale;
                scale.y *= -1;
                transform.localScale = scale;
            }
        }
        
        if (TryGetComponent(out BoxCollider2D box))
        {
            box.size = new Vector2(skill.width, skill.height);
            box.offset = Vector2.zero;
        }

        Transform visual = transform.Find("SkillVFX");
        if (visual != null)
        {
            visual.localScale = new Vector3(skill.width, skill.height, 1f);
        }

        initialized = true;
        // 제거 타이머
        Destroy(gameObject, 0.2f);
    }

    //데미지 판정
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!initialized) return; //초기화 전에는 충돌무시
        var other = collider.transform.parent;
        if (alreadyHit.Contains(other.gameObject)) return;
        alreadyHit.Add(other.gameObject);

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(skill.damage); //데미지

            if (skill.baseData is MeleeSkillData melee && melee.useBasicKnockback)
            {
                if (other.TryGetComponent<IKnockbackable>(out var knockbackable))
                {
                    float xDir = Mathf.Sign(other.transform.position.x - attacker.transform.position.x);
                    Vector2 knockbackForce = new Vector2(melee.knockbackX * xDir, melee.knockbackY);
                    knockbackable.ApplyKnockback(knockbackForce);
                }
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
    }
}
