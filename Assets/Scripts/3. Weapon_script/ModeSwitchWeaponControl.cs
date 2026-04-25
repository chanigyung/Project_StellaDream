using UnityEngine;

public class ModeSwitchWeaponControl : WeaponControlBase
{
    private SkillInstance lastActivatedSkillInstance;

    public ModeSwitchWeaponControl(WeaponInstance weaponInstance, SkillExecutor skillExecutor, ModeSwitchWeaponControlData modeSwitchControlData)
        : base(weaponInstance, skillExecutor)
    {
        this.weaponInstance?.ClampCurrentModeIndex();
    }

    public override bool HandleMainInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        SkillInstance currentModeSkill = GetCurrentModeSkill();
        if (currentModeSkill == null)
            return false;

        if (inputPhase == WeaponSkillInputPhase.Released)
            return CancelCastingSkill(lastActivatedSkillInstance ?? currentModeSkill);

        if (inputPhase != WeaponSkillInputPhase.Pressed)
            return false;

        bool success = RequestSkillUse(currentModeSkill, WeaponSkillSlot.Main, inputPhase, direction);
        if (!success)
            return false;

        lastActivatedSkillInstance = currentModeSkill;
        return true;
    }

    public override bool HandleSubInput(WeaponSkillInputPhase inputPhase, Vector2 direction)
    {
        if (inputPhase != WeaponSkillInputPhase.Pressed)
            return false;

        int modeCount = weaponInstance != null ? weaponInstance.GetModeSkillCount() : 0;
        if (modeCount <= 0)
            return false;

        weaponInstance.currentModeIndex = (weaponInstance.currentModeIndex + 1) % modeCount;

        SkillInstance currentModeSkill = GetCurrentModeSkill();
        string skillName = currentModeSkill?.data != null ? currentModeSkill.data.name : "None";
        Debug.Log($"[ModeSwitchWeaponControl] Switched '{weaponInstance.weaponData?.name}' to mode {weaponInstance.currentModeIndex + 1}: {skillName}");
        return true;
    }

    protected override SkillInstance GetMainSkillInstance()
    {
        return GetCurrentModeSkill();
    }

    private SkillInstance GetCurrentModeSkill()
    {
        if (weaponInstance == null)
            return null;

        weaponInstance.ClampCurrentModeIndex();
        return weaponInstance.GetModeSkillAt(weaponInstance.currentModeIndex);
    }
}
