// Scripts/3. Weapon_script/WeaponSkillBase.cs

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

public class WeaponSkillBase
{
    protected readonly WeaponInstance weaponInstance;
    protected readonly SkillExecutor skillExecutor;

    protected float mainCooldownEndTime;
    protected float subCooldownEndTime;

    public WeaponSkillBase(WeaponInstance weaponInstance, SkillExecutor skillExecutor)
    {
        this.weaponInstance = weaponInstance;
        this.skillExecutor = skillExecutor;
    }

    // SingleSkill / DualSkill 호환용 생성자
    protected WeaponSkillBase(WeaponInstance weaponInstance, GameObject owner, MonoBehaviour runner)
        : this(weaponInstance, ResolveSkillExecutor(owner, runner))
    {
    }

    public virtual bool HandleMainInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        return HandleSkillInput(GetMainSkillInstance(), WeaponSkillSlot.Main, inputPhase, direction);
    }

    public virtual bool HandleSubInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        return HandleSkillInput(GetSubSkillInstance(), WeaponSkillSlot.Sub, inputPhase, direction);
    }

    protected virtual SkillInstance GetMainSkillInstance()
    {
        return weaponInstance?.mainSkillInstance;
    }

    protected virtual SkillInstance GetSubSkillInstance()
    {
        return weaponInstance?.subSkillInstance;
    }

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
                    return BeginHeldSkill(skillInstance, skillSlot);

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

    protected virtual bool TryCastSkill(SkillInstance skillInstance, WeaponSkillSlot skillSlot, Vector2 direction)
    {
        if (!CanUseSkill(skillInstance, skillSlot))
            return false;

        if (skillExecutor == null)
            return false;

        SkillContext context = CreateSkillContext(skillInstance, direction);

        // [수정] 실제 실행은 SkillExecutor가 담당하고,
        // WeaponSkillBase는 슬롯 쿨타임만 관리한다.
        bool success = skillExecutor.TryExecuteSkill(context, false);

        if (!success)
            return false;

        StartCooldown(skillSlot);
        return true;
    }

    protected virtual bool CanUseSkill(SkillInstance skillInstance, WeaponSkillSlot skillSlot)
    {
        if (skillInstance == null)
            return false;

        if (skillExecutor == null)
            return false;

        if (skillInstance.IsLocked)
            return false;

        return !IsOnCooldown(skillSlot);
    }

    protected virtual bool BeginHeldSkill(SkillInstance skillInstance, WeaponSkillSlot skillSlot)
    {
        if (!CanUseSkill(skillInstance, skillSlot))
            return false;

        return skillExecutor.BeginHeldSkill(skillInstance);
    }

    protected virtual void EndHeldSkill(SkillInstance skillInstance)
    {
        if (skillExecutor == null || skillInstance == null)
            return;

        skillExecutor.EndHeldSkill(skillInstance);
    }

    protected bool IsOnCooldown(WeaponSkillSlot skillSlot)
    {
        return GetCooldownRemaining(skillSlot) > 0f;
    }

    protected virtual float GetBaseCooldown(WeaponSkillSlot skillSlot)
    {
        if (weaponInstance == null || weaponInstance.weaponData == null)
            return 0f;

        return skillSlot == WeaponSkillSlot.Main
            ? weaponInstance.weaponData.mainSkillCooldown
            : weaponInstance.weaponData.subSkillCooldown;
    }

    protected float GetCooldownRemaining(WeaponSkillSlot skillSlot)
    {
        return skillSlot == WeaponSkillSlot.Main
            ? Mathf.Max(0f, mainCooldownEndTime - Time.time)
            : Mathf.Max(0f, subCooldownEndTime - Time.time);
    }

    protected void StartCooldown(WeaponSkillSlot skillSlot)
    {
        float cooldown = GetBaseCooldown(skillSlot);
        float endTime = Time.time + Mathf.Max(0f, cooldown);

        if (skillSlot == WeaponSkillSlot.Main)
            mainCooldownEndTime = endTime;
        else
            subCooldownEndTime = endTime;
    }

    protected virtual SkillContext CreateSkillContext(SkillInstance skillInstance, Vector2 direction)
    {
        return skillExecutor.CreateCastContext(skillInstance, skillExecutor.gameObject, direction);
    }

    private static SkillExecutor ResolveSkillExecutor(GameObject owner, MonoBehaviour runner)
    {
        if (owner == null && runner == null)
            return null;

        GameObject target = owner != null ? owner : runner.gameObject;
        SkillExecutor executor = target.GetComponent<SkillExecutor>();

        if (executor == null)
            executor = target.AddComponent<SkillExecutor>();

        return executor;
    }
}