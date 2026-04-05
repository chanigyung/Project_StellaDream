using System;
using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponData data;
    public string uniqueID;

    public WeaponUpgradeInfo upgradeInfo = new WeaponUpgradeInfo();

    public SkillInstance mainSkillInstance;
    public SkillInstance subSkillInstance;

    public WeaponInstance(WeaponData data)
    {
        if (data == null)
            Debug.LogError("WeaponInstance 생성 시 data가 null입니다!");

        this.data = data;
        this.uniqueID = Guid.NewGuid().ToString();

        CreateSkillInstances();
    }

    private void CreateSkillInstances()
    {
        if (data == null)
            return;

        if (data.mainSkill != null)
        {
            mainSkillInstance = data.mainSkill.CreateInstance();
            mainSkillInstance.ApplyUpgrade(upgradeInfo);
        }

        if (data.subSkill != null)
        {
            subSkillInstance = data.subSkill.CreateInstance();
            subSkillInstance.ApplyUpgrade(upgradeInfo);
        }
    }

    public void ApplyUpgrade(WeaponUpgradeInfo newInfo)
    {
        upgradeInfo = newInfo;

        if (mainSkillInstance != null)
            mainSkillInstance.ApplyUpgrade(upgradeInfo);

        if (subSkillInstance != null)
            subSkillInstance.ApplyUpgrade(upgradeInfo);
    }

    public string GetPrimaryTag()
    {
        return data != null && data.tags != null && data.tags.Count > 0 ? data.tags[0] : "default";
    }

    public override bool Equals(object obj)
    {
        if (obj is WeaponInstance other)
            return uniqueID == other.uniqueID;
        return false;
    }

    public override int GetHashCode()
    {
        return uniqueID.GetHashCode();
    }
}