using UnityEngine;

public enum MonsterMoveType
{
    Ground,
    Flying,
}

// Ground/flying movement command produced by the navigator.
public struct MonsterMoveCommand
{
    public MonsterMoveType moveType;
    public float groundDirectionX;
    public Vector2 flyingDirection;
    public float speedMultiplier;
    public bool shouldJump;
    public bool shouldStop;
    public bool allowDrop;
    public bool reachedDestination;

    public static MonsterMoveCommand Stop(MonsterMoveType moveType)
    {
        return new MonsterMoveCommand
        {
            moveType = moveType,
            shouldStop = true,
            speedMultiplier = 1f,
        };
    }

    public static MonsterMoveCommand Ground(float directionX, bool shouldJump = false, bool allowDrop = false)
    {
        return new MonsterMoveCommand
        {
            moveType = MonsterMoveType.Ground,
            groundDirectionX = Mathf.Abs(directionX) > 0.01f ? Mathf.Sign(directionX) : 0f,
            shouldJump = shouldJump,
            allowDrop = allowDrop,
            speedMultiplier = 1f,
        };
    }

    public static MonsterMoveCommand Flying(Vector2 direction, float speedMultiplier)
    {
        return new MonsterMoveCommand
        {
            moveType = MonsterMoveType.Flying,
            flyingDirection = direction,
            speedMultiplier = Mathf.Max(0f, speedMultiplier),
        };
    }
}
