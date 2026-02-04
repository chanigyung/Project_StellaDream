using UnityEngine;

public class SkillSpawnPoints : MonoBehaviour
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
}
