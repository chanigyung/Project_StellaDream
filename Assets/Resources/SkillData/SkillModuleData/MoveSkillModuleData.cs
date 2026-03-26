using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Move/MoveSkill")]
public class MoveSkillModuleData : SkillModuleData
{
    public float distance = 3f;
    public float duration = 0.2f;

    public override ISkillModule CreateModule()
    {
        return new MoveSkillModule(this);
    }
}