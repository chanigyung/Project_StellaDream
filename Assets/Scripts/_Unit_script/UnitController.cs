using UnityEngine;

public class UnitController : MonoBehaviour, IDamageable, IKnockbackable
{
    public IUnitInstance instance { get; private set; }

    protected Rigidbody2D rigid;

    [SerializeField] protected GameObject floatingDamagePrefab;
    [SerializeField] protected Transform floatingDamageAnchor;

    private float knockbackTimer = 0f;

    public virtual void Initialize(IUnitInstance instance)
    {
        this.instance = instance;

        rigid = GetComponent<Rigidbody2D>();
    }

    protected virtual void Update()
    {
        if (instance.IsKnockbackActive)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                instance.IsKnockbackActive = false;
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        instance.TakeDamage(damage);
        // Debug.Log($"{gameObject.name}이 {damage}의 피해를 입음. 남은 체력: {instance.CurrentHealth}");

        ShowFloatingDamage(damage);

        if (instance.IsDead())
        {
            HandleDeath();
        }
    }

    protected virtual void HandleDeath()
    {
        Debug.Log($"{gameObject.name} 사망");
        Destroy(gameObject);
    }

    public virtual void ShowFloatingDamage(float damage)
    {
        if (floatingDamagePrefab == null || floatingDamageAnchor == null) return;

        GameObject obj = Instantiate(floatingDamagePrefab, floatingDamageAnchor);
        obj.transform.localPosition = Vector3.zero;
        obj.GetComponent<FloatingDamage>()?.Initialize(damage);
    }

    public virtual void ApplyKnockback(Vector2 force)
    {
        if (instance.IsKnockbackImmune()) return;

        Vector2 finalForce = new Vector2(
            force.x * (1f - instance.GetKnockbackResistance()),
            force.y
        );

        rigid.velocity = new Vector2(0, rigid.velocity.y);
        rigid.AddForce(finalForce, ForceMode2D.Impulse);

        instance.IsKnockbackActive = true;
        knockbackTimer = 0.3f * (1f - instance.GetKnockbackResistance());
    }
}
