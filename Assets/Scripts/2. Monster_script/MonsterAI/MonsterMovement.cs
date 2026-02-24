using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MonsterMovement : MonoBehaviour, IMovementController // IInterruptable
{
    private MonsterContext context;
    private BaseUnitInstance instance => context?.instance;

    private Rigidbody2D rigid;

    // move 관련 변수
    private float desiredDirX;
    private bool hasMoveInput;

    // jump 관련 변수
    private bool jumped = false;

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

        // float newVelX = Mathf.MoveTowards(rigid.velocity.x, targetVelX, accel * Time.fixedDeltaTime);
        // rigid.velocity = new Vector2(newVelX, rigid.velocity.y);

        bool grounded = (context != null && context.isGrounded);

        if (grounded)
        {
            float newVelX = Mathf.MoveTowards(rigid.velocity.x, targetVelX, accel * Time.fixedDeltaTime);
            rigid.velocity = new Vector2(newVelX, rigid.velocity.y);
        }
        else
        {
            float newVelX = Mathf.Lerp(rigid.velocity.x, targetVelX, 0.1f);
            rigid.velocity = new Vector2(newVelX, rigid.velocity.y);
        }

        // 점프, 착지시 점프변수 초기화
        if (context != null && context.isGrounded)
            jumped = false;
    }

    public bool TryJump()
    {
        if (context == null || instance == null)
            return false;

        // trace 상태에서만 점프(사양)
        if (!context.isTracing)
            return false;

        if (!context.isGrounded)
            return false;

        if (jumped)
            return false;

        if (isStunned || isPowerKnockbacked || isRooted || instance.IsKnockbackActive)
            return false;

        Jump();
        jumped = true;
        return true;
    }

    public void Jump()
    {
        if (rigid == null || instance == null)
            return;

        if (isStunned || isPowerKnockbacked || isRooted || instance.IsKnockbackActive)
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
