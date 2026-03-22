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

    // 스킬 잠금 관련
    private readonly HashSet<SkillLockReason> lockReasonSet = new();
    public bool IsLocked => lockReasonSet.Count > 0;

    public SkillInstance(SkillData data)
    {
        this.data = data;

        cooldown = data.cooldown;
        delay = data.delay;
        postDelay = data.postDelay;

        if (data.modules != null)
        {
            foreach (var moduleData in data.modules)
            {
                if (moduleData == null) continue;

                var module = moduleData.CreateModule();
                if (module != null)
                    modules.Add(module);
            }
        }
    }

    public void Delay(SkillContext context)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnDelay(context);
    }

    public void Execute(SkillContext context)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnExecute(context);
    }

    public void OnObjectSpawned(SkillContext context)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnObjectSpawned(context);
    }

    public void OnHit(SkillContext context)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnHit(context);
    }

    public void OnTick(SkillContext context)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnTick(context);
    }

    public void OnExpire(SkillContext context)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnExpire(context);
    }

    public void PostDelay(SkillContext context)
    {
        for (int i = 0; i < modules.Count; i++)
            modules[i].OnPostDelay(context);
    }

    public void ApplyUpgrade(WeaponUpgradeInfo UpgradeInfo)
    {
        
    }

    // ------------------------------ 스킬 잠금 관련 메서드 ---------------------------//
    public void AddLock(SkillLockReason reason)
    {
        lockReasonSet.Add(reason);
    }

    // 추가: Lock reason 해제
    public void RemoveLock(SkillLockReason reason)
    {
        lockReasonSet.Remove(reason);
    }

    // 추가: 특정 reason 보유 여부 확인
    public bool HasLockReason(SkillLockReason reason)
    {
        return lockReasonSet.Contains(reason);
    }

    // 추가: 전체 Lock 해제 필요 시 사용
    public void ClearAllLocks()
    {
        lockReasonSet.Clear();
    }

    // ------------ 스킬이 사용하는 오브젝트 등록 및 해제 메서드들(프로토타입) ------------ //
    public void RegisterSpawnedObject(GameObject obj)
    {
        if (obj == null) return;

        CleanupNullSpawnedObjects();

        if (spawnedObjectList.Contains(obj)) return;
        spawnedObjectList.Add(obj);
    }

    public bool HasSpawnedObject()
    {
        CleanupNullSpawnedObjects();
        return spawnedObjectList.Count > 0;
    }

    public void UnregisterSpawnedObject(GameObject obj)
    {
        if (obj == null) return;
        spawnedObjectList.Remove(obj);
    }

    public void ReleaseSpawnedObject(GameObject obj)
    {
        if (obj == null) return;

        UnregisterSpawnedObject(obj);
        Object.Destroy(obj);
    }

    public void ReleaseAllSpawnedObjects()
    {
        CleanupNullSpawnedObjects();

        for (int i = spawnedObjectList.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjectList[i];
            if (obj == null) continue;

            Object.Destroy(obj);
        }

        spawnedObjectList.Clear();
    }

    private void CleanupNullSpawnedObjects() 
    {
        for (int i = spawnedObjectList.Count - 1; i >= 0; i--)
        {
            if (spawnedObjectList[i] == null)
                spawnedObjectList.RemoveAt(i);
        }
    }
}
