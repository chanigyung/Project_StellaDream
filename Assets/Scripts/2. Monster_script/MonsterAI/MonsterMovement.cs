using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MonsterMovement : MonoBehaviour, IMovementController // IInterruptable
{
    private MonsterContext context;
    private BaseUnitInstance instance => context?.instance;

    private Rigidbody2D rigid;

    private float desiredDirX;
    private bool hasMoveInput;

    private bool isRooted = false;
    private bool isStunned = false;
    private bool isPowerKnockbacked = false;

    // 이동속도 목표값에 도달하는 속도 제어를 위한 변수
    // 클수록 목표 이동속도에 도달하는 속도가 빨라짐. *(이속2 기준 20이면 0.1초)
    // 추후 monsterInstance 또는 monsterContext등과 연계될 수 있도록 외부 변수로 뺄 것
    [SerializeField] private float accel = 30f;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    private bool CanMoveNow()
    {
        if (instance == null || isStunned || isPowerKnockbacked || isRooted || instance.IsKnockbackActive)
            return false;

        return context != null && context.canMove;
    }

    public void Move(Vector3 direction)
    {
        if (!CanMoveNow())
        {
            hasMoveInput = false;
            desiredDirX = 0f;
            context.animator?.PlayMoving(false);
            return;
        }

        float dirX = direction.x;
        if (Mathf.Abs(dirX) < 0.01f)
        {
            hasMoveInput = false;
            desiredDirX = 0f;
            context.animator?.PlayMoving(false);
            return;
        }

        hasMoveInput = true;
        desiredDirX = Mathf.Sign(dirX);

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
        if (rigid == null)
            return;

        if (!CanMoveNow() || !hasMoveInput)
        {
            rigid.velocity = new Vector2(0f, rigid.velocity.y);
            return;
        }

        float targetVelX = 0f;

        if (CanMoveNow() && hasMoveInput)
        {
            float speed = instance.GetCurrentMoveSpeed();
            targetVelX = desiredDirX * speed;
        }

        float newVelX = Mathf.MoveTowards(rigid.velocity.x, targetVelX, accel * Time.fixedDeltaTime);
        rigid.velocity = new Vector2(newVelX, rigid.velocity.y);
    }


    public void Jump()
    {
        if (instance == null || isStunned || isPowerKnockbacked || isRooted || instance.IsKnockbackActive)
            return;

        float jumpPower = instance.GetCurrentJumpPower();

        rigid.velocity = new Vector2(rigid.velocity.x, 0);
        rigid.AddForce(new Vector2(0, jumpPower), ForceMode2D.Impulse);
    }

    public void Stop()
    {
        hasMoveInput = false;
        desiredDirX = 0f;

        if (rigid != null)
            rigid.velocity = new Vector2(0f, rigid.velocity.y);

        context.animator?.PlayMoving(false);
    }
    
    public void SetRooted(bool value) => isRooted = value;
    public void SetStunned(bool value)
    {
        isStunned = value;
        context.animator?.PlayStunned(value);
    }
    public void SetPowerKnockbacked(bool value) => isPowerKnockbacked = value;
}
