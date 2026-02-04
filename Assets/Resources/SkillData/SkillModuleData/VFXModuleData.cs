using System.Collections.Generic;
using UnityEngine;

public enum VFXHook { Delay, Execute, PostDelay, Hit, Tick, Expire }

public enum VFXAnchor { Caster, Target, SourceObject, }

[System.Serializable]
public class VFXEntry
{
    public VFXHook hook;
    public VFXAnchor anchor;

    [Header("프리팹")]
    public GameObject prefab;

    [Header("애니메이터")]
    public RuntimeAnimatorController animator;

    [Header("방향")]
    public bool useDirection;

    [Header("오프셋")]
    public Vector3 localOffset;

    [Header("트리거")]
    public string trigger;
}

[CreateAssetMenu(menuName = "SkillModule/VFX")]
public class VFXModuleData : SkillModuleData
{
    [Header("VFX Entries")]
    public List<VFXEntry> vfxEntryList = new();

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new VFXModule(owner, this);
    }
}
