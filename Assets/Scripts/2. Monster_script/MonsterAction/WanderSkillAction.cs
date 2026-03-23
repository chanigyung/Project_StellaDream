using UnityEngine;

public class WanderSkillAction : IMonsterAction
{
    private Vector3 moveDirection = Vector3.zero;
    private float directionChangeTimer = 0f;

    // TryUseSkill()을 매 프레임 호출하지 않기 위한 최소 인터벌(내부 쿨/거리/글로벌쿨은 SkillAI가 판정)
    private float tryInterval = 0.25f;
    private float tryTimer = 0f;

    public bool CanExecute(MonsterContext context)
    {
        if (context == null) return false;
        if (context.isTracing) return false;
        if (!context.canMove) return false;
        if (context.skillAI == null) return false;

        return true;
    }

    public void Execute(MonsterContext context)
    {
        if (context == null) return;

        // 1) 랜덤 이동 (기존 WanderAction 로직 유지)
        directionChangeTimer -= Time.deltaTime;
        if (directionChangeTimer <= 0f)
        {
            int choice = Random.Range(0, 3); // 0: 정지, 1: 왼쪽, 2: 오른쪽
            moveDirection = choice switch
            {
                0 => Vector3.zero,
                1 => Vector3.left,
                _ => Vector3.right,
            };

            directionChangeTimer = 2f;
        }

        if (moveDirection == Vector3.zero)
        {
            context.instance.selfSpeedMultiplier = 1f;
            context.movement?.Move(moveDirection);
        }
        else
        {
            context.facingDirectionX = Mathf.Sign(moveDirection.x);
            context.instance.selfSpeedMultiplier = 1f;
            context.movement?.Move(moveDirection);
        }

        // 2) 주기적으로 스킬 사용 시도
        tryTimer -= Time.deltaTime;
        if (tryTimer > 0f) return;
        tryTimer = tryInterval;

        // 변경: 이 Action 실행 중에는 스킬 사용 게이트를 강제로 열어준다.
        // (MonsterContext.UpdateContext()에서 canAttack이 매 프레임 isTracing으로 기본 세팅되므로, 여기서만 true로 덮어쓰기)
        context.ForceCanAttack(true);

        context.skillAI.TryUseSkill();
    }
}