using UnityEngine;

public struct MonsterTraceMovePlan
{
    public bool shouldMove;
    public Vector3 moveDirection;
    public bool shouldJump;
    public bool shouldStop;

    public static MonsterTraceMovePlan Stop()
    {
        return new MonsterTraceMovePlan
        {
            shouldStop = true,
            shouldMove = false,
            shouldJump = false,
            moveDirection = Vector3.zero
        };
    }

    public static MonsterTraceMovePlan Move(Vector3 direction, bool jump = false)
    {
        return new MonsterTraceMovePlan
        {
            shouldStop = false,
            shouldMove = true,
            shouldJump = jump,
            moveDirection = direction
        };
    }
}
