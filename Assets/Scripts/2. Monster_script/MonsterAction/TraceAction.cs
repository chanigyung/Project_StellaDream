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
        if (context.target != null)
        {
            float deltaY = context.target.transform.position.y - context.selfTransform.position.y;
            if (deltaY > JumpTriggerHeight)
                context.movement?.TryJump();
        }

        Vector3 moveDirection = context.directionToTarget.x < 0 ? Vector3.left : Vector3.right;

        context.movement?.Move(moveDirection);
        // 걷기/추적 애니메이션은 MonsterMovement.Move() 내부에서 처리
    }
}
