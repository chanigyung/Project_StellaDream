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
    private readonly List<GameObject> spawnedObjectList = new();

    // 모듈 인스턴스들
    private readonly List<ISkillModule> modules = new();

    // 스킬 잠금(선딜레이 등)
    public bool skillLock;
    // 스킬이 사용하는 오브젝트(히트박스, 투사체), 추후 사용할지 여부 결정
    // private readonly List<GameObject> skillObjectList = new();

    public SkillInstance(SkillData data)
    {
        this.data = data;

        cooldown = data.cooldown;
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

    public void Delay(GameObject attacker, Vector2 direction)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnDelay(attacker, direction);
    }

    public void Execute(GameObject attacker, Vector2 direction)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnExecute(attacker, direction);
    }

    public void OnObjectSpawned(GameObject attacker, GameObject sourceObject, Vector2 direction)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnObjectSpawned(attacker, sourceObject, direction);
    }

    public void OnHit(GameObject attacker, GameObject target)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnHit(attacker, target);
    }

    public void OnTick(GameObject attacker, GameObject target, GameObject sourceObject)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnTick(attacker, target, sourceObject);
    }

    public void OnExpire(GameObject attacker, GameObject sourceObject)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnExpire(attacker, sourceObject);
    }

    public void PostDelay(GameObject attacker, Vector2 direction)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnPostDelay(attacker, direction);
    }

    public void ApplyUpgrade(WeaponUpgradeInfo UpgradeInfo)
    {
        
    }

    // ------------ 스킬이 사용하는 오브젝트 등록 및 해제 메서드들(프로토타입) ------------ //
    public void RegisterSpawnedObject(GameObject obj)
    {
        if (obj == null) return;
        CleanupNullSpawnedObjects();
        spawnedObjectList.Add(obj);
    }

    public bool HasSpawnedHitbox() //hitbox존재여부
    {
        CleanupNullSpawnedObjects();

        for (int i = 0; i < spawnedObjectList.Count; i++)
        {
            var obj = spawnedObjectList[i];
            if (obj == null) continue;

            // hitbox 계열 판정(근접/장판)
            if (obj.GetComponent<SkillHitbox>() != null) return true;
            if (obj.GetComponent<AreaHitbox>() != null) return true;
        }

        return false;
    }

    public GameObject FindFirstSpawnedHitboxObject() //hitbox 하나 찾기
    {
        CleanupNullSpawnedObjects();

        for (int i = 0; i < spawnedObjectList.Count; i++)
        {
            var obj = spawnedObjectList[i];
            if (obj == null) continue;

            if (obj.GetComponent<SkillHitbox>() != null) return obj;
            if (obj.GetComponent<AreaHitbox>() != null) return obj;
        }

        return null;
    }

    public bool HasSpawnedProjectile() //투사체 보유 여부
    {
        CleanupNullSpawnedObjects();

        for (int i = 0; i < spawnedObjectList.Count; i++)
        {
            var obj = spawnedObjectList[i];
            if (obj == null) continue;

            if (obj.GetComponent<Projectile>() != null) return true;
        }

        return false;
    }

    public bool HasSpawnedEffect() // 이펙트 보유 여부
    {
        CleanupNullSpawnedObjects();

        for (int i = 0; i < spawnedObjectList.Count; i++)
        {
            var obj = spawnedObjectList[i];
            if (obj == null) continue;

            if (obj.GetComponent<Animator>() != null) return true; // 간단 기준(프리팹 규칙에 맞게)
        }

        return false;
    }

    private void CleanupNullSpawnedObjects() // null상태의 모든 오브젝트 제거하기
    {
        for (int i = spawnedObjectList.Count - 1; i >= 0; i--)
        {
            if (spawnedObjectList[i] == null)
                spawnedObjectList.RemoveAt(i);
        }
    }

    // 추후 풀링 기능 만든 후 확장 및 수정할 메서드. 일단은 리스트 비우기만
    public void UnregisterSpawnedObject(GameObject obj)
    {
        if (obj == null) return;
        spawnedObjectList.Remove(obj);
    }
}
