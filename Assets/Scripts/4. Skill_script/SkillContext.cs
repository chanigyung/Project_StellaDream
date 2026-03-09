using UnityEngine;

public struct SkillContext
{
    public SkillInstance skillInstance;

    public GameObject attacker;
    public GameObject contextOwner;
    public GameObject sourceObject;
    public GameObject targetObject;

    public Vector3 position;
    public Quaternion rotation;
    public Vector2 direction;
    public bool hasDirection;

    public SkillSpawnPointType spawnPointType;

    public SkillContext Clone()
    {
        return this;
    }
}
