using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/AreaHitbox")]
public class AreaHitboxModuleData : SkillModuleData
{
    [Header("Prefab")]
    public GameObject hitboxPrefab;

    [Header("Area Stats")]
    public Vector2 size = new Vector2(2f, 2f);
    public float duration = 2f;

    [Header("Damage")]
    public float tickInterval = 0.5f;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new AreaHitboxModule(owner, this);
    }
}
