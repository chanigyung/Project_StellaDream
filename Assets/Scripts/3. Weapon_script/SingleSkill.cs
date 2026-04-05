using UnityEngine;

public class SingleSkill : WeaponSkillBase
{
    public SingleSkill(WeaponInstance weaponInstance, GameObject owner, MonoBehaviour runner)
        : base(weaponInstance, owner, runner)
    {
    }

    protected override SkillInstance GetMainSkillInstance()
    {
        return weaponInstance?.mainSkillInstance;
    }

    protected override SkillInstance GetSubSkillInstance()
    {
        return null;
    }

    public override bool HandleSubInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        return false;
    }
}