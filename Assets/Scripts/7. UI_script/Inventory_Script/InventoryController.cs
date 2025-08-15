using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    [SerializeField] private int capacity = 20; // 최초 인벤토리 용량
    private List<WeaponInstance> weaponList;

    public int Capacity => capacity;
    public IReadOnlyList<WeaponInstance> WeaponList => weaponList;

    public event System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        weaponList = new List<WeaponInstance>(new WeaponInstance[capacity]);
    }

    // 인벤토리 슬롯에 무기 자동 등록
    public bool AddWeapon(WeaponInstance weapon)
    {
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i] == null)
            {
                weaponList[i] = weapon;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false; // 빈칸 없음
    }

    public void RemoveWeapon(WeaponInstance weapon)
    {
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i] == weapon)
            {
                weaponList[i] = null;
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

    public void SetWeaponAt(int index, WeaponInstance weapon)
    {
        if (index < 0 || index >= weaponList.Count) return;
        weaponList[index] = weapon;
        OnInventoryChanged?.Invoke();
    }

    public int FindFirstEmptySlot()
    {
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i] == null)
                return i;
        }
        return -1;
    }

    // 외부에서 불러올 때 (예: 세이브 불러오기)
    public void LoadInventoryFromData(List<WeaponInstance> list)
    {
        weaponList = new List<WeaponInstance>(new WeaponInstance[capacity]);

        for (int i = 0; i < weaponList.Count && i < list.Count; i++)
        {
            weaponList[i] = list[i];
        }
    }

    public List<WeaponInstance> GetWeaponList()
    {
        return new List<WeaponInstance>(weaponList);
    }
}
