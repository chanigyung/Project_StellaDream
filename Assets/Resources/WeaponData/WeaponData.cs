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
    
    public float mainRotationOffsetZ = -135f; //왼손일때 회전 Z값
    public float subRotationOffsetZ = 45f;  // 오른손일 때 회전 Z값 (한손무기만 사용)

    //스킬 데이터
    public SkillData mainSkill;
    public SkillData subSkill;

    //태그
    public List<string> tags = new List<String>();

    // public RuntimeAnimatorController animatorOverride;
    private void OnEnable()
    {
        itemType = ItemType.Weapon; // 무기로 자동 분류
    }
}