using UnityEngine;

public interface ISkillSpawnPointProvider
{
    Vector3 GetWorldPoint(SkillSpawnPointType type);
}
