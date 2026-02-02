using System.Collections.Generic;
using UnityEngine;

public class SkillInstance
{
    // 원본 데이터
    public SkillData data;
    public bool RotateEffect => data.rotateSkill;
    public bool FlipSpriteY => data.flipSpriteY;
    public SkillSpawnPointType SpawnPointType => data.spawnPointType;

    // 기본 스탯 (캐싱)
    public float cooldown;
    public Vector2 spawnOffset;
    public float delay;
    public float postDelay;

    // 런타임 캐싱 오브젝트
    public GameObject spawnedHitbox;
    public GameObject spawnedProjectile;
    public GameObject spawnedEffect;

    // 모듈 인스턴스들
    private readonly List<ISkillModule> modules = new();

    public SkillInstance(SkillData data)
    {
        this.data = data;

        cooldown = data.cooldown;
        spawnOffset = data.spawnOffset;
        delay = data.delay;
        postDelay = data.postDelay;

        // 모듈 인스턴스 생성 (핵심)
        if (data.modules != null)
        {
            foreach (var moduleData in data.modules)
            {
                if (moduleData == null) continue;

                var module = moduleData.CreateModule(this);
                if (module != null)
                    modules.Add(module);
            }
        }
    }

    public void Delay(GameObject attacker)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnDelay(attacker);
    }

    public void Execute(GameObject attacker, Vector2 direction)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnExecute(attacker, direction);
    }

    public void OnHit(GameObject attacker, GameObject target)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnHit(attacker, target);
    }

    public void OnTick(GameObject attacker)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnTick(attacker);
    }

    public void OnExpire(GameObject attacker)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnExpire(attacker);
    }

    public void PostDelay(GameObject attacker)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnPostDelay(attacker);
    }

    public void ApplyUpgrade(WeaponUpgradeInfo UpgradeInfo)
    {
        
    }
}
