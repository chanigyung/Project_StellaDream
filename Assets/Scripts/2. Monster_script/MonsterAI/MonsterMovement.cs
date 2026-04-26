using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MonsterMovement : MonoBehaviour
{
    private MonsterContext context;
    private BaseUnitInstance instance => context?.instance;
    private Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    public void Move(Vector3 direction)
    {
        MoveGround(direction);
    }

    public void MoveGround(Vector3 direction)
    {
        if (context == null || context.unitMovement == null || !context.canMove || !context.unitMovement.CanMoveNow())
        {
            context.animator?.PlayMoving(false);
            return;
        }

        float dirX = direction.x;
        if (Mathf.Abs(dirX) < 0.01f)
        {
            context.unitMovement?.Stop();
            context.animator?.PlayMoving(false);
            return;
        }

        context.unitMovement?.Move(direction);

        float desiredDirX = Mathf.Sign(dirX);

        if (context != null)
            context.facingDirectionX = desiredDirX;

        context.selfTransform.localScale = (desiredDirX < 0f) ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        context.animator?.PlayMoving(true);

        if (instance.selfSpeedMultiplier > 1.2f)
            context.animator?.PlayTracing(true);
        else
            context.animator?.PlayTracing(false);
    }

    public void MoveFlying(Vector2 direction, float speedMultiplier = 1f)
    {
        if (context == null || !context.canMove)
        {
            context?.animator?.PlayMoving(false);
            return;
        }

        if (rigid == null || instance == null)
            return;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            StopFlying();
            return;
        }

        Vector2 normalized = direction.normalized;
        float moveSpeed = instance.GetCurrentMoveSpeed() * Mathf.Max(0f, speedMultiplier);
        rigid.velocity = normalized * moveSpeed;

        if (Mathf.Abs(normalized.x) > 0.01f)
        {
            float desiredDirX = Mathf.Sign(normalized.x);
            context.facingDirectionX = desiredDirX;
            context.selfTransform.localScale = (desiredDirX < 0f) ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        }

        context.animator?.PlayMoving(true);
        context.animator?.PlayTracing(speedMultiplier > 1.2f);
    }

    //실제 이동
    private void FixedUpdate()
    {
        if (context == null || context.unitMovement == null)
            return;

        if (context.isMoveSkillActive)
        {
            context.unitMovement.TickMoveSkill();
            return;
        }

        if (context.isFlyingMonster)
            return;

        context.unitMovement.SetGrounded(context.isGrounded);
        context.unitMovement.TickMove();
    }

    public void ClearMove()
    {
        if (context != null && context.isFlyingMonster)
        {
            StopFlying();
            return;
        }

        context.unitMovement?.ClearMoveInput();
        context.animator?.PlayMoving(false);
    }

    public bool TryJump()
    {
        if (context == null || instance == null || context.unitMovement == null)
            return false;

        if (!context.isTracing)
            return false;

        if (!context.isGrounded)
            return false;

        if (!context.canMove || !context.unitMovement.CanMoveNow())
            return false;

        return context.unitMovement.TryJump();
    }

    public void Jump()
    {
        context.unitMovement?.Jump();
    }

    public void Stop()
    {
        if (context != null && context.isFlyingMonster)
        {
            StopFlying();
            return;
        }

        context.unitMovement?.Stop();
        context.animator?.PlayMoving(false);
    }

    public void StopFlying()
    {
        if (rigid != null)
            rigid.velocity = Vector2.zero;

        context?.animator?.PlayMoving(false);
        context?.animator?.PlayTracing(false);
    }
}
