using UnityEngine;

public class TraceAction : IMonsterAction
{
    private const float JumpTriggerHeight = 0.8f; //점프 판정용, 추후 수정

    public bool CanExecute(MonsterContext context)
    {
        // 추적 중이고 이동 가능한 상태여야 실행 가능
        return context.isTracing && context.canMove;
    }

    public void Execute(MonsterContext context)
    {
        Vector3 moveDirection = context.directionToTarget.x < 0 ? Vector3.left : Vector3.right;

        context.movement?.Move(moveDirection);

        UnitController targetUnit = context.target.GetComponent<UnitController>();

        if (targetUnit != null && context.selfGroundPoint != null)
        {
            float deltaY = targetUnit.GroundPoint.position.y - context.selfGroundPoint.position.y;

            if (deltaY > JumpTriggerHeight)
                context.movement?.TryJump();
        }
    }
}
