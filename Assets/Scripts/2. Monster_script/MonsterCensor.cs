using UnityEngine;

public class MonsterCensor : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    public bool IsGrounded { get; private set; }

    private MonsterContext context;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    private void FixedUpdate()
    {
        if (groundCheckPoint == null)
            return;

        IsGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (context != null)
            context.isGrounded = IsGrounded;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null)
            return;

        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }
}