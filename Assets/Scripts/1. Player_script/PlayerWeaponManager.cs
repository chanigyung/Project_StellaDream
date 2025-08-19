using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour
{
    public static PlayerWeaponManager Instance { get; private set; }

    public Transform rightArm;
    public Transform leftArm;

    public WeaponInstance mainWeaponInstance;
    public WeaponInstance subWeaponInstance;

    public SpriteRenderer mainWeaponRenderer;
    public SpriteRenderer subWeaponRenderer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EquipMainWeapon(WeaponInstance weaponInstance)
    {
        //보조무기에 좌클릭한경우 서로 스왑
        if (weaponInstance == subWeaponInstance)
        {
            subWeaponInstance = mainWeaponInstance;
            ShowWeapon(subWeaponRenderer, subWeaponInstance.data.weaponSprite, subWeaponInstance.data.subRotationOffsetZ);
        }

        mainWeaponInstance = weaponInstance;

        if (mainWeaponInstance != null && mainWeaponInstance.data != null)
        {
            ShowWeapon(mainWeaponRenderer, mainWeaponInstance.data.weaponSprite, mainWeaponInstance.data.mainRotationOffsetZ);
            // 양손 무기일 경우 왼손 해제
            if (mainWeaponInstance.data.weaponType == WeaponType.TwoHanded)
                UnequipSubWeapon();
        }
        else
        {
            HideWeapon(mainWeaponRenderer);
        }

        if (HotbarController.Instance != null)
            HotbarController.Instance.SyncEquipped(mainWeaponInstance, subWeaponInstance);
    }

    public bool EquipSubWeapon(WeaponInstance weaponInstance)
    {
        if (weaponInstance.data.weaponType == WeaponType.TwoHanded)
        {
            Debug.LogWarning("양손 무기는 보조무기로 장착 불가");
            return false;
        }

        if(weaponInstance == null)
        {
            Debug.LogWarning("주무기가 장착되어 있지 않으므로 보조무기 장착 불가");
            return false;
        }
        else if (mainWeaponInstance.data.weaponType == WeaponType.TwoHanded)
        {
            Debug.LogWarning("양손무기가 장착되어있으므로 보조무기 장착 불가");
            return false;
        }

        // 주무기에 우클릭한 경우 서로 스왑
        if (weaponInstance == mainWeaponInstance)
        {
            mainWeaponInstance = subWeaponInstance;
            ShowWeapon(mainWeaponRenderer, mainWeaponInstance.data.weaponSprite, mainWeaponInstance.data.mainRotationOffsetZ);
        }

        subWeaponInstance = weaponInstance;

        if (weaponInstance != null)
        {
            ShowWeapon(subWeaponRenderer, weaponInstance.data.weaponSprite, weaponInstance.data.subRotationOffsetZ);
        }
        else
        {
            HideWeapon(subWeaponRenderer);
        }

        if (HotbarController.Instance != null)
            HotbarController.Instance.SyncEquipped(mainWeaponInstance, subWeaponInstance);
        return true;
    }

    public void UnequipMainWeapon() //무기 해제
    {
        mainWeaponInstance = null;
        HideWeapon(mainWeaponRenderer);
    }

    public void UnequipSubWeapon() //무기 해제
    {
        subWeaponInstance = null;
        HideWeapon(subWeaponRenderer);
    }

    public SkillInstance GetMainSkill() //메인스킬 가져오기, 무기종류 상관없음
    {
        return mainWeaponInstance != null && mainWeaponInstance.data != null
        ? mainWeaponInstance.GetMainSkillInstance() : null;
    }

    public SkillInstance GetSubSkill() //서브스킬 가져오기, 양손이면 메인무기의 서브스킬 / 한손이면 서브무기의 서브스킬
    {
        // 양손 무기일 경우 → 오른손 무기의 서브 스킬
        if (mainWeaponInstance != null && mainWeaponInstance.data != null && mainWeaponInstance.data.weaponType == WeaponType.TwoHanded)
        {
            return mainWeaponInstance.GetSubSkillInstance();
        }
        // 그 외의 경우 → 왼손 무기 서브 스킬
        else if (subWeaponInstance != null && subWeaponInstance.data != null)
        {
            return subWeaponInstance.GetSubSkillInstance();
        }
        
        return null;
    }

    public WeaponInstance GetWeaponBySkill(SkillInstance skillInstance) //사용한 스킬을 받아서 해당하는 무기 인스턴스 돌려주기
    {
        if (mainWeaponInstance != null)
        {
            if (mainWeaponInstance.GetMainSkillInstance() == skillInstance)
                return mainWeaponInstance;

            if (mainWeaponInstance.data.weaponType == WeaponType.TwoHanded &&
                mainWeaponInstance.GetSubSkillInstance() == skillInstance)
                return mainWeaponInstance;
        }
        if (subWeaponInstance != null && subWeaponInstance.GetSubSkillInstance() == skillInstance)
            return subWeaponInstance;

        return null;
    }

    private void ShowWeapon(SpriteRenderer renderer, Sprite sprite, float rotationOffsetZ)
    {
        renderer.sprite = sprite;
        renderer.transform.localRotation = Quaternion.Euler(0, 0, rotationOffsetZ);
        renderer.enabled = true;
    }

    private void HideWeapon(SpriteRenderer renderer)
    {
        renderer.sprite = null;
        renderer.enabled = false;
    }
}