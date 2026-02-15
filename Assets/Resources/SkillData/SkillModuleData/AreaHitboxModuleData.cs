using UnityEngine;

[CreateAssetMenu(menuName = "SkillModule/AreaHitbox")]
public class AreaHitboxModuleData : SkillModuleData
{
    [Header("Prefab")]
    public GameObject hitboxPrefab;

    [Header("Area Stats")]
    public Vector2 size = new Vector2(2f, 2f);
    public float duration = 2f;

    [Header("데미지 주기")]
    public float tickInterval = 0.5f;

    [Header("회전 옵션")]
    public bool followWhileHeld = false;
    public bool rotateWhileHeld = false;

    [Header("히트박스 이펙트")]
    public RuntimeAnimatorController hitboxAnimator;

    public override ISkillModule CreateModule(SkillInstance owner)
    {
        return new AreaHitboxModule(owner, this);
    }
}
