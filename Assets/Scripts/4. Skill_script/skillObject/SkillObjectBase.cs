using System.Collections;
using UnityEngine;

public abstract class SkillObjectBase : MonoBehaviour
{
    protected GameObject attacker;
    protected SkillInstance skill;
    protected Vector2 direction;

    protected bool initialized;
    private Coroutine lifetimeRoutine;

    public void Initialize(GameObject attacker, SkillInstance skill, Vector2 direction, float lifetime)
    {
        this.attacker = attacker;
        this.skill = skill;
        this.direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;

        initialized = true;

        OnInitialize();

        if (this.skill != null)
            this.skill.OnObjectSpawned(this.attacker, gameObject, this.direction);

        if (lifetime > 0f)
            lifetimeRoutine = StartCoroutine(LifetimeRoutine(lifetime));
    }

    protected virtual void OnInitialize() { }
    protected virtual void OnTick() { }
    protected virtual void OnExpire() { }

    /// lifetime 코루틴을 중단(히트 등으로 즉시 파괴되는 경우)
    protected void StopLifetime()
    {
        if (lifetimeRoutine != null)
            StopCoroutine(lifetimeRoutine);
    }

    private IEnumerator LifetimeRoutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        OnExpire();
        Destroy(gameObject);
    }
}