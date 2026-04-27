using System.Collections.Generic;
using UnityEngine;

// Single destination point and movement edge type produced by a path planner.
public struct MonsterPathWaypoint
{
    public Vector2 position;
    public MonsterPathEdgeType edgeType;

    public MonsterPathWaypoint(Vector2 position, MonsterPathEdgeType edgeType)
    {
        this.position = position;
        this.edgeType = edgeType;
    }
}

// Path calculation result returned from a planner to the navigator.
public struct MonsterPathResult
{
    public bool success;
    public MonsterMoveCommand nextCommand;
    public List<MonsterPathWaypoint> waypoints;

    public static MonsterPathResult Failed()
    {
        return new MonsterPathResult
        {
            success = false,
            nextCommand = default,
            waypoints = null,
        };
    }

    public static MonsterPathResult FromCommand(MonsterMoveCommand command, List<MonsterPathWaypoint> waypoints = null)
    {
        return new MonsterPathResult
        {
            success = true,
            nextCommand = command,
            waypoints = waypoints,
        };
    }
}
