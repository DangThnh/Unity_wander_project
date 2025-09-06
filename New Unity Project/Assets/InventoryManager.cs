using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Singleton instance
    public static InventoryManager instance;

    // Danh sách các item trong kho đồ
    public List<Item> items = new List<Item>();

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
}