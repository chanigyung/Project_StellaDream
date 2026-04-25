using UnityEngine;

public enum CoreTestTargetSlot
{
    Main,
    Sub
}

public class CoreTestNPC : MonoBehaviour, IInteractable
{
    [Header("Test Core")]
    [SerializeField] private CoreData coreData;
    [SerializeField] private CoreTestTargetSlot targetSlot = CoreTestTargetSlot.Main;

    [Header("Optional Visuals")]
    [SerializeField] private GameObject highlightObject;

    public void SetHighlight(bool isOn)
    {
        if (highlightObject != null)
            highlightObject.SetActive(isOn);
    }

    public void Interact()
    {
        PlayerWeaponManager weaponManager = PlayerWeaponManager.Instance;
        if (weaponManager == null)
        {
            Debug.LogWarning("[CoreTestNPC] PlayerWeaponManager.Instance is null.");
            return;
        }

        if (coreData == null)
        {
            Debug.LogWarning($"[CoreTestNPC] CoreData is not assigned on '{name}'.");
            return;
        }

        WeaponInstance targetWeapon = ResolveTargetWeapon(weaponManager, out string resolvedSlotLabel);
        if (targetWeapon == null || targetWeapon.data == null)
        {
            Debug.LogWarning($"[CoreTestNPC] No valid weapon equipped in {resolvedSlotLabel} slot.");
            return;
        }

        if (targetWeapon.coreInstance != null)
        {
            string removedCoreName = targetWeapon.coreInstance.data != null
                ? targetWeapon.coreInstance.data.name
                : "Unknown Core";

            targetWeapon.UnequipCore();
            Debug.Log($"[CoreTestNPC] Unequipped '{removedCoreName}' from {resolvedSlotLabel} weapon '{targetWeapon.data.name}'.");
            return;
        }

        if (!targetWeapon.CanEquipCore(coreData))
        {
            Debug.LogWarning($"[CoreTestNPC] '{coreData.name}' cannot be equipped on '{targetWeapon.data.name}'.");
            return;
        }

        bool equipped = targetWeapon.EquipCore(coreData);
        if (!equipped)
        {
            Debug.LogWarning($"[CoreTestNPC] Failed to equip '{coreData.name}' on '{targetWeapon.data.name}'.");
            return;
        }

        Debug.Log($"[CoreTestNPC] Equipped '{coreData.name}' to {resolvedSlotLabel} weapon '{targetWeapon.data.name}'.");
    }

    private WeaponInstance ResolveTargetWeapon(PlayerWeaponManager weaponManager, out string resolvedSlotLabel)
    {
        WeaponInstance mainWeapon = weaponManager.mainWeaponInstance;
        WeaponInstance subWeapon = weaponManager.subWeaponInstance;

        if (targetSlot == CoreTestTargetSlot.Main)
        {
            resolvedSlotLabel = "main";
            return mainWeapon;
        }

        if (mainWeapon != null &&
            mainWeapon.data != null &&
            mainWeapon.data.weaponType == WeaponType.TwoHanded)
        {
            resolvedSlotLabel = "main (two-handed override)";
            return mainWeapon;
        }

        resolvedSlotLabel = "sub";
        return subWeapon;
    }
}
