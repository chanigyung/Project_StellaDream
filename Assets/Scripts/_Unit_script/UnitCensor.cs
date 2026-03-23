using UnityEngine;

public class UnitCensor : MonoBehaviour
{
    [Header("바닥 체크")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("벽 체크")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private float wallCheckRadius = 0.12f;

    [Header("낙하 체크")]
    [SerializeField] private float ledgeCheckForwardDistance = 0.35f; // 발 전방 거리
    [SerializeField] private float maxDropDistance = 0.8f; // 아래 거리

    [SerializeField] private UnitMovement unitMovement;
    private UnitContext context; // 1단계에서는 MonsterContext 그대로 사용

    public void Initialize(UnitContext ctx) // 1단계에서는 MonsterContext 그대로 사용
    {
        context = ctx;
    }

    private void FixedUpdate()
    {
        if (context == null)
            return;

        UpdateGrounded();
        UpdateWallAhead();
        UpdateGroundAhead();
    }

    private void UpdateGrounded()
    {
        if (groundCheckPoint == null)
            return;

        bool grounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        context.isGrounded = grounded;
        unitMovement?.SetGrounded(grounded);
    }

    private void UpdateWallAhead()
    {
        if (wallCheckPoint == null)
            return;

        bool hasWall = Physics2D.OverlapCircle(wallCheckPoint.position, wallCheckRadius, groundLayer);
        context.hasWallAhead = hasWall;
    }

    private void UpdateGroundAhead()
    {
        if (groundCheckPoint == null)
            return;

        context.hasGroundLeft = CheckGroundInDirection(-1f);
        context.hasGroundRight = CheckGroundInDirection(1f);
    }

    private bool CheckGroundInDirection(float dirX)
    {
        Vector2 origin = (Vector2)groundCheckPoint.position + Vector2.right * (dirX * ledgeCheckForwardDistance);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxDropDistance, groundLayer);
        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);

        if (wallCheckPoint != null)
            Gizmos.DrawWireSphere(wallCheckPoint.position, wallCheckRadius);

        if (groundCheckPoint != null)
        {
            Vector3 leftOrigin = groundCheckPoint.position + Vector3.right * (-ledgeCheckForwardDistance);
            Vector3 rightOrigin = groundCheckPoint.position + Vector3.right * (ledgeCheckForwardDistance);
            Gizmos.DrawLine(leftOrigin, leftOrigin + Vector3.down * maxDropDistance);
            Gizmos.DrawLine(rightOrigin, rightOrigin + Vector3.down * maxDropDistance);
        }
    }
}