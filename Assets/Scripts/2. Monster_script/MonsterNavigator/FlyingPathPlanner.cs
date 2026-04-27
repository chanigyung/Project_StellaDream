// 비행 몬스터의 공중 이동 경로 탐색을 담당할 Planner입니다.
public class FlyingPathPlanner : MonsterPathPlanner
{
    public override bool CanPlan(MonsterPathRequest request)
    {
        return context != null && request.moveType == MonsterMoveType.Flying;
    }

    public override bool TryFindPath(MonsterPathRequest request, out MonsterPathResult result)
    {
        result = MonsterPathResult.Failed();
        return false;
    }
}
