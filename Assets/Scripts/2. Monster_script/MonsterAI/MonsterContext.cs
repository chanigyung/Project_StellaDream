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

    public void UpdateContext()
    {
        // 현재 상태(위치, 상태이상 등) 업데이트 예정
    }
}
