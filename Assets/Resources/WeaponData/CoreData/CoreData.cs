using UnityEngine;

public enum CoreType
{
    Normal,
    Special
}

[CreateAssetMenu(menuName = "Core/CoreData")]
public class CoreData : ScriptableObject
{
    [Header("Equip")]
    public CoreType coreType = CoreType.Normal;
    public SkillTag primaryTag = SkillTag.Damage;

    [Header("Runtime Values")]
    public float damageMultiplier = 1f;
    public float cooldownMultiplier = 1f;
    public float hitboxSizeMultiplier = 1f;
    public float projectileSpeedMultiplier = 1f;
    public float lifetimeMultiplier = 1f;
    public int additionalHitCount = 0;
    public int pierceCount = 0;

    public CoreInstance CreateInstance()
    {
        return new CoreInstance(this);
    }

    public void ApplyValues(ref SkillContext context)
    {
        context.EnsureValues();

        context.values.damageMultiplier *= Mathf.Max(0f, damageMultiplier);
        context.values.cooldownMultiplier *= Mathf.Max(0f, cooldownMultiplier);
        context.values.hitboxSizeMultiplier *= Mathf.Max(0f, hitboxSizeMultiplier);
        context.values.projectileSpeedMultiplier *= Mathf.Max(0f, projectileSpeedMultiplier);
        context.values.lifetimeMultiplier *= Mathf.Max(0f, lifetimeMultiplier);
        context.values.additionalHitCount += additionalHitCount;
        context.values.pierceCount += pierceCount;
    }
}
