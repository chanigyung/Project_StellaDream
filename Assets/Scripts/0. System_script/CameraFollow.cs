using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라갈 대상
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;

        targetPos.x = Mathf.Round(targetPos.x * 100f) / 100f;
        targetPos.y = Mathf.Round(targetPos.y * 100f) / 100f;
        
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
