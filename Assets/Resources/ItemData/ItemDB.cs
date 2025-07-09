using System.Collections.Generic;
using UnityEngine;

public static class ItemDB
{
    private static Dictionary<string, ItemData> itemCache;

    static ItemDB() //모든 ItemData를 가져와서 allItems에 dictionary형태로 저장함
    {
        itemCache = new Dictionary<string, ItemData>();

        var allItems = Resources.LoadAll<ItemData>("ItemData");
        foreach (var item in allItems)
        {
            itemCache[item.itemName] = item;
        }
    }

    public static ItemData Get(string itemName)
    {
        if (itemCache.ContainsKey(itemName))
            return itemCache[itemName];

        Debug.LogWarning($"[ItemDB] 아이템 '{itemName}'을 찾을 수 없습니다.");
        return null;
    }
}