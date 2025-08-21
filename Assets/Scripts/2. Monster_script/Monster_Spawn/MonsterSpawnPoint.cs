using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    [Header("스폰 태그")]
    public string monsterTag;

    [Header("생성 위치에서의 오프셋")]
    public Vector2 spawnOffset;

    [Header("디버그")]
    public Color gizmoColor = Color.red;

    public Vector2 GetSpawnPosition()
    {
        return (Vector2)transform.position + spawnOffset;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(GetSpawnPosition(), 0.3f);
    }
}
