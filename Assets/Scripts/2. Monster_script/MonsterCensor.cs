using UnityEngine;

public class MonsterCensor : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private float wallCheckDistance = 0.3f;

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

        float dirX = context.facingDirectionX;
        if (Mathf.Abs(dirX) < 0.01f)
            dirX = 1f;

        Vector2 dir = new Vector2(Mathf.Sign(dirX), 0f);
        RaycastHit2D hit = Physics2D.Raycast(wallCheckPoint.position, dir, wallCheckDistance, groundLayer);
        context.hasWallAhead = hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);

        if (wallCheckPoint != null)
            Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + Vector3.right * wallCheckDistance);
    }
}