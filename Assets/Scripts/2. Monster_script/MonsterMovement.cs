using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MonsterMovement : MonoBehaviour, IInterruptable, IMovementController
{
    private MonsterAnimator monsterAnimator;
    public UnitController controller;
    private BaseUnitInstance instance => controller.instance as BaseUnitInstance;

    int movementFlag = 0;   //0:idle, -1:left, 1:right
    public bool isTracing;  //추적 상태인지 판정에 위해 사용
    GameObject traceTarget; //추적 대상

    private bool isRooted = false;
    private bool isStunned = false;
    private bool isPowerKnockbacked = false;

    void Awake()
    {
        monsterAnimator = GetComponent<MonsterAnimator>();
        controller = GetComponent<UnitController>();
    }

    void Start()
    {
        StartCoroutine("ChangeMovement");
    }

    void FixedUpdate()
    {
        if (instance.IsKnockbackActive) return;
        if (isStunned || isPowerKnockbacked)
        {
            return;
        }
            
        Move();
        Jump();
    }

    IEnumerator ChangeMovement()
    {
        movementFlag = Random.Range(-1, 2); //랜덤으로 -1부터 3개의 상태 중으로 하나로 바꿈
        monsterAnimator.PlayMoving(movementFlag != 0); //움직일 때만 걷기 모션
        yield return new WaitForSeconds(3f); //3초마다 멈추기를 반복
        StartCoroutine("ChangeMovement");   //코루틴을 다시 재생
    }

    void Move()
    {
        if (isRooted) return;
        Vector3 moveVelocity = Vector3.zero; //움직임 벡터값 초기화
        string dist = "";

        if (isTracing)   //추적할 때 방향 체크
        {
            Vector3 playerPos = traceTarget.transform.position;

            if (playerPos.x < transform.position.x)
                dist = "Left";
            else if (playerPos.x > transform.position.x)
                dist = "Right";

            instance.selfSpeedMultiplier = 2f;
        }
        else
        {
            if (movementFlag == -1)  //추적 끝나도 어색하지 않게 해줌
                dist = "Left";
            else if (movementFlag == 1)
                dist = "Right";

            instance.selfSpeedMultiplier = 1f;
        }

        if (dist == "Left")
        {
            moveVelocity = Vector3.left;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (dist == "Right")
        {
            moveVelocity = Vector3.right;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        float speed = instance.GetCurrentMoveSpeed();
        transform.position += moveVelocity * speed * Time.deltaTime;
    }

    void Jump()
    {
        if (isRooted) return;
    }

    public void SetTraceTarget(GameObject target)
    {
        traceTarget = target;
        StopCoroutine("ChangeMovement");
    }

    public void SetTracing(bool value)
    {
        isTracing = value;
        monsterAnimator.PlayTracing(value);

        if (!value)
            StartCoroutine("ChangeMovement");
    }

    /*-------------------------------------------이동 제어 관련 로직------------------------------------------------------*/
    public void SetRooted(bool value) => isRooted = value;
    public void SetStunned(bool value)
    {
        isStunned = value;

        monsterAnimator.PlayStunned(value);

        if (!value)
        {
            GetComponent<MonsterSkillAI>()?.NotifyRecoverDelay();
        }
    }

    public void SetPowerKnockbacked(bool value)
    {
        isPowerKnockbacked = value;

        monsterAnimator.PlayStunned(value);

        if (!value)
        {
            GetComponent<MonsterSkillAI>()?.NotifyRecoverDelay();
        }
    }
    
    public void Interrupt()
    {
        // 몬스터가 현재 도주/추적/이동/점프 중이라면 중단
        isTracing = false;
        StopAllCoroutines(); // 예: 코루틴 기반 행동 패턴이라면 중단

        Debug.Log($"{gameObject.name}의 행동이 기절로 중단됨");
    }
}
