using UnityEngine;

// 지상/비행 경로 탐색기가 공유하는 요청 처리 인터페이스 역할의 베이스 클래스입니다.
public abstract class MonsterPathPlanner : MonoBehaviour
{
    protected MonsterContext context;

    public virtual void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    public abstract bool CanPlan(MonsterPathRequest request);
    public abstract bool TryFindPath(MonsterPathRequest request, out MonsterPathResult result);
}
