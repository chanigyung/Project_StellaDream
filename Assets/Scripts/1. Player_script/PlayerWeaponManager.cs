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

    private WeaponSkillBase mainWeaponSkill;
    private WeaponSkillBase subWeaponSkill;

    [SerializeField] private SkillExecutor skillExecutor;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (skillExecutor == null)
            skillExecutor = GetComponent<SkillExecutor>();

        if (skillExecutor == null)
            skillExecutor = gameObject.AddComponent<SkillExecutor>();
    }

    private void Start()
    {
        RebuildWeaponSkills();
    }

    public void EquipMainWeapon(WeaponInstance weaponInstance)
    {
        // 보조무기에 좌클릭한 경우 서로 스왑
        if (weaponInstance == subWeaponInstance)
        {
            subWeaponInstance = mainWeaponInstance;

            if (subWeaponInstance != null)
                ShowWeapon(subWeaponRenderer, subWeaponInstance.data.weaponSprite, subWeaponInstance.data.subRotationOffsetZ);
            else
                HideWeapon(subWeaponRenderer);
        }

        mainWeaponInstance = weaponInstance;

        if (mainWeaponInstance != null && mainWeaponInstance.data != null)
        {
            ShowWeapon(mainWeaponRenderer, mainWeaponInstance.data.weaponSprite, mainWeaponInstance.data.mainRotationOffsetZ);

            if (mainWeaponInstance.data.weaponType == WeaponType.TwoHanded)
                UnequipSubWeapon();
        }
        else
        {
            HideWeapon(mainWeaponRenderer);
        }

        RebuildWeaponSkills();

        if (HotbarController.Instance != null)
            HotbarController.Instance.SyncEquipped(mainWeaponInstance, subWeaponInstance);
    }

    public bool EquipSubWeapon(WeaponInstance weaponInstance)
    {
        if (weaponInstance == null || weaponInstance.data == null)
        {
            Debug.Log("Instance 또는 data가 비어있음");
            return false;
        }

        if (weaponInstance.data.weaponType == WeaponType.TwoHanded)
        {
            Debug.LogWarning("양손 무기는 보조무기로 장착 불가");
            return false;
        }

        if (mainWeaponInstance != null && mainWeaponInstance.data != null &&
            mainWeaponInstance.data.weaponType == WeaponType.TwoHanded)
        {
            mainWeaponInstance = null;
            HideWeapon(mainWeaponRenderer);
        }

        // 주무기에 우클릭한 경우 서로 스왑
        if (weaponInstance == mainWeaponInstance)
        {
            mainWeaponInstance = subWeaponInstance;

            if (mainWeaponInstance != null)
                ShowWeapon(mainWeaponRenderer, mainWeaponInstance.data.weaponSprite, mainWeaponInstance.data.mainRotationOffsetZ);
            else
                HideWeapon(mainWeaponRenderer);
        }

        subWeaponInstance = weaponInstance;

        ShowWeapon(subWeaponRenderer, weaponInstance.data.weaponSprite, weaponInstance.data.subRotationOffsetZ);

        RebuildWeaponSkills();

        if (HotbarController.Instance != null)
            HotbarController.Instance.SyncEquipped(mainWeaponInstance, subWeaponInstance);

        return true;
    }

    public void UnequipMainWeapon()
    {
        mainWeaponInstance = null;
        HideWeapon(mainWeaponRenderer);
        RebuildWeaponSkills();
    }

    public void UnequipSubWeapon()
    {
        subWeaponInstance = null;
        HideWeapon(subWeaponRenderer);
        RebuildWeaponSkills();
    }

    public bool HandleMainInput(WeaponSkillInputPhase inputPhase, Vector2 aimDirection)
    {
        if (mainWeaponSkill == null)
            return false;

        return mainWeaponSkill.HandleMainInput(inputPhase, aimDirection);
    }

    public bool HandleSubInput(WeaponSkillInputPhase inputPhase, Vector2 aimDirection)
    {
        if (mainWeaponSkill == null)
            return false;

        return mainWeaponSkill.HandleSubInput(inputPhase, aimDirection);
    }

    private void RebuildWeaponSkills()
    {
        mainWeaponSkill = CreateWeaponSkill(mainWeaponInstance);
        subWeaponSkill = CreateWeaponSkill(subWeaponInstance);
    }

    private WeaponSkillBase CreateWeaponSkill(WeaponInstance weaponInstance)
    {
        if (weaponInstance == null || weaponInstance.weaponData == null)
            return null;

        switch (weaponInstance.weaponData.weaponSkillType)
        {
            case WeaponSkillType.Default:
            default:
                return new WeaponSkillBase(weaponInstance, skillExecutor);
        }
    }

    private WeaponSkillBase GetActiveSubWeaponSkill()
    {
        if (mainWeaponInstance != null &&
            mainWeaponInstance.data != null &&
            mainWeaponInstance.data.weaponType == WeaponType.TwoHanded)
        {
            return mainWeaponSkill;
        }

        return subWeaponSkill;
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