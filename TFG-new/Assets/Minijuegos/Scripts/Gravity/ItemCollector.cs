using UnityEngine;
using System.Collections.Generic;

public static class ItemCollector
{
    // Dictionary to track how many items of each type have been collected
    private static Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    /// <summary>
    /// Adds one instance of the specified item type to the counter.
    /// </summary>
    /// <param name="itemType">The type of the item (e.g., "Tomato").</param>
    public static void AddItem(string itemType)
    {
        if (!itemCounts.ContainsKey(itemType))
        {
            itemCounts[itemType] = 0;
        }
        itemCounts[itemType]++;
        Debug.Log($"Collected item '{itemType}'. Total = {itemCounts[itemType]}");
    }

    /// <summary>
    /// Returns the total count for the specified item type.
    /// </summary>
    /// <param name="itemType">The item type to check.</param>
    /// <returns>The count of collected items of that type.</returns>
    public static int GetItemCount(string itemType)
    {
        return itemCounts.ContainsKey(itemType) ? itemCounts[itemType] : 0;
    }

    public static void ResetItemCounts()
    {
        itemCounts.Clear();
        Debug.Log("ItemCollector: contadores reseteados.");
    }

}
