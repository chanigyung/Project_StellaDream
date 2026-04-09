// Scripts/3. Weapon_script/WeaponSkillBase.cs

using UnityEngine;

public enum WeaponSkillInputPhase
{
    Pressed,
    Released
}

public enum WeaponSkillSlot
{
    Main,
    Sub
}

public class WeaponControlBase
{
    protected readonly WeaponInstance weaponInstance;
    protected readonly SkillExecutor skillExecutor;

    public WeaponControlBase(WeaponInstance weaponInstance, SkillExecutor skillExecutor)
    {
        this.weaponInstance = weaponInstance;
        this.skillExecutor = skillExecutor;
    }

    protected WeaponControlBase(WeaponInstance weaponInstance, GameObject owner, MonoBehaviour runner)
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

        if (inputPhase == WeaponSkillInputPhase.Pressed)
            return RequestSkillUse(skillInstance, direction);

        if (inputPhase == WeaponSkillInputPhase.Released)
            return CancelCastingSkill(skillInstance);

        return false;
    }

    protected virtual bool RequestSkillUse(SkillInstance skillInstance, Vector2 direction)
    {
        if (skillInstance == null)
            return false;

        if (skillExecutor == null)
            return false;

        SkillContext context = CreateSkillContext(skillInstance, direction);
        return skillExecutor.UseSkill(context);
    }

    //캐스팅스킬 종료
    protected virtual bool CancelCastingSkill(SkillInstance skillInstance)
    {
        if (skillExecutor == null || skillInstance == null)
            return false;

        if (!skillInstance.IsCastingSkill)
            return false;

        if (!skillExecutor.IsCurrentCastingSkill(skillInstance))
            return false;

        skillExecutor.CancelCurrentCasting();
        return true;
    }

    protected virtual SkillContext CreateSkillContext(SkillInstance skillInstance, Vector2 direction)
    {
        return SkillUtils.CreateSkillContext(skillInstance, skillExecutor.gameObject, direction);
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