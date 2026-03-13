using UnityEngine;

public class DynamicHitboxSpawnPoints : MonoBehaviour, ISkillSpawnPointProvider
{
    [SerializeField] private BoxCollider2D targetCollider;

    private void Awake()
    {
        // 추가: 미지정 시 같은 오브젝트의 BoxCollider2D 자동 참조
        if (targetCollider == null)
            targetCollider = GetComponent<BoxCollider2D>();
    }

    public Vector3 GetWorldPoint(SkillSpawnPointType type)
    {
        if (targetCollider == null)
            return transform.position;

        Vector2 localPoint = GetLocalPoint(type);
        return transform.TransformPoint(localPoint);
    }

    // 추가: 현재 collider 크기/offset 기준으로 동적 기준점 계산
    public Vector2 GetLocalPoint(SkillSpawnPointType type)
    {
        if (targetCollider == null)
            return Vector2.zero;

        Vector2 size = targetCollider.size;
        Vector2 offset = targetCollider.offset;

        return type switch
        {
            SkillSpawnPointType.Center => offset,
            SkillSpawnPointType.Left => offset + new Vector2(-size.x * 0.5f, 0f),
            SkillSpawnPointType.Right => offset + new Vector2(size.x * 0.5f, 0f),
            SkillSpawnPointType.Ground => offset + new Vector2(0f, -size.y * 0.5f),
            _ => offset
        };
    }
}
