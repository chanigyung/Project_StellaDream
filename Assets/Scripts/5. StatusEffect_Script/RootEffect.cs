using UnityEngine;

public class RootEffect : StatusEffect
{
    private IMovementController movementController;

    public RootEffect(GameObject target, StatusEffectManager manager, GameObject attacker, float duration)
        : base(target, manager, attacker)
    {
        this.effectType = StatusEffectType.Root;
        this.duration = duration;
        this.icon = StatusEffectIconLibrary.Instance.rootSprite;
    }

    public override void Start()
    {
        if (target.TryGetComponent(out movementController))
        {
            movementController.SetRooted(true);
        }
        //이동기 사용중 피격 시 판정 추가 필요, 잔여시간 적용
    }

    public override void Expire()
    {
        if (movementController != null)
        {
            movementController.SetRooted(false);
        }
        base.Expire();
    }

    public override bool TryReplace(StatusEffect newEffect)
    {
        if (newEffect is RootEffect newRoot)
        {
            float remaining = duration - elapsedTime;
            return newRoot.duration > remaining;
        }

        return false;
    }
}
