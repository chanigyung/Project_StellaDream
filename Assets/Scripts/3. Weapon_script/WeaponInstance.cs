using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponInstance
{
    public WeaponData data;
    public string uniqueID;
    
    public WeaponUpgradeInfo upgradeInfo = new WeaponUpgradeInfo();
    public SkillInstance[] skillInstances;

    //내구도 관련
    public bool isTemporary;
    public int maxDurability;
    public int currentDurability;

    public WeaponInstance(WeaponData data, bool isTemporary = true)
    {
        if (data == null)
            Debug.LogError("WeaponInstance 생성 시 data가 null입니다!");

        this.data = data;
        this.isTemporary = isTemporary;
        this.uniqueID = Guid.NewGuid().ToString(); // 고유 식별자

        if (isTemporary)
        {
            maxDurability = 100;
            currentDurability = UnityEngine.Random.Range(20, 101); // 20~100
        }
        else
        {
            maxDurability = -1;   // 무한
            currentDurability = -1;
        }

        skillInstances = CreateSkillInstances(data, upgradeInfo);
    }

    //무기 인스턴스 첫 생성시 호출. 스킬 인스턴스 가져오고 강화 적용시켜서 스킬 배열에 저장
    private SkillInstance[] CreateSkillInstances(WeaponData data, WeaponUpgradeInfo upgradeInfo)
    {
        var list = new List<SkillInstance>();

        if (data.mainSkill != null)
        {
            var instance = data.mainSkill.CreateInstance();
            instance.ApplyUpgrade(upgradeInfo);
            list.Add(instance);
        }

        if (data.subSkill != null)
        {
            var instance = data.subSkill.CreateInstance();
            instance.ApplyUpgrade(upgradeInfo);
            list.Add(instance);
        }

        return list.ToArray();
    }

    // 강화 시스템에서 호출
    public void ApplyUpgrade(WeaponUpgradeInfo newInfo)
    {
        this.upgradeInfo = newInfo;

        foreach (var skill in skillInstances)
        {
            skill.ApplyUpgrade(upgradeInfo);
        }
    }
    
    public SkillInstance GetMainSkillInstance()
    {
        return skillInstances.Length > 0 ? skillInstances[0] : null;
    }

    public SkillInstance GetSubSkillInstance()
    {
        return skillInstances.Length > 1 ? skillInstances[1] : null;
    }

    public bool UseOnce() //무기 사용시 내구도1 감소, 내구도 값 기반으로 true false 반환
    {
        if (!isTemporary) return true;

        if (currentDurability <= 0)
            return false;

        currentDurability--;
        return currentDurability > 0;
    }

    public float GetDurabilityPercent() //임시무기 내구도 백분율 계산(내구도 바 표시용)
    {
        if (!isTemporary) return 1f;
        return Mathf.Clamp01((float)currentDurability / maxDurability);
    }

    public string GetPrimaryTag() //무기의 첫 번째 태그 반환해주기
    {
        return data != null && data.tags != null && data.tags.Count > 0 ? data.tags[0] : "default";
    }

    // 비교 시 동일 무기 인스턴스인지 확인할 수 있도록
    // 같은 uniqueID인데 다른 객체로 인식되는 경우 방지
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