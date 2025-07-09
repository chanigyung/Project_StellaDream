using UnityEngine;
using TMPro;

public class FloatingDamage : MonoBehaviour
{
    public TextMeshPro text;            // World Space용 TextMeshPro
    public float lifetime = 0.8f;         // 생존 시간

    public float initialSpeed = 15f;
    public float damping = 2f; // 감속 비율
    private Vector3 velocity;

    private float timer;

    public void Initialize(float damage)
    {
        if (text != null)
            text.text = damage.ToString();
        // text.raycastTarget = false;

        Vector3 dir = Vector3.up + (Random.value < 0.5f ? Vector3.left : Vector3.right);
        velocity = dir.normalized * initialSpeed;

        timer = lifetime;
    }

    private void Update()
    {
        velocity = Vector3.Lerp(velocity, Vector3.zero, damping * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;

        timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / lifetime);

        float t = Mathf.Clamp01(1f - (timer / lifetime));
        float scale = Mathf.Lerp(3.0f, 0.8f, t);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (text != null)
        {
            Color c = text.color;
            text.color = new Color(c.r, c.g, c.b, alpha);
        }

        if (timer <= 0f)
        {
            Destroy(gameObject); // 추후 풀링 적용 가능
        }
    }
}
