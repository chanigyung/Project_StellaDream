using UnityEngine;

public enum ItemType
{
    Material,Equipment,Weapon
}
public enum Rarity { Common, Rare, Epic}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Item/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType itemType;
    public Rarity rarity;
    [TextArea]
    public string description;
}