using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UnitMovement : MonoBehaviour
{
    private Rigidbody2D rigid;
    private UnitController unitController;
    private BaseUnitInstance instance => unitController?.instance as BaseUnitInstance;

    private float desiredDirX;
    private bool hasMoveInput;
    private bool jumped = false;

    private bool isGrounded = true;
    public bool IsGrounded => isGrounded;

    [SerializeField] private bool useInstantGroundMove = false;
    [SerializeField] private float accel = 30f;
    [SerializeField] private float airLerpFactor = 0.1f;

    private UnitContext context;

    private bool isMoveSkillRunning = false;
    private Vector2 moveSkillDirection;
    private float moveSkillSpeed = 0f;
    private float moveSkillRemainingTime = 0f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        unitController = GetComponent<UnitController>();
    }

    public void Initialize(UnitContext ctx)
    {
        context = ctx;
    }

    public void SetGrounded(bool value)
    {
        isGrounded = value;

        if (isGrounded)
            jumped = false;
    }

    public void Move(Vector3 direction)
    {
        if (!CanMoveNow())
        {
            hasMoveInput = false;
            desiredDirX = 0f;
            return;
        }

        float dirX = direction.x;
        if (Mathf.Abs(dirX) < 0.01f)
        {
            hasMoveInput = false;
            desiredDirX = 0f;
            return;
        }

        hasMoveInput = true;
        desiredDirX = Mathf.Sign(dirX);
    }

    // 이동 의도 제거 / 속도는 건드리지 않음
    public void ClearMoveInput()
    {
        hasMoveInput = false;
        desiredDirX = 0f;
    }

    public void Stop()
    {
        ClearMoveInput();

        if (rigid != null)
            rigid.velocity = new Vector2(0f, rigid.velocity.y);
    }

    public void TickMove()
    {
        if (rigid == null || instance == null)
            return;

        if (instance.IsKnockbackActive)
            return;

        float targetVelX = desiredDirX * instance.GetCurrentMoveSpeed();

        if (isGrounded)
        {
            if (useInstantGroundMove)
            {
                rigid.velocity = new Vector2(targetVelX, rigid.velocity.y);
            }
            else
            {
                float newVelX = Mathf.MoveTowards(rigid.velocity.x, targetVelX, accel * Time.fixedDeltaTime);
                rigid.velocity = new Vector2(newVelX, rigid.velocity.y);
            }
        }
        else
        {
            float newVelX = Mathf.Lerp(rigid.velocity.x, targetVelX, airLerpFactor);
            rigid.velocity = new Vector2(newVelX, rigid.velocity.y);
        }
    }

    public bool TryJump()
    {
        if (rigid == null || instance == null)
            return false;

        if (!CanJumpNow())
            return false;

        Jump();
        jumped = true;
        isGrounded = false;
        return true;
    }

    public void Jump()
    {
        if (rigid == null || instance == null)
            return;

        float jumpPower = instance.GetCurrentJumpPower();

        rigid.velocity = new Vector2(rigid.velocity.x, 0f);
        rigid.AddForce(new Vector2(0f, jumpPower), ForceMode2D.Impulse);
    }

    public bool HasMoveInput()
    {
        return hasMoveInput;
    }

    public float GetDesiredDirX()
    {
        return desiredDirX;
    }

    public bool CanMoveNow()
    {
        if (instance == null)
            return false;
        //일반 넉백
        if (unitController != null && instance.IsKnockbackActive)
            return false;

        if (context != null && context.isMoveSkillActive) // [추가]
            return false;

        return true;
    }

    public bool CanJumpNow()
    {
        if (!CanMoveNow())
            return false;

        if (!isGrounded)
            return false;

        if (jumped)
            return false;

        return true;
     }

     // ================================ 이동 스킬 관련 =========================== //
    public bool StartMoveSkill(Vector2 direction, float distance, float duration)
    {
        if (rigid == null || instance == null || context == null)
            return false;

        if (isMoveSkillRunning)
            return false;

        if (direction.sqrMagnitude <= 0.0001f)
            return false;

        if (distance <= 0f || duration <= 0f)
            return false;

        ClearMoveInput();
        rigid.velocity = Vector2.zero;

        isMoveSkillRunning = true;
        moveSkillDirection = direction.normalized;
        moveSkillSpeed = distance / duration;
        moveSkillRemainingTime = duration;

        context.isMoveSkillActive = true;
        context.UpdateContext();

        return true;
    }

    public void TickMoveSkill()
    {
        if (!isMoveSkillRunning)
            return;

        if (rigid == null || context == null)
        {
            EndMoveSkill();
            return;
        }

        float deltaTime = Time.fixedDeltaTime;
        Vector2 deltaMove = moveSkillDirection * moveSkillSpeed * deltaTime;

        rigid.MovePosition(rigid.position + deltaMove);
        rigid.velocity = Vector2.zero;

        moveSkillRemainingTime -= deltaTime;

        if (moveSkillRemainingTime <= 0f)
        {
            EndMoveSkill();
        }
    }

    private void EndMoveSkill()
    {
        isMoveSkillRunning = false;
        moveSkillDirection = Vector2.zero;
        moveSkillSpeed = 0f;
        moveSkillRemainingTime = 0f;

        if (rigid != null)
            rigid.velocity = Vector2.zero;

        if (context != null)
        {
            context.isMoveSkillActive = false;
            context.UpdateContext();
        }
    }

    // 이동스킬 강제 중지시킬 경우 호출
    public void StopMoveSkill()
    {
        if (!isMoveSkillRunning)
            return;

        EndMoveSkill();
    }
}