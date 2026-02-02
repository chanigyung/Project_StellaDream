using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SkillExecutor : MonoBehaviour
{
    private Dictionary<SkillInstance, float> lastUsedTimeDict = new();
    
    public bool UseSkill(SkillInstance skillInstance, Vector2 direction)
    {
        if (skillInstance == null) return false;

        if (lastUsedTimeDict.TryGetValue(skillInstance, out float lastUsed))
        {
            if (Time.time < lastUsed + skillInstance.cooldown)
                return false;
        }

        lastUsedTimeDict[skillInstance] = Time.time;

        if (skillInstance.delay <= 0f && skillInstance.postDelay <= 0f)
        {
            skillInstance.Execute(gameObject, direction.normalized);
            return true;
        }

        // 하나라도 딜레이가 있으면 코루틴 실행
        StartCoroutine(ExecuteSkillWithDelay(skillInstance, direction.normalized));
        return true;
    }

    private IEnumerator ExecuteSkillWithDelay(SkillInstance skill, Vector2 direction)
    {
        // 선딜
        skill.Delay(gameObject);

        if (skill.delay > 0f)
            yield return new WaitForSeconds(skill.delay);

        // 실제 스킬 실행
        skill.Execute(gameObject, direction);

        // 후딜
        skill.PostDelay(gameObject);

        if (skill.postDelay > 0f)
            yield return new WaitForSeconds(skill.postDelay);
    }
}