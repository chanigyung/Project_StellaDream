using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/Object/Hitbox")]
public class HitboxModuleData : SkillModuleData
{
    public string objectId;

    public GameObject hitboxPrefab;
    public Vector2 hitboxSize = Vector2.one;
    public Vector2 spawnOffset = Vector2.zero;
    public float lifetime = 0.2f;

    public SkillSpawnPointType ownerSpawnPointType = SkillSpawnPointType.Left;
    public SkillSpawnPointType prefabSpawnPointType = SkillSpawnPointType.Left;

    public bool followOwner = false;

    public hitEffect hitEffect = new hitEffect();

    protected virtual void OnEnable()
    {
        EnsureTags(SkillTag.Hitbox, SkillTag.Damage);
    }

    protected virtual void OnValidate()
    {
        EnsureTags(SkillTag.Hitbox, SkillTag.Damage);
    }

    public override ISkillModule CreateModule()
    {
        return new HitboxModule(this);
    }
}
