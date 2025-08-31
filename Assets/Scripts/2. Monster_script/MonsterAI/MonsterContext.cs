using UnityEngine;

public class MonsterContext : MonoBehaviour
{
    public Transform selfTransform;
    public GameObject target;
    public bool isPlayerDetected;
    public bool isStunned;
    public bool isRooted;
    public bool isKnockbacked;

    public void UpdateContext()
    {
        // 현재 상태(위치, 상태이상 등) 업데이트 예정
    }
}
