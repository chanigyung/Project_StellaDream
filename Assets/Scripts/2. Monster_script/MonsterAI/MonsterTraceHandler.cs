using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterTraceHandler : MonoBehaviour
{
    [SerializeField] private float traceReleaseDelay = 1f;

    private MonsterContext context;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !context.isTracing)
        {
            BeginTrace();
        }
        else if (other.CompareTag("Player") && context.isTraceReleasedPending)
        {
            // 재진입 시 추적 해제 대기 취소
            context.isTraceReleasedPending = false;
            context.traceReleaseTimer = 0f;
            context.instance.selfSpeedMultiplier = 1.5f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !context.isTracePermanent)
        {
            context.isTraceReleasedPending = true;
            context.traceReleaseTimer = traceReleaseDelay;
            context.instance.selfSpeedMultiplier = 1.3f; // 조금 느려진 속도로 계속 추적
        }
    }

    private void Update()
    {
        if (context == null || !context.isTraceReleasedPending) return;

        context.traceReleaseTimer -= Time.deltaTime;
        if (context.traceReleaseTimer <= 0f)
        {
            EndTrace();
        }
    }

    public void NotifyDamaged()
    {
        if (!context.isTracing)
            BeginTrace();

        context.isTracePermanent = true;
        context.isTraceReleasedPending = false;
    }

    private void BeginTrace()
    {
        context.isTracing = true;
        context.isTraceReleasedPending = false;
        context.traceReleaseTimer = 0f;
        context.instance.selfSpeedMultiplier = 1.5f;

        if (context.target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            context.target = player;
        }
    }

    private void EndTrace()
    {
        context.isTracing = false;
        context.isTraceReleasedPending = false;
        context.instance.selfSpeedMultiplier = 1f;
        context.target = null;
    }
}
