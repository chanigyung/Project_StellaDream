using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewModeSwitchWeaponControlData", menuName = "WeaponData/Control/ModeSwitch")]
public class ModeSwitchWeaponControlData : WeaponControlData
{
    [Header("Mode Skills")]
    public List<SkillData> modeSkills = new();

    public override WeaponControlType ControlType => WeaponControlType.ModeSwitch;

    public override WeaponControlBase CreateControl(WeaponInstance weaponInstance, SkillExecutor skillExecutor)
    {
        return new ModeSwitchWeaponControl(weaponInstance, skillExecutor, this);
    }

    public override void InitializeWeaponInstance(WeaponInstance weaponInstance)
    {
        if (weaponInstance == null)
            return;

        weaponInstance.modeSkillInstances.Clear();

        if (modeSkills == null)
            return;

        for (int i = 0; i < modeSkills.Count; i++)
        {
            SkillData modeSkillData = modeSkills[i];
            if (modeSkillData == null)
                continue;

            SkillInstance modeSkillInstance = modeSkillData.CreateInstance();
            modeSkillInstance.ApplyUpgrade(weaponInstance.upgradeInfo);
            weaponInstance.modeSkillInstances.Add(modeSkillInstance);
        }
    }
}
