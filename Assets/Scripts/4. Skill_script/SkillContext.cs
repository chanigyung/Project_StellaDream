using UnityEngine;
using System.Collections.Generic;

public static class SkillObjectTags
{
    public static readonly IReadOnlyList<SkillTag> VFX = new[] { SkillTag.VFX };
}

public enum SkillInputSlot
{
    None,
    Main,
    Sub,
    Extra
}

public enum SkillInputPhase
{
    None,
    Pressed,
    Released
}

public class SkillRuntimeValues
{
    public float damageMultiplier = 1f;
    public float cooldownMultiplier = 1f;
    public float hitboxSizeMultiplier = 1f;
    public float projectileSpeedMultiplier = 1f;
    public float lifetimeMultiplier = 1f;

    public int additionalHitCount = 0;
    public int pierceCount = 0;

    public SkillRuntimeValues Clone()
    {
        return new SkillRuntimeValues
        {
            damageMultiplier = damageMultiplier,
            cooldownMultiplier = cooldownMultiplier,
            hitboxSizeMultiplier = hitboxSizeMultiplier,
            projectileSpeedMultiplier = projectileSpeedMultiplier,
            lifetimeMultiplier = lifetimeMultiplier,
            additionalHitCount = additionalHitCount,
            pierceCount = pierceCount
        };
    }
}

public struct SkillContext
{
    public SkillInstance skillInstance;
    public WeaponInstance weaponInstance;

    public SkillInputSlot inputSlot;
    public SkillInputPhase inputPhase;

    public GameObject attacker;
    public GameObject contextOwner;
    public GameObject sourceObject;
    public GameObject targetObject;

    public Vector3 position;
    public Vector3 groundPoint;
    public Vector3 leftPoint;
    public Vector3 rightPoint; 

    public Quaternion rotation;
    public Vector2 direction;
    public bool hasDirection;

    public SkillSpawnPointType spawnPointType;
    public SkillRuntimeValues values;

    public void EnsureValues()
    {
        values ??= new SkillRuntimeValues();
    }

    public SkillContext Clone()
    {
        SkillContext clone = this;
        clone.values = values?.Clone();
        return clone;
    }
}
