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

        if (rigid.velocity.y > 0f)
        {
            // 점프 상승 속도(+y)와 넉백 y가 누적되어 과하게 튀는 현상 방지
            // - 상승 중일 때만 y 속도를 제한(또는 0으로 리셋)해서 넉백이 과하게 합쳐지지 않게 함
            float maxUpSpeedBeforeKnockback = 2.0f;
            rigid.velocity = new Vector2(rigid.velocity.x, Mathf.Min(rigid.velocity.y, maxUpSpeedBeforeKnockback));
        }

        // 기존: x는 즉시 0으로 초기화해서 넉백이 명확히 적용되게
        rigid.velocity = new Vector2(0, rigid.velocity.y);
        rigid.AddForce(finalForce, ForceMode2D.Impulse);

        instance.IsKnockbackActive = true;
        knockbackTimer = 0.3f * (1f - instance.GetKnockbackResistance());
    }
}
