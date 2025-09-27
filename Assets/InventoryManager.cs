using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Cần dùng cho các hàm LINQ

// Cấu trúc để định nghĩa công thức kết hợp
[System.Serializable]
public struct CraftingRecipe
{
    // Cần 2 item đầu vào, thứ tự không quan trọng
    public Item itemA;
    public Item itemB;
    // Item kết quả (Cần kéo thả Item ScriptableObject vào đây)
    public Item resultItem;
}

public class InventoryManager : MonoBehaviour
{
    // Singleton instance
    public static InventoryManager instance;

    // Danh sách các item trong kho đồ
    public List<Item> items = new List<Item>();

    // Danh sách các công thức kết hợp (Cần thiết lập trong Inspector)
    [Header("Crafting Recipes")]
    public List<CraftingRecipe> craftingRecipes = new List<CraftingRecipe>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Đảm bảo đối tượng này không bị hủy khi chuyển scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Nếu đã tồn tại, hủy đối tượng hiện tại để tránh trùng lặp
            Destroy(gameObject);
        }
    }

    // Thêm một item vào kho đồ
    public void AddItem(Item item)
    {
        items.Add(item);
    }

    // Kiểm tra xem item có tồn tại trong kho đồ không bằng itemId
    public bool HasItem(string itemId)
    {
        foreach (var item in items)
        {
            if (item.uniqueId == itemId)
            {
                return true;
            }
        }
        return false;
    }

    // Xóa một item khỏi kho đồ (Dùng item object để đảm bảo xóa đúng instance)
    public void RemoveItem(Item itemToRemove)
    {
        items.Remove(itemToRemove);
    }

    // Xóa một item khỏi kho đồ (Bản cũ, giữ lại để tương thích)
    public void RemoveItem(string itemId)
    {
        Item itemToRemove = null;
        foreach (var item in items)
        {
            if (item.uniqueId == itemId)
            {
                itemToRemove = item;
                break;
            }
        }

        if (itemToRemove != null)
        {
            items.Remove(itemToRemove);
        }
    }

    /// <summary>
    /// Thử kết hợp hai item và trả về item kết quả nếu có công thức.
    /// </summary>
    /// <param name="item1">Item thứ nhất.</param>
    /// <param name="item2">Item thứ hai.</param>
    /// <returns>Item kết quả nếu thành công, ngược lại là null.</returns>
    public Item TryCraft(Item item1, Item item2)
    {
        if (item1 == null || item2 == null || item1 == item2) return null;

        // Lặp qua tất cả công thức
        foreach (var recipe in craftingRecipes)
        {
            // Kiểm tra xem item1 và item2 có khớp với itemA và itemB của công thức không
            // Thứ tự không quan trọng: (A, B) hoặc (B, A) đều được chấp nhận
            bool matchA = (recipe.itemA == item1 && recipe.itemB == item2);
            bool matchB = (recipe.itemA == item2 && recipe.itemB == item1);

            if (matchA || matchB)
            {
                // Trả về đối tượng ScriptableObject Item mới.
                return recipe.resultItem;
            }
        }

        return null;
    }
}
