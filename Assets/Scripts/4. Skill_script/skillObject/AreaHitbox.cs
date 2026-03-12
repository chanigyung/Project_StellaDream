using System.Collections.Generic;
using UnityEngine;

public class AreaHitbox : SkillHitbox
{
    protected readonly HashSet<GameObject> insideTargetSet = new();

    private float tickInterval;
    private float tickTimer;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        insideTargetSet.Clear();
        tickTimer = 0f;
        tickInterval = ResolveTickInterval();
    }

    private float ResolveTickInterval()
    {
        if (skill == null || skill.data == null || skill.data.modules == null)
            return 0f;

        for (int i = 0; i < skill.data.modules.Count; i++)
        {
            if (skill.data.modules[i] is AreaHitboxModuleData areaData)
                return areaData.tickInterval;
        }

        return 0f;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;
        if (!TryGetDamageableTarget(other, out GameObject target)) return;

        insideTargetSet.Add(target);

        if (alreadyHit.Contains(target)) return;

        alreadyHit.Add(target);
        HandleHitTarget(target);
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!TryGetDamageableTarget(other, out GameObject target)) return;

        insideTargetSet.Remove(target);
    }

    private void FixedUpdate()
    {
        if (!initialized) return;
        if (tickInterval <= 0f) return;
        if (insideTargetSet.Count == 0) return;

        tickTimer += Time.fixedDeltaTime;
        if (tickTimer < tickInterval) return;

        tickTimer = 0f;
        ProcessTick();
    }

    private void ProcessTick()
    {
        if (insideTargetSet.Count == 0) return;

        List<GameObject> removeTargetList = null;

        foreach (GameObject target in insideTargetSet)
        {
            if (target == null)
            {
                removeTargetList ??= new List<GameObject>();
                removeTargetList.Add(target);
                continue;
            }

            SkillContext tickContext = CreateTickContext(target);
            skill.OnTick(tickContext);
        }

        if (removeTargetList == null) return;

        for (int i = 0; i < removeTargetList.Count; i++)
        {
            insideTargetSet.Remove(removeTargetList[i]);
        }
    }
}