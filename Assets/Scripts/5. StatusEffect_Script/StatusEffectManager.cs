using System.Collections.Generic;
using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private StatusEffectUIController uiController;

    private List<StatusEffect> activeEffects = new List<StatusEffect>();    

    // 면역 플래그들 (임시 예시)
    public bool immuneToStun = false;
    public bool immuneToRoot = false;
    public bool immuneToSlow = false;
    public bool immuneToPowerKnockback = false;
    public bool immuneToBleed = false;
    public bool immuneToIgnite = false;
    public bool immuneToPoison = false;

    private void Awake()
    {
        uiController = GetComponentInChildren<StatusEffectUIController>();
    }

    public void ApplyEffect(StatusEffect newEffect)
    {
        if (IsImmuneTo(newEffect.effectType))
            return;

        if (newEffect.effectType == StatusEffectType.Bleed)
        {
            newEffect.Start();
            activeEffects.Add(newEffect);
            uiController?.AddEffectIcon(newEffect);
            return;
        }

        StatusEffect existing = activeEffects.Find(e => e.effectType == newEffect.effectType);
        if (existing != null)
        {
            if (existing.TryReplace(newEffect))
            {
                existing.Expire();
                activeEffects.Remove(existing);

                newEffect.Start();
                activeEffects.Add(newEffect);
                uiController?.AddEffectIcon(newEffect); // 새 효과니까 아이콘 생성
            }
            else
            {
                // 기존 인스턴스 유지 → 아이콘만 갱신
                uiController?.UpdateEffectProgress(existing.effectType, existing.GetElapsedTime(), existing.duration);
                uiController?.RefreshIcon(existing);
            }
        return;
        }
        newEffect.Start();
        activeEffects.Add(newEffect);
        uiController?.AddEffectIcon(newEffect);
    }

    public void RemoveEffect(StatusEffect effect)
    {
        activeEffects.Remove(effect);
        uiController?.RemoveEffectIcon(effect);
    }

    public List<StatusEffect> GetActiveEffects() => activeEffects;

    public bool IsImmuneTo(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Stun: return immuneToStun;
            case StatusEffectType.Root: return immuneToRoot;
            case StatusEffectType.Slow: return immuneToSlow;
            case StatusEffectType.PowerKnockback: return immuneToPowerKnockback;
            case StatusEffectType.Bleed: return immuneToBleed;
            case StatusEffectType.Ignite: return immuneToIgnite;
            case StatusEffectType.Poison: return immuneToPoison;
            default: return false;
        }
    }

    private void Update()
    {
        //리스트 비어있으면 리턴? 효과 받은 이후에만 켜주기?
        foreach (var effect in activeEffects.ToArray())
        {
            effect.Update(Time.deltaTime);

            uiController?.UpdateEffectProgress(effect.effectType, effect.GetElapsedTime(), effect.duration);
        }
    }

}
