using UnityEngine;

public struct SkillContext
{
    public GameObject attacker;
    public GameObject contextOwner;
    public GameObject sourceObject;
    public GameObject targetObject;

    public Vector3 position;
    public Quaternion rotation;
    public Vector2 direction;
    public bool hasDirection;

    public SkillSpawnPointType spawnPointType;

    // [추가] 현재 컨텍스트를 복사한 뒤 일부 값만 교체해서 파생 컨텍스트를 만들 때 사용
    public SkillContext Clone()
    {
        return this;
    }
}
