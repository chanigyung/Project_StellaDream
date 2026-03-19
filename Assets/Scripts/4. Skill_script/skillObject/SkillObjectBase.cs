using System.Collections;
using UnityEngine;

public abstract class SkillObjectBase : MonoBehaviour
{
    protected SkillContext context;
    protected GameObject attacker;
    protected SkillInstance skill;
    protected Vector2 direction;

    protected bool initialized;
    private Coroutine lifetimeRoutine;

    private bool expired; // 오브젝트 expire 중복호출 방지용

    public void Initialize(SkillContext context, float lifetime)
    {
        this.context = context;
        this.attacker = context.attacker;
        this.skill = context.skillInstance;
        this.direction = context.hasDirection && context.direction.sqrMagnitude > 0.0001f
            ? context.direction.normalized : Vector2.right;

        initialized = true;

        OnInitialize();

        if (lifetime > 0f)
            lifetimeRoutine = StartCoroutine(LifetimeRoutine(lifetime));
    }

    public void ObjectSpawned()
    {
        if (skill == null) return;

        SkillContext spawnedContext = CreateSpawnedContext();
        skill.OnObjectSpawned(spawnedContext);
    }

    protected virtual void OnInitialize() { }
    protected virtual void OnTick() { }

    // 외부에서 즉시 Expire+오브젝트 파괴용
    protected void ExpireNowAndDestroy()
    {
        if (expired) return;
        expired = true;

        if (skill != null)
        {
            SkillContext expireContext = CreateExpireContext();
            skill.OnExpire(expireContext);
            skill.UnregisterSpawnedObject(gameObject);
        }

        Destroy(gameObject);
    }

    /// lifetime 코루틴을 중단(히트 등으로 즉시 파괴되는 경우)
    protected void StopLifetime()
    {
        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);
    }

    private IEnumerator LifetimeRoutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        ExpireNowAndDestroy();
    }

    // 생성 직후 기본 컨텍스트 생성
    protected virtual SkillContext CreateSpawnedContext()
    {
        SkillContext spawnedContext = context.Clone();

        spawnedContext.contextOwner = gameObject;
        spawnedContext.sourceObject = gameObject;
        spawnedContext.targetObject = null;
        spawnedContext.position = transform.position;
        spawnedContext.rotation = transform.rotation;
        spawnedContext.spawnPointType = context.spawnPointType;

        if (context.hasDirection && context.direction.sqrMagnitude > 0.0001f)
        {
            spawnedContext.direction = context.direction.normalized;
            spawnedContext.hasDirection = true;
        }
        else
        {
            spawnedContext.direction = direction;
            spawnedContext.hasDirection = false;
        }
        SkillUtils.FillContextSpawnPoints(ref spawnedContext, spawnedContext.contextOwner);

        return spawnedContext;
    }

    // 피격 시 기본 컨텍스트 생성
    protected virtual SkillContext CreateHitContext(GameObject targetObject)
    {
        SkillContext hitContext = context.Clone();

        hitContext.contextOwner = targetObject != null ? targetObject : gameObject;
        hitContext.sourceObject = gameObject;
        hitContext.targetObject = targetObject;
        hitContext.position = transform.position;
        hitContext.rotation = transform.rotation;
        hitContext.direction = direction;
        hitContext.hasDirection = true;
        SkillUtils.FillContextSpawnPoints(ref hitContext, hitContext.contextOwner);

        return hitContext;
    }

    protected virtual SkillContext CreateTickContext(GameObject targetObject)
    {
        SkillContext tickContext = context.Clone();

        tickContext.contextOwner = targetObject != null ? targetObject : gameObject;
        tickContext.sourceObject = gameObject;
        tickContext.targetObject = targetObject;
        tickContext.position = transform.position;
        tickContext.rotation = transform.rotation;
        tickContext.direction = direction;
        tickContext.hasDirection = true;
        SkillUtils.FillContextSpawnPoints(ref tickContext, tickContext.contextOwner);

        return tickContext;
    }

    // 만료 시 기본 컨텍스트 생성
    protected virtual SkillContext CreateExpireContext()
    {
        SkillContext expireContext = context.Clone();

        expireContext.contextOwner = gameObject;
        expireContext.sourceObject = gameObject;
        expireContext.targetObject = null;
        expireContext.position = transform.position;
        expireContext.rotation = transform.rotation;
        expireContext.direction = direction;
        expireContext.hasDirection = context.hasDirection;
        SkillUtils.FillContextSpawnPoints(ref expireContext, expireContext.contextOwner);

        return expireContext;
    }
}