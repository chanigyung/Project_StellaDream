using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/VFX")]
public class VFXModuleData : SkillModuleData
{
    [Header("스킬 이펙트 프리팹")]
    public GameObject effectPrefab;

    [Header("스킬 애니메이션")]
    public RuntimeAnimatorController animator;

    [Header("이펙트 지속시간")]
    public float duration = 0.5f;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new VFXModule(owner, this);
    }
}
