using UnityEngine;

public abstract class SkillModuleData : ScriptableObject
{
    public abstract ISkillModule CreateModule(SkillInstance owner);
}
