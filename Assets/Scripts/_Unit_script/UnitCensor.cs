using UnityEngine;

public class UnitCensor : MonoBehaviour
{
    [Header("Ground Check")]
    [Tooltip("Collider used as the unit body. If empty, the first Collider2D found in the parent hierarchy is used.")]
    [SerializeField] private Collider2D bodyCollider;
    [Range(0.1f, 1f)]
    [Tooltip("Ground check box width as a ratio of the body collider width. Lower values ignore more of the left and right edges.")]
    [SerializeField] private float groundBoxWidthRatio = 0.85f;
    [Range(0.01f, 0.3f)]
    [Tooltip("Ground check box height as a ratio of the body collider height. Higher values detect ground earlier, but can catch nearby side geometry.")]
    [SerializeField] private float groundBoxHeightRatio = 0.08f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private float wallCheckRadius = 0.12f;

    [Header("Ledge Check")]
    [SerializeField] private float ledgeCheckForwardDistance = 0.35f;
    [SerializeField] private float maxDropDistance = 0.8f;

    [SerializeField] private UnitMovement unitMovement;
    private UnitContext context;

    private void Awake()
    {
        ResolveBodyCollider();
    }

    public void Initialize(UnitContext ctx)
    {
        context = ctx;
        ResolveBodyCollider();
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
        if (!TryGetGroundBox(out Vector2 center, out Vector2 size))
        {
            context.isGrounded = false;
            unitMovement?.SetGrounded(false);
            return;
        }

        bool grounded = Physics2D.OverlapBox(center, size, 0f, GetEffectiveGroundLayer());
        context.isGrounded = grounded;
        unitMovement?.SetGrounded(grounded);
    }

    private void UpdateWallAhead()
    {
        if (wallCheckPoint == null)
            return;

        bool hasWall = Physics2D.OverlapCircle(wallCheckPoint.position, wallCheckRadius, GetEffectiveGroundLayer());
        context.hasWallAhead = hasWall;
    }

    private void UpdateGroundAhead()
    {
        if (bodyCollider == null)
            ResolveBodyCollider();

        if (bodyCollider == null)
        {
            context.hasGroundLeft = false;
            context.hasGroundRight = false;
            return;
        }

        context.hasGroundLeft = CheckGroundInDirection(-1f);
        context.hasGroundRight = CheckGroundInDirection(1f);
    }

    private bool CheckGroundInDirection(float dirX)
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x + dirX * ledgeCheckForwardDistance, bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, maxDropDistance, GetEffectiveGroundLayer());
        return hit.collider != null;
    }

    private void ResolveBodyCollider()
    {
        if (bodyCollider != null)
            return;

        bodyCollider = GetComponentInParent<Collider2D>();
    }

    private bool TryGetGroundBox(out Vector2 center, out Vector2 size)
    {
        center = Vector2.zero;
        size = Vector2.zero;

        if (bodyCollider == null)
            ResolveBodyCollider();

        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;
        float width = bounds.size.x * groundBoxWidthRatio;
        float height = bounds.size.y * groundBoxHeightRatio;

        size = new Vector2(width, height);
        center = new Vector2(bounds.center.x, bounds.min.y - height * 0.5f);
        return width > 0f && height > 0f;
    }

    private LayerMask GetEffectiveGroundLayer()
    {
        MonsterContext monsterContext = context as MonsterContext;
        if (monsterContext == null || !monsterContext.isFlyingMonster)
            return groundLayer;

        int platformLayer = LayerMask.NameToLayer("Platform");
        if (platformLayer < 0)
            return groundLayer;

        int maskBits = groundLayer.value & ~(1 << platformLayer);
        return maskBits;
    }

    private void OnDrawGizmosSelected()
    {
        ResolveBodyCollider();

        if (TryGetGroundBox(out Vector2 center, out Vector2 size))
            Gizmos.DrawWireCube(center, size);

        if (wallCheckPoint != null)
            Gizmos.DrawWireSphere(wallCheckPoint.position, wallCheckRadius);

        if (bodyCollider != null)
        {
            Bounds bounds = bodyCollider.bounds;
            Vector3 leftOrigin = new Vector3(bounds.center.x - ledgeCheckForwardDistance, bounds.min.y, 0f);
            Vector3 rightOrigin = new Vector3(bounds.center.x + ledgeCheckForwardDistance, bounds.min.y, 0f);
            Gizmos.DrawLine(leftOrigin, leftOrigin + Vector3.down * maxDropDistance);
            Gizmos.DrawLine(rightOrigin, rightOrigin + Vector3.down * maxDropDistance);
        }
    }
}
