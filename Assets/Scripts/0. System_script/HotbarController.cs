using System;
using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour
{
    public static HotbarController Instance { get; private set; }

    private readonly List<WeaponInstance> weapons = new();
    public IReadOnlyList<WeaponInstance> Weapons => weapons;

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
    }

    public void LoadWeaponList(List<WeaponInstance> list)
    {
        weapons.Clear();
        weapons.AddRange(list);
        OnHotbarChanged?.Invoke();
    }

    public void AddWeapon(WeaponInstance instance)
    {
        if (!weapons.Contains(instance))
        {
            weapons.Add(instance);
            OnHotbarChanged?.Invoke();
        }
    }

    public void RemoveWeapon(WeaponInstance instance)
    {
        if (MainWeapon == instance) MainWeapon = null;
        if (SubWeapon == instance) SubWeapon = null;

        weapons.Remove(instance);
        OnHotbarChanged?.Invoke();
    }

    public void EquipMain(WeaponInstance instance)
    {
        if (instance == null || !weapons.Contains(instance)) return;

        if (instance == SubWeapon)
            SubWeapon = MainWeapon;

        MainWeapon = instance;
        OnHotbarChanged?.Invoke();
    }

    public void EquipSub(WeaponInstance instance)
    {
        if (instance == null || !weapons.Contains(instance)) return;

        if (instance == MainWeapon) return;

        SubWeapon = instance;
        OnHotbarChanged?.Invoke();
    }

    public void UnequipMain()
    {
        MainWeapon = null;
        OnHotbarChanged?.Invoke();
    }

    public void UnequipSub()
    {
        SubWeapon = null;
        OnHotbarChanged?.Invoke();
    }

    public List<WeaponInstance> GetWeaponList()
    {
        return new List<WeaponInstance>(weapons);
    }
}