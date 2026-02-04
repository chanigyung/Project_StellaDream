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

    public bool UseSkill(SkillInstance skillInstance, Vector2 direction, bool skipPostDelay = false)
    {
        if (skillInstance == null) return false;

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

        if (skillInstance.delay <= 0f && (skillInstance.postDelay <= 0f || skipPostDelay))
        {
            skillInstance.Execute(gameObject, direction.normalized);

            if (!skipPostDelay)
                ReleaseActiveSkill(skillInstance);

            return true;
        }

        // 하나라도 딜레이가 있으면 코루틴 실행
        StartCoroutine(ExecuteSkillDelay(skillInstance, direction.normalized, skipPostDelay));
        return true;
    }

    // [변경] ExecuteSkillWithDelay -> ExecuteSkillDelay
    private IEnumerator ExecuteSkillDelay(SkillInstance skill, Vector2 direction, bool skipPostDelay)
    {
        // 스킬 딜레이 적용
        if (skill.delay > 0f)
        {
            skill.Delay(gameObject, direction);
            yield return new WaitForSeconds(skill.delay);
        }  

        // 실행
        skill.Execute(gameObject, direction);

        // 후딜 (WhileHeld 유지 중이면 스킵)
        if (!skipPostDelay)
        {
            skill.PostDelay(gameObject, direction);

            if (skill.postDelay > 0f)
                yield return new WaitForSeconds(skill.postDelay);

            // 후딜까지 끝났으면 락 해제
            ReleaseActiveSkill(skill);
        }
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
    public void EndHeldSkill(SkillInstance skillInstance, Vector2 direction)
    {
        if (skillInstance == null) return;
        if (!heldSkill.Contains(skillInstance)) return;

        heldSkill.Remove(skillInstance);

        // 종료 순간에만 후딜 처리
        StartCoroutine(heldSkillPostDelay(skillInstance, direction));
    }

    // 후딜용 코루틴
    private IEnumerator heldSkillPostDelay(SkillInstance skill, Vector2 direction)
    {
        skill.PostDelay(gameObject, Vector2.right);

        if (skill.postDelay > 0f)
            yield return new WaitForSeconds(skill.postDelay);

        ReleaseActiveSkill(skill);
    }

    // activeSkill 해제 공통 함수
    private void ReleaseActiveSkill(SkillInstance skillInstance)
    {
        if (activeSkill == skillInstance)
            activeSkill = null;
    }
}
