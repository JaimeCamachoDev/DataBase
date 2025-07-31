using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingListUI : MonoBehaviour
{
    [Header("References")]
    public ShoppingListManager manager;
    public InputField listInput;
    public InputField itemInput;
    public InputField quantityInput;
    public Text displayText;
    public GoogleSheetsShoppingListWriter writer;

    void Start()
    {
        RefreshDisplay();
    }

    public void AddItem()
    {
        if (manager == null) return;
        string listName = string.IsNullOrEmpty(listInput.text) ? "List" : listInput.text;
        string itemName = itemInput.text;
        if (string.IsNullOrEmpty(itemName)) return;
        int qty = 0;
        int.TryParse(quantityInput.text, out qty);
        manager.AddItem(listName, itemName, qty);
        RefreshDisplay();
        if (writer != null)
            writer.UploadList(manager);
    }

    public void RemoveItem()
    {
        if (manager == null) return;
        string listName = string.IsNullOrEmpty(listInput.text) ? "List" : listInput.text;
        var list = manager.lists.Find(l => l.name == listName);
        if (list == null) return;
        var item = list.items.Find(i => i.name == itemInput.text);
        if (item != null)
            list.items.Remove(item);
        RefreshDisplay();
        if (writer != null)
            writer.UploadList(manager);
    }

    public void RefreshDisplay()
    {
        if (displayText == null || manager == null) return;
        StringBuilder sb = new StringBuilder();
        foreach (var list in manager.lists)
        {
            sb.AppendLine(list.name);
            foreach (var item in list.items)
            {
                sb.AppendFormat("- {0} ({1})\n", item.name, item.quantity);
            }
        }
        displayText.text = sb.ToString();
    }
}
