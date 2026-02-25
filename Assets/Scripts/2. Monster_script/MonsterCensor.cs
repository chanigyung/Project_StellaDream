using UnityEngine;

public class MonsterCensor : MonoBehaviour
{
    [Header("바닥 체크")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("벽 체크")]
    [SerializeField] private Transform wallCheckPoint;
    // [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private float wallCheckRadius = 0.12f;

    private MonsterContext context;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    private void FixedUpdate()
    {
        if (context == null)
            return;

        UpdateGrounded();
        UpdateWallAhead();
    }

    private void UpdateGrounded()
    {
        if (groundCheckPoint == null)
            return;

        bool grounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        context.isGrounded = grounded;
    }

    // 2단계: 전방 벽 판정(Raycast)
    private void UpdateWallAhead()
    {   
        if (wallCheckPoint == null)
            return;

        bool hasWall = Physics2D.OverlapCircle(wallCheckPoint.position, wallCheckRadius, groundLayer);
        context.hasWallAhead = hasWall;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);

        if (wallCheckPoint != null)
        {
            Gizmos.DrawWireSphere(wallCheckPoint.position, wallCheckRadius);
        }
    }
}