using UnityEngine;
using System.Collections.Generic;

public abstract class SkillModuleData : ScriptableObject
{
    [Header("Module Tags")]
    public List<SkillTag> tags = new();

    public abstract ISkillModule CreateModule();
}
