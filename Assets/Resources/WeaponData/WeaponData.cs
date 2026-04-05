using System;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { OneHanded, TwoHanded }

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "WeaponData/Weapon")]

public class WeaponData : ItemData
{
    public WeaponType weaponType;
    
    //외형 및 UI에 쓰일 데이터들
    public Sprite weaponSprite;
    
    public float mainRotationOffsetZ = 0f; //왼손일때 회전 Z값
    public float subRotationOffsetZ = 0f;  // 오른손일 때 회전 Z값 (한손무기만 사용)

    [Header("무기 스킬")]
    public SkillData mainSkill;
    public SkillData subSkill;

    [Header("무기 기준 쿨타임")]
    public float mainSkillCooldown = 0f;
    public float subSkillCooldown = 0f;

    //태그
    public List<string> tags = new List<String>();

    // public RuntimeAnimatorController animatorOverride;
    private void OnEnable()
    {
        itemType = ItemType.Weapon; // 무기로 자동 분류
    }
}