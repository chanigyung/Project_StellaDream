using UnityEngine;

public class MonsterUIFixFlip : MonoBehaviour
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        // 부모의 전체 스케일을 받아옴 (월드 기준이 아닌 로컬 기준)
        Vector3 parentScale = transform.parent.lossyScale;

        float scaleXFix = parentScale.x >= 0 ? 1 : -1;

        // 반전 여부에 따라 자기 자신의 x스케일 뒤집기
        transform.localScale = new Vector3(
            originalScale.x * scaleXFix,
            originalScale.y,
            originalScale.z
        );
    }
}
