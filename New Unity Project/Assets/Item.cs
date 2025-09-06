using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName = "New Item";
    public string uniqueId;
    public Sprite itemIcon = null;
    [TextArea(3, 10)]
    public string itemDescription = "Description of the item.";
}