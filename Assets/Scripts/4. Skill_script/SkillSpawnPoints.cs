using UnityEngine;

public class SkillSpawnPoints : MonoBehaviour
{
    public Transform leftArmPoint;
    public Transform rightArmPoint;
    public Transform groundPoint;

    public Transform GetPoint(SkillSpawnPointType type)
    {
        return type switch
        {
            SkillSpawnPointType.LeftArm => leftArmPoint,
            SkillSpawnPointType.RightArm => rightArmPoint,
            SkillSpawnPointType.GroundCenter => groundPoint,
            _ => leftArmPoint
        };
    }
}
