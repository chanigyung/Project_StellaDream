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
    [SerializeField] private Animator startEndAnimator;
    [SerializeField] private Animator endEndAnimator;
    private bool endAnchorsInitialized; // 시작, 끝 지점 중 하나 있으면 true

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

        ApplyOptionalAnimator(startEndAnimator, data.startEndAnimator);
        ApplyOptionalAnimator(endEndAnimator, data.endEndAnimator);

        // 시작 지점 있으면 애니메이션 앵커 이동
        if (startEndAnimator != null)
        {
            if (!TryGetComponent(out BoxCollider2D col)) return;

            float halfX = col.size.x * 0.5f;

            startEndAnimator.transform.localPosition = new Vector3(-halfX, 0f, 0f);
            endEndAnimator.transform.localPosition   = new Vector3( halfX, 0f, 0f);
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

    private void Tick()
    {
        skill.OnTick(attacker, null, gameObject); // [변경/추가]

        if (targetSet.Count == 0) return;

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

    private Vector2 GetMouseDirFromAttacker()
    {
        if (attacker == null || Camera.main == null) return Vector2.right;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)(mouseWorld - attacker.transform.position);

        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
    }

    // 레이저 스킬일 경우 시작지점/끝지점 이펙트 불여주기
    private void ApplyOptionalAnimator(Animator anim, RuntimeAnimatorController controller)
    {
        if (anim == null) return;

        if (controller == null)
        {
            anim.enabled = false;
            return;
        }

        anim.runtimeAnimatorController = controller;
        anim.enabled = true;
    }
}
