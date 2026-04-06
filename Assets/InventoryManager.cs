using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct CraftingRecipe
{
    public Item itemA;
    public Item itemB;
    public Item resultItem;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    [Header("Inventory Data")]
    // Danh sách các item đang có trong người (Runtime)
    public List<Item> items = new List<Item>();

    // Danh sách lưu trữ các item mặc định được gán từ Inspector (như ID Card)
    private List<Item> defaultItems = new List<Item>();

    [Header("Crafting Recipes")]
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Lưu lại danh sách item mặc định mà bạn đã gán trong Inspector lúc đầu
            if (items != null && items.Count > 0)
            {
                defaultItems = new List<Item>(items);
            }
        }
        else
        {
            // Nếu một InventoryManager mới xuất hiện ở scene khác, 
            // chúng ta có thể muốn cập nhật lại defaultItems nếu scene đó có thiết lập khác.
            // Tuy nhiên, thông thường Singleton sẽ giữ nguyên bản đầu tiên.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Reset kho đồ nhưng vẫn giữ lại các vật phẩm mặc định (như ID Card).
    /// </summary>
    public void ResetInventory()
    {
        items.Clear();

        // Nạp lại những item quan trọng ban đầu
        if (defaultItems != null && defaultItems.Count > 0)
        {
            foreach (var defaultItem in defaultItems)
            {
                items.Add(defaultItem);
            }
            Debug.Log("InventoryManager: Đã reset kho đồ về trạng thái mặc định.");
        }
        else
        {
            Debug.Log("InventoryManager: Kho đồ đã trống hoàn toàn (không có item mặc định).");
        }
    }

    public void AddItem(Item item)
    {
        if (item != null)
        {
            items.Add(item);
        }
    }

    public bool HasItem(string itemId)
    {
        return items.Any(item => item != null && item.uniqueId == itemId);
    }

    public void RemoveItem(Item itemToRemove)
    {
        items.Remove(itemToRemove);
    }

    public void RemoveItem(string itemId)
    {
        Item itemToRemove = items.FirstOrDefault(item => item != null && item.uniqueId == itemId);
        if (itemToRemove != null)
        {
            items.Remove(itemToRemove);
        }
    }

    public Item TryCraft(Item item1, Item item2)
    {
        if (item1 == null || item2 == null || item1 == item2) return null;

        foreach (var recipe in craftingRecipes)
        {
            bool matchA = (recipe.itemA == item1 && recipe.itemB == item2);
            bool matchB = (recipe.itemA == item2 && recipe.itemB == item1);

            if (matchA || matchB)
            {
                return recipe.resultItem;
            }
        }
        return null;
    }
}