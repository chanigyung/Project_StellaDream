using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Hitbox")]
public class HitboxModuleData : SkillModuleData
{
    public GameObject hitboxPrefab;
    public Vector2 hitboxSize = Vector2.one;
    public Vector2 spawnOffset = Vector2.zero;
    public float lifetime = 0.2f;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new HitboxModule(owner, this);
    }
}
