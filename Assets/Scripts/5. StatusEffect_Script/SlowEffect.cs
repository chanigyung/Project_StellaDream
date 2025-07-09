using Unity.VisualScripting;
using UnityEngine;

public class SlowEffect : StatusEffect
{
    private float slowRate; // 예: 0.3f → 30% 느려짐 → 실제 이동속도는 70%
    private float originalMultiplier = 1f;

    public SlowEffect(GameObject target, StatusEffectManager manager, GameObject attacker, float rate, float duration)
        : base(target, manager, attacker)
    {
        this.slowRate = Mathf.Clamp01(rate); // 0~1 사이 제한
        this.duration = duration;
        this.effectType = StatusEffectType.Slow;
        this.icon = StatusEffectIconLibrary.Instance.slowSprite;
    }

    public override void Start()
    {
        // 원래 이동속도 배율 저장 후 슬로우 적용
        if (target.TryGetComponent(out UnitController controller))
        {
            originalMultiplier = controller.instance.externalSpeedMultiplier;
            controller.instance.externalSpeedMultiplier = originalMultiplier * (1f - slowRate);
            Debug.Log("현재 이동 속도 : " + controller.instance.GetCurrentMoveSpeed()); ///////////////
        }
    }

    public override void Expire()
    {
        // 원래 이동속도 배율 복구
        if (target.TryGetComponent(out UnitController controller))
        {
            controller.instance.externalSpeedMultiplier = originalMultiplier;
        }
        Debug.Log("현재 이동 속도 : " + controller.instance.GetCurrentMoveSpeed()); ///////////////

        base.Expire();
    }

    public override bool TryReplace(StatusEffect newEffect)
    {
        if (newEffect is SlowEffect incoming)
        {
            if (incoming.slowRate > this.slowRate)
            {
                Debug.Log("강한 슬로우로 교체");
                return true; // 더 강한 슬로우가 들어오면 교체
            }
            else if (Mathf.Approximately(incoming.slowRate, this.slowRate))
            {
                this.elapsedTime = 0f; // 동일 비율일 경우 시간만 갱신
                return false;
            }
            else
            {
                Debug.Log("더 약한 슬로우 무시");
                return false;
            }
        }
        return false;
    }
}