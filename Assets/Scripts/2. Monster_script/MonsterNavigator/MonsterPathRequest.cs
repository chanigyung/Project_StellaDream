using UnityEngine;

public enum MonsterNavigationPurpose
{
    Trace,
    Wander,
}

public enum MonsterPathEdgeType
{
    Walk,
    Jump,
    Drop,
    Fly,
}

// Input data passed from the navigator to a path planner.
public struct MonsterPathRequest
{
    public MonsterMoveType moveType;
    public MonsterNavigationPurpose purpose;
    public Vector2 start;
    public Vector2 destination;
    public float preferredDirectionX;
    public float speedMultiplier;
    public bool directPathBlocked;
    public GameObject target;
}
