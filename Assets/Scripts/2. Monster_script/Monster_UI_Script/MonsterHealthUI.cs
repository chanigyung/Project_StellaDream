using UnityEngine;
using UnityEngine.UI;

public class MonsterHealthUI : MonoBehaviour
{
    public UnitController controller;
    
    public Image healthFill;
    public Canvas canvas;
    public MonsterInstance monster;

    private float maxHealth;
    private float currentHealth;

    private float visibleTime = 2f;
    private float timer = 0f;

    private void Start()
    {
        canvas.enabled = false;
        maxHealth = controller.instance.MaxHealth;
        currentHealth = controller.instance.CurrentHealth;
    }

    private void Update()
    {
        // 일정 시간 지나면 비활성화
        if (canvas.enabled)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                canvas.enabled = false;
            }
        }
    }

    public void SetHealth(float current)
    {
        currentHealth = current;

        healthFill.fillAmount = current / maxHealth;

        canvas.enabled = true;
        timer = visibleTime;
    }
}
