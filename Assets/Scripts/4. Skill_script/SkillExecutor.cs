using UnityEngine;
using System.Collections.Generic;

public class SkillExecutor : MonoBehaviour
{
    private Dictionary<SkillInstance, float> lastUsedTimeDict = new();
    
    public bool UseSkill(SkillInstance skillInstance, Vector2 direction)
    {
        if (skillInstance == null) return false;

        float lastUsed;
        if (lastUsedTimeDict.TryGetValue(skillInstance, out lastUsed))
        {
            if (Time.time < lastUsed + skillInstance.cooldown)
                return false;
        }

        lastUsedTimeDict[skillInstance] = Time.time;

        skillInstance.Execute(gameObject, direction.normalized);
        return true;
    }
}