using System.Collections.Generic;
using UnityEngine;

public class AreaHitbox : MonoBehaviour
{
    private GameObject attacker;
    private SkillInstance skill;

    private float tickInterval;
    private float tickTimer;

    private readonly HashSet<GameObject> targetSet = new();

    private bool initialized;

    public void Initialize(GameObject attacker, SkillInstance skill, float tickInterval, float duration)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.tickInterval = Mathf.Max(0.01f, tickInterval);

        initialized = true;

        Invoke(nameof(Expire), Mathf.Max(0.01f, duration));
    }

    private void Update()
    {
        if (!initialized) return;

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;

        tickTimer = 0f;

        // [추가] Tick 처리: 대상별 데미지 + SkillInstance.OnTick 브로드캐스트
        Tick();
    }

    private void Tick()
    {
        skill.OnTick(attacker, null, gameObject); // [변경/추가]

        if (targetSet.Count == 0) return;

        // [유지] null 정리용
        List<GameObject> removeList = null;

        foreach (var target in targetSet)
        {
            if (target == null)
            {
                removeList ??= new List<GameObject>();
                removeList.Add(target);
                continue;
            }

            skill.OnHit(attacker, target); // [변경/추가]
        }

        if (removeList != null)
        {
            for (int i = 0; i < removeList.Count; i++)
                targetSet.Remove(removeList[i]);
        }
    }

    private void Expire()
    {
        if (!initialized) return;

        skill.OnExpire(attacker, gameObject);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return;

        targetSet.Add(damageable.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!initialized) return;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return;

        targetSet.Remove(damageable.gameObject);
    }
}
