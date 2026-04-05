using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponSkillInputPhase
{
    Pressed,
    Held,
    Released
}

public enum WeaponSkillSlot
{
    Main,
    Sub
}

public abstract class WeaponSkillBase
{
    protected readonly WeaponInstance weaponInstance;
    protected readonly GameObject owner;
    protected readonly MonoBehaviour runner;

    protected float mainCooldownEndTime;
    protected float subCooldownEndTime;

    protected SkillInstance activeSkill = null;
    protected readonly HashSet<SkillInstance> heldSkillSet = new();

    protected WeaponSkillBase(WeaponInstance weaponInstance, GameObject owner, MonoBehaviour runner)
    {
        this.weaponInstance = weaponInstance;
        this.owner = owner;
        this.runner = runner;
    }

    // main 스킬 입력(눌림/홀드/뗌)을 받아 activationType에 맞게 처리한다.
    public virtual bool HandleMainInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        return HandleSkillInput(GetMainSkillInstance(), WeaponSkillSlot.Main, inputPhase, direction);
    }

    // main 스킬 입력(눌림/홀드/뗌)을 받아 activationType에 맞게 처리한다.
    public virtual bool HandleSubInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        return HandleSkillInput(GetSubSkillInstance(), WeaponSkillSlot.Sub, inputPhase, direction);
    }

    protected abstract SkillInstance GetMainSkillInstance();
    protected abstract SkillInstance GetSubSkillInstance();


    // 입력 단계와 activationType을 비교해 실제 시전/홀드 시작/홀드 종료를 분기한다.
    protected virtual bool HandleSkillInput(SkillInstance skillInstance, WeaponSkillSlot skillSlot, WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        if (skillInstance == null)
            return false;

        switch (skillInstance.data.activationType)
        {
            case SkillActivationType.OnPress:
                if (inputPhase != WeaponSkillInputPhase.Pressed)
                    return false;
                return TryCastSkill(skillInstance, skillSlot, direction);

            case SkillActivationType.OnRelease:
                if (inputPhase != WeaponSkillInputPhase.Released)
                    return false;
                return TryCastSkill(skillInstance, skillSlot, direction);

            case SkillActivationType.WhileHeld:
                if (inputPhase == WeaponSkillInputPhase.Pressed)
                {
                    BeginHeldSkill(skillInstance);
                    return false;
                }

                if (inputPhase == WeaponSkillInputPhase.Held)
                    return TryCastSkill(skillInstance, skillSlot, direction);

                if (inputPhase == WeaponSkillInputPhase.Released)
                {
                    EndHeldSkill(skillInstance);
                    return false;
                }
                break;
        }

        return false;
    }

    // 스킬 사용가능 여부 확인 후 context 생성 및 스킬 코루틴 실행
    protected virtual bool TryCastSkill(SkillInstance skillInstance, WeaponSkillSlot skillSlot, Vector2 direction)
    {
        if (!CanUseSkill(skillInstance, skillSlot))
            return false;

        SkillContext context = CreateSkillContext(skillInstance, direction);

        if (!skillInstance.data.ignoreCastLock)
            activeSkill = skillInstance;

        StartCooldown(skillSlot);
        runner.StartCoroutine(ExecuteSkillDelay(context));
        return true;
    }

    protected virtual bool CanUseSkill(SkillInstance skillInstance, WeaponSkillSlot skillSlot)
    {
        if (skillInstance == null)
            return false;

        if (skillInstance.IsLocked)
            return false;

        if (activeSkill != null &&
            activeSkill != skillInstance &&
            !skillInstance.data.ignoreCastLock)
        {
            return false;
        }

        return !IsOnCooldown(skillSlot);
    }

    protected bool IsOnCooldown(WeaponSkillSlot skillSlot)
    {
        return GetCooldownRemaining(skillSlot) > 0f;
    }


    //지정한 슬롯의 기본 쿨타임 값을 WeaponData에서 가져온다.
    protected virtual float GetBaseCooldown(WeaponSkillSlot skillSlot)
    {
        if (weaponInstance == null || weaponInstance.data == null)
            return 0f;

        return skillSlot == WeaponSkillSlot.Main
            ? weaponInstance.data.mainSkillCooldown
            : weaponInstance.data.subSkillCooldown;
    }

    // 지정한 슬롯의 남은 쿨타임을 반환한다.
    protected float GetCooldownRemaining(WeaponSkillSlot skillSlot)
    {
        return skillSlot == WeaponSkillSlot.Main
            ? Mathf.Max(0f, mainCooldownEndTime - Time.time)
            : Mathf.Max(0f, subCooldownEndTime - Time.time);
    }

    // 지정한 슬롯의 쿨타임을 현재 시각 기준으로 시작한다.
    protected void StartCooldown(WeaponSkillSlot skillSlot)
    {
        float cooldown = GetBaseCooldown(skillSlot);
        float endTime = Time.time + Mathf.Max(0f, cooldown);

        if (skillSlot == WeaponSkillSlot.Main)
            mainCooldownEndTime = endTime;
        else
            subCooldownEndTime = endTime;
    }

    protected virtual IEnumerator ExecuteSkillDelay(SkillContext context)
    {
        SkillInstance skill = context.skillInstance;

        if (skill.delay > 0f)
        {
            skill.Delay(context);
            yield return new WaitForSeconds(skill.delay);
        }

        skill.Execute(context);
        skill.PostDelay(context);

        if (skill.postDelay > 0f)
            yield return new WaitForSeconds(skill.postDelay);

        ReleaseActiveSkill(skill);
    }

    protected void BeginHeldSkill(SkillInstance skillInstance)
    {
        if (skillInstance == null)
            return;

        heldSkillSet.Add(skillInstance);

        if (!skillInstance.data.ignoreCastLock)
            activeSkill = skillInstance;
    }

    protected void EndHeldSkill(SkillInstance skillInstance)
    {
        if (skillInstance == null)
            return;

        if (!heldSkillSet.Contains(skillInstance))
            return;

        heldSkillSet.Remove(skillInstance);
        ReleaseActiveSkill(skillInstance);
    }

    protected void ReleaseActiveSkill(SkillInstance skillInstance)
    {
        if (activeSkill == skillInstance)
            activeSkill = null;
    }

    protected virtual SkillContext CreateSkillContext(SkillInstance skillInstance, Vector2 direction)
    {
        Vector2 normalizedDirection = direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : Vector2.right;

        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;

        SkillContext context = new SkillContext
        {
            skillInstance = skillInstance,
            attacker = owner,
            contextOwner = owner,
            sourceObject = null,
            targetObject = null,
            position = owner != null ? owner.transform.position : Vector3.zero,
            rotation = Quaternion.Euler(0f, 0f, angle),
            direction = normalizedDirection,
            hasDirection = true,
            spawnPointType = skillInstance != null ? skillInstance.SpawnPointType : SkillSpawnPointType.Center
        };

        SkillUtils.FillContextSpawnPoints(ref context, owner);
        return context;
    }
}