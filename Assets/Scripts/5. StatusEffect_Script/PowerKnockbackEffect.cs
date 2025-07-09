using UnityEngine;

public class PowerKnockbackEffect : StatusEffect
{
    private float power;
    private Rigidbody2D rb;
    private IMovementController movementController;
    private bool hasCollidedWithWall = false;

    public PowerKnockbackEffect(GameObject target, StatusEffectManager manager, GameObject attacker, float power, float duration)
        : base(target, manager, attacker)
    {
        this.effectType = StatusEffectType.PowerKnockback;
        this.duration = duration;
        this.power = power;
    }

    public override void Start()
    {
        if (target.TryGetComponent(out rb) && target.TryGetComponent(out movementController))
        {
            Vector2 direction = (target.transform.position.x > attacker.transform.position.x) ? Vector2.right : Vector2.left;
            rb.velocity = Vector2.zero;
            rb.AddForce(direction * power, ForceMode2D.Impulse);
            movementController.SetPowerKnockbacked(true);
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (!hasCollidedWithWall && rb != null)
        {
            Debug.DrawRay(target.transform.position, rb.velocity.normalized * 0.1f, Color.red, 0.5f);
            RaycastHit2D hit = Physics2D.Raycast(target.transform.position, rb.velocity.normalized, 0.5f, LayerMask.GetMask("Ground"));
            if (hit.collider != null)
            {
                hasCollidedWithWall = true;
                rb.velocity = Vector2.zero;
                Debug.Log("벽에 부딪힘, 이동 정지");
            }
        }
    }

    public override void Expire()
    {
        if (movementController != null)
        {
            movementController.SetPowerKnockbacked(false);
        }
        base.Expire();
    }

    public override bool TryReplace(StatusEffect newEffect)
    {
        // 즉시 교체 허용 (넉백 중 새 넉백 적용 가능)
        return true;
    }
}
