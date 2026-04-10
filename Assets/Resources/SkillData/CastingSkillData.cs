using UnityEngine;
using UnityEngine.Serialization;

// [추가] 캐스팅 스킬 전용 데이터
[CreateAssetMenu(menuName = "Skill/Casting SkillData")]
public class CastingSkillData : SkillData
{
    [Header("캐스팅 설정")]
    [FormerlySerializedAs("castTime")]
    public float maxCastTime = 0.5f;
    public float castTickInterval = 0f;

    public override SkillUseType UseType => SkillUseType.Casting;

    public override SkillInstance CreateInstance()
    {
        return new CastingSkillInstance(this);
    }
}
