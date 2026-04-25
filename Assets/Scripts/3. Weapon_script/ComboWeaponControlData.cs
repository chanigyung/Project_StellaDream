using UnityEngine;

[CreateAssetMenu(fileName = "NewComboWeaponControlData", menuName = "WeaponData/Control/Combo")]
public class ComboWeaponControlData : WeaponControlData
{
    public float comboTimeLimit = 1f;

    public override WeaponControlType ControlType => WeaponControlType.Combo;

    public override WeaponControlBase CreateControl(WeaponInstance weaponInstance, SkillExecutor skillExecutor)
    {
        return new ComboWeaponControl(weaponInstance, skillExecutor, this);
    }
}
