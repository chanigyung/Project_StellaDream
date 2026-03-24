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

    private bool isRooted = false;
    private bool isStunned = false;
    private bool isPowerKnockbacked = false;

    private bool isGrounded = true;
    public bool IsGrounded => isGrounded;

    [SerializeField] private bool useInstantGroundMove = false;
    [SerializeField] private float accel = 30f;
    [SerializeField] private float airLerpFactor = 0.1f;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        unitController = GetComponent<UnitController>();
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

        if (isPowerKnockbacked || isStunned)
            return;

        if (unitController != null && unitController.BlockByKnockback() && instance.IsKnockbackActive)
            return;

        if (isPowerKnockbacked || isRooted || !hasMoveInput)
        {
            rigid.velocity = new Vector2(0f, rigid.velocity.y);
            return;
        }

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
        if (unitController != null && unitController.BlockByKnockback() && instance.IsKnockbackActive)
            return false;
        //상태이상
        if (isStunned || isPowerKnockbacked || isRooted)
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

    public void SetRooted(bool value) => isRooted = value;
    public void SetStunned(bool value) => isStunned = value;
    public void SetPowerKnockbacked(bool value) => isPowerKnockbacked = value;
}