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
        initialized = true;

        if (skill.RotateEffect)
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

        initialized = true;
        // 제거 타이머
        Destroy(gameObject, 0.2f);
    }

    //데미지 판정
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!initialized) return;

        Component damageableComp = collider.GetComponentInParent<IDamageable>() as Component;
        GameObject target = damageableComp?.gameObject;

        if (target == null || alreadyHit.Contains(target)) return;

        alreadyHit.Add(target);
        skill.OnHit(attacker, target);
    }
}
