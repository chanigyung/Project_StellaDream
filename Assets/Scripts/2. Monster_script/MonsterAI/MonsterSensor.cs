using UnityEngine;

public class MonsterSensor : MonoBehaviour
{
    private MonsterContext context;

    public void Initialize(MonsterContext ctx)
    {
        context = ctx;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            context.isPlayerDetected = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            context.isPlayerDetected = false;
        }
    }
}