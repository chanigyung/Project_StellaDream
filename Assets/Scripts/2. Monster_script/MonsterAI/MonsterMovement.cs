using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MonsterMovement : MonoBehaviour
{
    private MonsterContext context;
    private BaseUnitInstance instance => context?.instance;
    [SerializeField] private UnitMovement unitMovement;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    private bool CanMoveNow()
    {
        if (unitMovement == null || !unitMovement.CanMoveNow())
            return false;

        return context != null && context.canMove;
    }

    public void Move(Vector3 direction)
    {
        if (!CanMoveNow())
        {
            unitMovement?.Stop();
            context.animator?.PlayMoving(false);
            return;
        }

        float dirX = direction.x;
        if (Mathf.Abs(dirX) < 0.01f)
        {
            unitMovement?.Stop();
            context.animator?.PlayMoving(false);
            return;
        }

        unitMovement?.Move(direction);

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

    //실제 이동
    private void FixedUpdate()
    {
        if (context == null || unitMovement == null)
            return;

        unitMovement.SetGrounded(context.isGrounded);
        unitMovement.TickMove();
    }

    public bool TryJump()
    {
        if (context == null || instance == null || unitMovement == null)
            return false;

        if (!context.isTracing)
            return false;

        if (!context.isGrounded)
            return false;

        if (!CanMoveNow())
            return false;

        return unitMovement.TryJump();
    }

    public void Jump()
    {
        unitMovement?.Jump();
    }

    public void Stop()
    {
        unitMovement?.Stop();
        context.animator?.PlayMoving(false);
    }
}
