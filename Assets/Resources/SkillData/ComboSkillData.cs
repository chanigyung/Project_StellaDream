using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skill/ComboSkill")]
public class ComboSkillData : SkillData
{
    public override SkillType SkillType => SkillType.Combo;

    [Header("콤보 단계 스킬들")]
    public List<SkillData> comboSteps;

    [Header("콤보 리셋 시간 (초)")]
    public float comboResetTime = 1.0f;

    public override SkillInstance CreateInstance()
    {
        return new ComboSkillInstance(this);
    }
}
