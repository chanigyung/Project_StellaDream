using UnityEngine;
using UnityEngine.EventSystems;

/// 스킬 발동을 담당하는 컴포넌트. 마우스 클릭 시 지정된 스킬을 실행한다.
public class PlayerSkillController : MonoBehaviour
{
    public PlayerController playerController;
    public PlayerWeaponManager weaponManager;
    public SkillExecutor skillExecutor;

    void Update()
    {
        //나중에 좌클릭우클릭 동시에 못하는 처리 해야함
        
        if (InputBlocker.IsBlocked) return;

        // 아무 입력 없으면 리턴
        int button = playerController.GetPressedButton();
        // int button = GetPressedButton();
        if (button == -1) return; 

        SkillInstance skillToUse = (button == 0) ? weaponManager.GetMainSkill() : weaponManager.GetSubSkill();

        //무기 미장착시 예외처리
        if (skillToUse == null)
        {
            if (Input.GetMouseButtonDown(button))
            {
                Debug.Log($"{(button == 0 ? "주무기" : "보조무기")} 미장착 상태");
            }
            return;
        }

        var dir = GetMouseDirection();

        // 바로시전 / 누르다 떼는순간 시전 / 누르는내내 시전
        bool inputTriggered = false;
        switch (skillToUse.baseData.activationType)
        {
            case SkillActivationType.OnPress:
                inputTriggered = Input.GetMouseButtonDown(button);
                break;
            case SkillActivationType.OnRelease:
                inputTriggered = Input.GetMouseButtonUp(button);
                break;
            case SkillActivationType.WhileHeld:
                inputTriggered = Input.GetMouseButton(button);
                break;
        }

        if (inputTriggered)
        {
            if (skillExecutor.UseSkill(skillToUse, dir))
            {
                var weapon = weaponManager.GetWeaponBySkill(skillToUse);
                if (weapon != null && weapon.isTemporary)
                    HandleDurabilityAfterSkill(weapon);
            }
        }
    }

    //마우스 좌표 받아와 방향 설정
    Vector2 GetMouseDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return (mouseWorld - transform.position).normalized;
    }

    private void HandleDurabilityAfterSkill(WeaponInstance weaponInstance) //스킬 사용 후 무기 내구도 1 줄이기
    {
        if (weaponInstance == null || !weaponInstance.isTemporary) return;

        bool stillUsable = weaponInstance.UseOnce();
        Debug.Log("현재 내구도 : " + weaponInstance.currentDurability);

        int slotIndex = HotbarUIManager.Instance.GetSlotIndexByWeapon(weaponInstance);
        if (slotIndex != -1)
        {
            HotbarUIManager.Instance.UpdateDurabilityUI(weaponInstance);

            if (!stillUsable)
            {
                HotbarController.Instance.ClearWeaponAt(slotIndex); // 데이터 제거
                var wm = PlayerWeaponManager.Instance;
                if (weaponInstance == wm.mainWeaponInstance)
                    wm.UnequipMainWeapon();
                if (weaponInstance == wm.subWeaponInstance)
                    wm.UnequipSubWeapon();
            }
        }
    }
}