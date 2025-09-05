using UnityEngine;
using System.Collections.Generic;

public class SkillExecutor : MonoBehaviour
{
    private Dictionary<SkillInstance, float> lastUsedTimeDict = new();
    [SerializeField] private GameObject commonEffectPrefab;

    public bool UseSkill(SkillInstance skillInstance, Vector2 direction)
    {
        if (skillInstance == null) return false;

        float lastUsed;
        if (lastUsedTimeDict.TryGetValue(skillInstance, out lastUsed))
        {
            if (Time.time < lastUsed + skillInstance.cooldown)
            {
                // Debug.Log(skillData.name + "스킬 쿨타임 중");
                return false;
            }
        }

        lastUsedTimeDict[skillInstance] = Time.time;

        Vector2 dir = direction.normalized;

        switch (skillInstance)
        {
            case MeleeSkillInstance melee:
                ExecuteMeleeSkill(melee, dir);
                break;

            case ProjectileSkillInstance proj:
                ExecuteProjectileSkill(proj, dir);
                break;

            default:
                Debug.LogWarning("지원하지 않는 스킬 타입입니다.");
                break;
        }
        return true;
    }

    private void ExecuteMeleeSkill(MeleeSkillInstance skill, Vector2 dir)
    {
        Vector3 spawnPos = transform.position + (Vector3)(dir * (skill.distanceFromUser + skill.width * 0.5f));
        //히트박스 생성 위치 계산(플레이어로부터 거리)
        GameObject hitbox = Instantiate(skill.hitboxPrefab, spawnPos, Quaternion.identity);

        SkillHitbox hitboxComponent = hitbox.GetComponent<SkillHitbox>();
        if (hitboxComponent != null)
        {
            hitboxComponent.Initialize(gameObject, skill, dir);
        }
        CreateSkillEffect(skill.effectAnimator, skill.effectDuration, spawnPos, dir, skill.RotateEffect, skill.FlipSpriteY);
    }

    private void ExecuteProjectileSkill(ProjectileSkillInstance skill, Vector2 dir)
    {
        Vector3 spawnPos = transform.position + (Vector3)(dir * skill.distanceFromUser);
        GameObject projectile = Instantiate(skill.projectilePrefab, spawnPos, Quaternion.identity);

        Projectile projComp = projectile.GetComponent<Projectile>();
        if (projComp != null)
        {
            projComp.Initialize(gameObject, skill, dir);
        }
        CreateSkillEffect(skill.effectAnimator, skill.effectDuration, spawnPos, dir, skill.RotateEffect, skill.FlipSpriteY);
    }

    private void CreateSkillEffect(RuntimeAnimatorController animator, float duration, Vector2 pos, Vector2 dir, bool rotate, bool flipY) //스킬 이펙트 재생하기
    {
        if (commonEffectPrefab == null || animator == null) return;

        GameObject effect = Instantiate(commonEffectPrefab, pos, Quaternion.identity);
        if (effect.TryGetComponent(out SkillVFXController vfx))
        {
            vfx.applyRotation = rotate;
            vfx.Initialize(dir, duration, animator, flipY);
        }
    }
}