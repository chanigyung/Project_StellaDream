public interface IItemSlot //아이템 슬롯 인터페이스
{
    WeaponInstance GetWeaponInstance();
    // void SetWeaponInstance(WeaponInstance instance);
    void ClearSlot();
    bool IsEmpty();
    SlotType GetSlotType();
}
