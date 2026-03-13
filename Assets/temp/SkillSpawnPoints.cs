using UnityEngine;

public class SkillSpawnPoints : MonoBehaviour, ISkillSpawnPointProvider
{
    public Transform centerPoint;
    public Transform leftPoint;
    public Transform rightPoint;
    public Transform groundPoint;

    public Transform GetPoint(SkillSpawnPointType type)
    {
        return type switch
        {
            SkillSpawnPointType.Center=> centerPoint,
            SkillSpawnPointType.Left => leftPoint,
            SkillSpawnPointType.Right => rightPoint,
            SkillSpawnPointType.Ground => groundPoint,
            _ => centerPoint
        };
    }

    // 추가: 공용 spawn point provider 인터페이스 대응
    public Vector3 GetWorldPoint(SkillSpawnPointType type)
    {
        Transform point = GetPoint(type);
        return point != null ? point.position : transform.position;
    }
}
