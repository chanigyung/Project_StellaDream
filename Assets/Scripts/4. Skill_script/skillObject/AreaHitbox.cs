using System.Collections.Generic;
using UnityEngine;

public class AreaHitbox : MonoBehaviour
{
    private GameObject attacker;
    private SkillInstance skill;
    private AreaHitboxModuleData data; 

    private SkillSpawnPointType spawnPointType;
    private Vector2 spawnOffset;

    private float tickInterval;
    private float tickTimer;
    private float duration;

    private readonly HashSet<GameObject> targetSet = new();

    private bool initialized;

    private bool followWhileHeld;
    private bool rotateWhileHeld;
    // duration <= 0 무한 유지 지원 + Invoke 취소/중복 방지용
    // private bool hasExpireTimer;

    //히트박스 이미지 렌더러
    [SerializeField] private Animator hitboxAnimator;

    public void Initialize(GameObject attacker, SkillInstance skill, AreaHitboxModuleData data)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.data = data;

        spawnPointType = skill.data.spawnPointType;
        spawnOffset = data.spawnOffset;

        initialized = true;

        tickInterval = Mathf.Max(0.01f, data.tickInterval);
        duration = data.duration;
        followWhileHeld = data.followWhileHeld;
        rotateWhileHeld = data.rotateWhileHeld;

        if (hitboxAnimator != null && data.hitboxAnimator != null)
        {
            hitboxAnimator.runtimeAnimatorController = data.hitboxAnimator;
            hitboxAnimator.enabled = true;
        }

        if (duration > 0f)
        {
            Invoke(nameof(Expire), duration);
        }
    }

    private void Update()
    {
        if (!initialized) return;

        if (skill != null &&
        skill.data.activationType == SkillActivationType.WhileHeld &&
        (followWhileHeld || rotateWhileHeld))
        {
            Vector2 dir = GetMouseDirFromAttacker();
            SkillUtils.CalculateSpawnTransform(attacker, skill, dir, spawnPointType, spawnOffset, out var pos, out var rot, out _);

            if (followWhileHeld)
                transform.position = pos;

            if (rotateWhileHeld && skill.RotateEffect)
                transform.rotation = rot;
        }

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval) return;

        tickTimer = 0f;
        Tick();
    }

    private Vector2 GetMouseDirFromAttacker()
    {
        if (attacker == null || Camera.main == null) return Vector2.right;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)(mouseWorld - attacker.transform.position);

        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
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

        initialized = false;

        skill.OnExpire(attacker, gameObject);

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized) return;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return;
        if (damageable.gameObject == attacker) return;

        targetSet.Add(damageable.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!initialized) return;

        Component damageable = other.GetComponentInParent<IDamageable>() as Component;
        if (damageable == null) return;
        if (damageable.gameObject == attacker) return;

        targetSet.Remove(damageable.gameObject);
    }
}
