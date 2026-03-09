using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SkillExecutor : MonoBehaviour
{
    private Dictionary<SkillInstance, float> lastUsedTimeDict = new();

    private SkillInstance activeSkill = null; //사용중인 스킬
    private readonly HashSet<SkillInstance> heldSkill = new();

    // 외부에서 현재 락 여부 확인시
    public bool IsCastLocked => activeSkill != null;

    public bool UseSkill(SkillContext context)
    {
        SkillInstance skillInstance = context.skillInstance;

        if (skillInstance == null) return false;
        if (skillInstance.skillLock) return false;

        if (activeSkill != null &&
            activeSkill != skillInstance &&
            !skillInstance.data.ignoreCastLock)
        {
            return false;
        }

        // 쿨타임 체크
        if (lastUsedTimeDict.TryGetValue(skillInstance, out float lastUsed))
        {
            if (Time.time < lastUsed + skillInstance.cooldown)
                return false;
        }

        lastUsedTimeDict[skillInstance] = Time.time;

        // 캐스팅 락을 무시하는 스킬은 activeSkill로 등록X
        if (!skillInstance.data.ignoreCastLock)
            activeSkill = skillInstance;

        // if (skillInstance.delay <= 0f && (skillInstance.postDelay <= 0f))
        // {
        //     skillInstance.Execute(context);
        //     ReleaseActiveSkill(skillInstance);
        //     return true;
        // }

        // 하나라도 딜레이가 있으면 코루틴 실행
        StartCoroutine(ExecuteSkillDelay(context));
        return true;
    }

    private IEnumerator ExecuteSkillDelay(SkillContext context)
    {
        SkillInstance skill = context.skillInstance;
        if (skill.delay > 0f)
        {
            skill.Delay(context);
            yield return new WaitForSeconds(skill.delay);
        }

        skill.Execute(context);

        skill.PostDelay(context);

        if (skill.postDelay > 0f)
            yield return new WaitForSeconds(skill.postDelay);

        ReleaseActiveSkill(skill);
    }

    // 홀드형 스킬 시작시 호출
    public void BeginHeldSkill(SkillInstance skillInstance)
    {
        if (skillInstance == null) return;

        heldSkill.Add(skillInstance);

        if (!skillInstance.data.ignoreCastLock)
            activeSkill = skillInstance;
    }

    // 홀드형 스킬 종료시 호출
    public void EndHeldSkill(SkillInstance skillInstance)
    {
        if (skillInstance == null) return;
        if (!heldSkill.Contains(skillInstance)) return;

        heldSkill.Remove(skillInstance);

        var hitboxObj = skillInstance.FindFirstSpawnedHitboxObject();
        if (hitboxObj != null)
        {
            skillInstance.UnregisterSpawnedObject(hitboxObj);
            Destroy(hitboxObj);
        }

        ReleaseActiveSkill(skillInstance);
    }

    // activeSkill 해제 공통 함수
    private void ReleaseActiveSkill(SkillInstance skillInstance)
    {
        if (activeSkill == skillInstance)
            activeSkill = null;
    }
}
