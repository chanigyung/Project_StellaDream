using UnityEngine;

public abstract class StatusEffect
{
    public StatusEffectType effectType;
    public Sprite icon;
    public float duration;

    protected float elapsedTime;
    protected GameObject target;
    protected StatusEffectManager manager;
    protected GameObject attacker;

    private bool isPaused = false;

    public StatusEffect(GameObject target, StatusEffectManager manager, GameObject attacker)
    {
        this.target = target;
        this.manager = manager;
        this.attacker = attacker;
    }

    public virtual void Start() { }

    public virtual void Update(float deltaTime)
    {
        if (isPaused) return;

        elapsedTime += deltaTime;
        if (elapsedTime >= duration)
            Expire();
    }

    public virtual void Expire()
    {
        Debug.Log(effectType + " 효과 지속시간 종료");
        manager.RemoveEffect(this);
    }

    public virtual void Pause() => isPaused = true;
    public virtual void Resume() => isPaused = false;

    public abstract bool TryReplace(StatusEffect newEffect); // 중복 시 교체 여부

    public float GetElapsedTime() => elapsedTime;
    public virtual int GetStackCount() => 1;
}
