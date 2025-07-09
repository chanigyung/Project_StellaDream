using System.Collections.Generic;
using UnityEngine;

public class StatusEffectUIController : MonoBehaviour
{
    [SerializeField] private GameObject iconPrefab; // 프리팹 참조
    [SerializeField] private Transform iconParent;  // GridLayoutGroup 등 사용

    private List<StatusEffectIcon> icons = new();

    public void AddEffectIcon(StatusEffect effect)
    {
        var type = effect.effectType;

        // 아이콘 없는 효과 제외
        if (type == StatusEffectType.PowerKnockback)
            return;

        // Bleed만 여러 개 허용
        if (type != StatusEffectType.Bleed)
        {
            var existing = icons.Find(icon => icon.EffectType == type);
            if (existing != null)
            {
                existing.Refresh(effect);
                return;
            }
        }

        // 아이콘 생성
        GameObject obj = Instantiate(iconPrefab, iconParent);
        var icon = obj.GetComponent<StatusEffectIcon>();
        icon.Initialize(effect);
        icons.Add(icon);
    }

    public void RemoveEffectIcon(StatusEffect effect)
    {
        var target = icons.Find(icon => icon.Matches(effect));
        if (target != null)
        {
            icons.Remove(target);
            Destroy(target.gameObject);
        }
    }

    public void RefreshIcon(StatusEffect effect)
    {
        foreach (var icon in icons)
        {
            if (icon.EffectType == effect.effectType)
            {
                icon.Refresh(effect);
                return;
            }
        }
    }

    public void UpdateEffectProgress(StatusEffectType type, float elapsed, float duration)
    {
        foreach (var icon in icons)
        {
            if (icon.EffectType == type)
            {
                icon.UpdateProgress(elapsed, duration);
            }
        }
    }
}
