using UnityEngine;
using System.Collections.Generic;

public abstract class SkillModuleData : ScriptableObject
{
    [Header("Module Tags")]
    public List<SkillTag> tags = new();

    public abstract ISkillModule CreateModule();

    protected void EnsureTags(params SkillTag[] defaultTags)
    {
        tags ??= new List<SkillTag>();

        if (defaultTags == null)
            return;

        for (int i = 0; i < defaultTags.Length; i++)
        {
            SkillTag tag = defaultTags[i];
            if (!tags.Contains(tag))
                tags.Add(tag);
        }
    }
}
