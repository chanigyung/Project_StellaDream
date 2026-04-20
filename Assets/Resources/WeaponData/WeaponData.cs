using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { OneHanded, TwoHanded }
public enum WeaponControlType { Default, Combo, ModeSwitch }

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "WeaponData/Weapon")]

public class WeaponData : ItemData
{
    public WeaponType weaponType;
    public WeaponControlData controlData;

    //외형 및 UI에 쓰일 데이터들
    public Sprite weaponSprite;
    
    [Header("무기 스킬")]
    public SkillData mainSkill;
    public SkillData subSkill;
    
    [Header("특수 무기용 스킬 리스트")]
    public List<SkillData> extraSkillList = new List<SkillData>();
    //태그
    public List<string> tags = new List<String>();

    // public RuntimeAnimatorController animatorOverride;
    private void OnEnable()
    {
        itemType = ItemType.Weapon; // 무기로 자동 분류
    }
}
