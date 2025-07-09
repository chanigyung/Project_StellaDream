using UnityEngine;

public class StunEffect : StatusEffect
{
    private IMovementController movementController;

    public StunEffect(GameObject target, StatusEffectManager manager, GameObject attacker, float duration)
        : base(target, manager, attacker)
    {
        this.effectType = StatusEffectType.Stun;
        this.duration = duration;
        this.icon = StatusEffectIconLibrary.Instance.stunSprite;
    }

    public override void Start()
    {
        if (target.TryGetComponent(out movementController))
        {
            movementController.SetStunned(true);
        }
    }

    public override void Expire()
    {
        if (movementController != null)
        {
            movementController.SetStunned(false);
        }
        base.Expire();
    }

    public override bool TryReplace(StatusEffect newEffect)
    {
        if (newEffect is StunEffect newStun)
        {
            float remaining = duration - elapsedTime;
            return newStun.duration > remaining;
        }
        return false;
    }
}
