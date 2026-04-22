using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponData weaponData;
    public WeaponData data => weaponData;

    public string uniqueID;

    public WeaponUpgradeInfo upgradeInfo = new WeaponUpgradeInfo();

    public SkillInstance mainSkillInstance;
    public SkillInstance subSkillInstance;
    public readonly List<SkillInstance> extraSkillInstances = new();
    public CoreInstance coreInstance;

    public WeaponInstance(WeaponData data)
    {
        if (data == null)
            Debug.LogError("WeaponInstance 생성 시 data가 null입니다!");

        this.weaponData = data;
        this.uniqueID = Guid.NewGuid().ToString();

        CreateSkillInstances();
    }

    private void CreateSkillInstances()
    {
        if (data == null)
            return;

        extraSkillInstances.Clear();

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

        if (data.extraSkillList == null)
            return;

        for (int i = 0; i < data.extraSkillList.Count; i++)
        {
            SkillData extraSkillData = data.extraSkillList[i];
            if (extraSkillData == null)
                continue;

            SkillInstance extraSkillInstance = extraSkillData.CreateInstance();
            extraSkillInstance.ApplyUpgrade(upgradeInfo);
            extraSkillInstances.Add(extraSkillInstance);
        }
    }

    public void ApplyUpgrade(WeaponUpgradeInfo newInfo)
    {
        upgradeInfo = newInfo;

        if (mainSkillInstance != null)
            mainSkillInstance.ApplyUpgrade(upgradeInfo);

        if (subSkillInstance != null)
            subSkillInstance.ApplyUpgrade(upgradeInfo);

        for (int i = 0; i < extraSkillInstances.Count; i++)
        {
            SkillInstance extraSkillInstance = extraSkillInstances[i];
            if (extraSkillInstance == null)
                continue;

            extraSkillInstance.ApplyUpgrade(upgradeInfo);
        }
    }

    public int GetMainComboSkillCount()
    {
        int count = mainSkillInstance != null ? 1 : 0;
        count += extraSkillInstances.Count;
        return count;
    }

    public SkillInstance GetMainComboSkillAt(int comboIndex)
    {
        if (comboIndex < 0)
            return null;

        if (comboIndex == 0)
            return mainSkillInstance;

        int extraSkillIndex = comboIndex - 1;
        if (extraSkillIndex < 0 || extraSkillIndex >= extraSkillInstances.Count)
            return null;

        return extraSkillInstances[extraSkillIndex];
    }

    public string GetPrimaryTag()
    {
        return data != null && data.tags != null && data.tags.Count > 0 ? data.tags[0].ToString() : "default";
    }

    public bool CanEquipCore(CoreData coreData)
    {
        if (coreData == null || data == null || data.tags == null)
            return false;

        return data.tags.Contains(coreData.primaryTag);
    }

    public bool EquipCore(CoreData coreData)
    {
        if (!CanEquipCore(coreData))
            return false;

        coreInstance = coreData.CreateInstance();
        return true;
    }

    public void UnequipCore()
    {
        coreInstance = null;
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
