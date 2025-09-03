using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MonsterMovement : MonoBehaviour, IMovementController // IInterruptable
{
    private MonsterContext context;
    private BaseUnitInstance instance => context?.instance;

    private bool isRooted = false;
    private bool isStunned = false;
    private bool isPowerKnockbacked = false;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    public void Move(Vector3 direction)
    {
        if (instance == null || isStunned || isPowerKnockbacked || isRooted || instance.IsKnockbackActive)
            return;

        float speed = instance.GetCurrentMoveSpeed();
        context.selfTransform.position += direction * speed * Time.deltaTime;

        context.selfTransform.localScale = (direction == Vector3.left) ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
        context.animator?.PlayMoving(true);
    }

    public void Jump()
    {
        if (instance == null || isStunned || isPowerKnockbacked || isRooted || instance.IsKnockbackActive)
            return;

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        float jumpPower = instance.GetCurrentJumpPower();

        rigid.velocity = new Vector2(rigid.velocity.x, 0);
        rigid.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
    }

    public void Stop()
    {
        // Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        // if (rigid != null)
        //     rigid.velocity = new Vector2(0, rigid.velocity.y);
        context.animator?.PlayMoving(false);
    }
    
    public void SetRooted(bool value) => isRooted = value;
    public void SetStunned(bool value) => isStunned = value;
    public void SetPowerKnockbacked(bool value) => isPowerKnockbacked = value;
}
