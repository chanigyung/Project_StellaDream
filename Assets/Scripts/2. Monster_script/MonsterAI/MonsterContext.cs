using UnityEngine;

public class MonsterContext
{
    public Transform selfTransform;
    public GameObject target;
    public bool isPlayerDetected;
    public bool isStunned;
    public bool isRooted;
    public bool isKnockbacked;

    // 캐싱 컴포넌트 참조용 변수
    public MonsterMovement movement;
    public MonsterAnimator animator;
    public MonsterInstance instance;

    // update를 통해 계산된 변수들
    public Vector2 directionToTarget { get; private set; }
    public float distanceToTarget { get; private set; }
    public float targetDirectionX { get; private set; }
    public bool canMove { get; private set; }
    public bool canAct { get; private set; }

    public void UpdateContext()
    {
        // 현재 상태(위치, 상태이상 등) 업데이트
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
            targetDirectionX = 1f; // 기본값은 오른쪽
        }

        canMove = !isStunned && !isRooted && !isKnockbacked;
        canAct  = !isStunned && !isKnockbacked;
    }
}
