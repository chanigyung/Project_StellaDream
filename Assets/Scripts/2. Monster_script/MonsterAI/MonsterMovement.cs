using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MonsterMovement : MonoBehaviour, IMovementController // IInterruptable
{
    private MonsterAnimator monsterAnimator;
    public UnitController controller;
    private BaseUnitInstance instance => controller.instance as BaseUnitInstance;

    private bool isRooted = false;
    private bool isStunned = false;
    private bool isPowerKnockbacked = false;

    void Awake()
    {
        monsterAnimator = GetComponent<MonsterAnimator>();
        controller = GetComponent<UnitController>();
    }

    public void Move(Vector3 direction)
    {
        if (instance == null || isStunned || isPowerKnockbacked || isRooted || instance.IsKnockbackActive)
            return;

        float speed = instance.GetCurrentMoveSpeed();
        transform.position += direction * speed * Time.deltaTime;

        transform.localScale = (direction == Vector3.left) ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
    }

    public void ManualJump()
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
    }
    
    public void SetRooted(bool value) => isRooted = value;
    public void SetStunned(bool value) => isStunned = value;
    public void SetPowerKnockbacked(bool value) => isPowerKnockbacked = value;
}
