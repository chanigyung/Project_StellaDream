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

    [Header("회전 옵션")]
    public bool followWhileHeld = false;
    public bool rotateWhileHeld = false;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new AreaHitboxModule(owner, this);
    }
}
