using UnityEngine;

public class UnitContext
{
    // === 캐싱 컴포넌트 참조 ===
    public Transform selfTransform;
    public GameObject target;
    public BaseUnitInstance instance;
    public Transform selfGroundPoint;
    public UnitMovement unitMovement;

    // === 상태이상 / 행동 차단 관련 ===
    public bool isStunned;
    public bool isRooted;
    public bool isKnockbacked;
    public bool isCastingSkill = false;

    // === 센서 관련 변수 ===
    public bool isGrounded = false;
    public bool hasWallAhead = false;
    public float facingDirectionX = 1f;
    public bool hasGroundLeft = false;
    public bool hasGroundRight = false;

    // === 업데이트 계산 변수 ===
    public Vector2 directionToTarget { get; protected set; }
    public float distanceToTarget { get; protected set; }
    public float targetDirectionX { get; protected set; }
    public bool canMove { get; protected set; }
    public bool canAct { get; protected set; }
    public bool canAttack { get; protected set; }

    public virtual void UpdateContext()
    {
        isKnockbacked = instance != null && instance.IsKnockbackActive;

        if (target != null)
        {
            Vector2 delta = target.transform.position - selfTransform.position;
            directionToTarget = delta.normalized;
            distanceToTarget = delta.magnitude;
            targetDirectionX = Mathf.Sign(delta.x);
        }
        else
        {
            directionToTarget = Vector2.zero;
            distanceToTarget = float.MaxValue;
            targetDirectionX = 1f;
        }
    }

    public void ForceCanAttack(bool value) // [추가] WanderSkillAction 같은 예외용
    {
        canAttack = value;
    }
}