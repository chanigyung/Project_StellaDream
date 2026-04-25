using UnityEngine;

public abstract class WeaponControlData : ScriptableObject
{
    public abstract WeaponControlType ControlType { get; }

    public abstract WeaponControlBase CreateControl(WeaponInstance weaponInstance, SkillExecutor skillExecutor);

    public virtual void InitializeWeaponInstance(WeaponInstance weaponInstance)
    {
    }

    public virtual void ApplyUpgrade(WeaponInstance weaponInstance, WeaponUpgradeInfo upgradeInfo)
    {
    }
}
