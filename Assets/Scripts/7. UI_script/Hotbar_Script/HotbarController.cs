using System;
using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour
{
    public static HotbarController Instance { get; private set; }

    [SerializeField] private int hotbarSize = 6; // 슬롯 개수 설정 가능하게
    private WeaponInstance[] weaponList;
    public IReadOnlyList<WeaponInstance> WeaponList => weaponList;

    public WeaponInstance MainWeapon { get; private set; }
    public WeaponInstance SubWeapon { get; private set; }

    public event Action OnHotbarChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        weaponList = new WeaponInstance[hotbarSize]; // 배열 초기화
    }

    // 슬롯에 무기 설정
    public void SetWeaponAt(int index, WeaponInstance instance)
    {
        if (index < 0 || index >= weaponList.Length) return;
        weaponList[index] = instance;
        OnHotbarChanged?.Invoke();
    }

    // 슬롯 비우기
    public void ClearWeaponAt(int index)
    {
        if (index < 0 || index >= weaponList.Length) return;
        if (weaponList[index] == MainWeapon) MainWeapon = null;
        if (weaponList[index] == SubWeapon) SubWeapon = null;
        weaponList[index] = null;
        OnHotbarChanged?.Invoke();
    }

    public void EquipMain(int index)
    {
        if (index < 0 || index >= weaponList.Length) return;
        var instance = weaponList[index];
        if (instance == null) return;

        // 실제 장착 처리 (조건은 PlayerWeaponManager가 판단)
        PlayerWeaponManager.Instance?.EquipMainWeapon(instance);

        MainWeapon = PlayerWeaponManager.Instance.mainWeaponInstance;
        SubWeapon = PlayerWeaponManager.Instance.subWeaponInstance;

        OnHotbarChanged?.Invoke();
    }

    public void EquipSub(int index)
    {
        if (index < 0 || index >= weaponList.Length) return;
        var instance = weaponList[index];
        if (instance == null) return;

        // 실제 장착 처리 (PlayerWeaponManager에서 양손 무기 여부 판단)
        bool equipped = PlayerWeaponManager.Instance?.EquipSubWeapon(instance) ?? false;
        if (!equipped) return;

        MainWeapon = PlayerWeaponManager.Instance.mainWeaponInstance;
        SubWeapon = PlayerWeaponManager.Instance.subWeaponInstance;

        OnHotbarChanged?.Invoke();
    }

    public void UnequipMain()
    {
        MainWeapon = null;
        PlayerWeaponManager.Instance?.UnequipMainWeapon();
        OnHotbarChanged?.Invoke();
    }

    public void UnequipSub()
    {
        SubWeapon = null;
        PlayerWeaponManager.Instance?.UnequipSubWeapon();
        OnHotbarChanged?.Invoke();
    }

    // 외부에서 무기 리스트를 받아 초기화할 때 (세이브 복구)
    public void LoadWeaponList(List<WeaponInstance> list)
    {
        weaponList = new WeaponInstance[hotbarSize];
        for (int i = 0; i < hotbarSize && i < list.Count; i++)
        {
            if (i < list.Count && list[i]?.data != null)
                weaponList[i] = list[i];
            else
                weaponList[i] = null;
        }
        OnHotbarChanged?.Invoke();
    }

    // 무기 리스트 내보내기 (세이브)
    public List<WeaponInstance> GetWeaponList()
    {
        return new List<WeaponInstance>(weaponList);
    }

    //첫 번째 빈 슬롯 인덱스 반환, 다찼으면 -1
    public int FindFirstEmptySlot()
    {
        for (int i = 0; i < weaponList.Length; i++)
        {
            if (weaponList[i] == null)
                return i;
        }
        return -1;
    }
    
    public void SyncEquipped(WeaponInstance main, WeaponInstance sub) //핫바 - 플레이어 컨트롤러간 장착무기 정보 일치시키기
    {
        MainWeapon = main;
        SubWeapon = sub;
        OnHotbarChanged?.Invoke();
    }
}