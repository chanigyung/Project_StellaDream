using UnityEngine;

public class WeaponDrop : MonoBehaviour
{
    public WeaponInstance weaponInstance; // 드랍된 무기의 정보
    public SpriteRenderer iconRenderer;
    public GameObject highlightObject; // 하이라이트용 오브젝트

    //드랍된 무기 아이콘 표시해주기
    public void Initialize(WeaponInstance instance)
    {
        weaponInstance = instance;

        if (weaponInstance != null && iconRenderer != null)
        {
            iconRenderer.sprite = weaponInstance.data.icon;
        }

        if (highlightObject != null)
        {
            highlightObject.SetActive(false); // 기본은 꺼둠
        }
    }

    //현재 획득 가능한(=가장 가까운) 무기가 뭔지 표시
    public void SetHighlight(bool isOn)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(isOn);
        }
    }
}