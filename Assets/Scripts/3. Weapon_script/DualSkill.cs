using UnityEngine;

public class DualSkill : WeaponSkillBase
{
    public DualSkill(WeaponInstance weaponInstance, GameObject owner, MonoBehaviour runner)
        : base(weaponInstance, owner, runner)
    {
    }

    protected override SkillInstance GetMainSkillInstance()
    {
        return weaponInstance?.mainSkillInstance;
    }

    protected override SkillInstance GetSubSkillInstance()
    {
        return weaponInstance?.subSkillInstance;
    }
}