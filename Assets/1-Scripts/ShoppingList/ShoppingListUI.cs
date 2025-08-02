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
    public InputField positionInput;
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
        int pos = -1;
        if (positionInput != null)
            int.TryParse(positionInput.text, out pos);
        manager.AddItem(listName, itemName, qty, pos);
        RefreshDisplay();
        if (writer != null)
            writer.UploadList(manager);
    }

    public void RemoveItem()
    {
        if (manager == null) return;
        string listName = string.IsNullOrEmpty(listInput.text) ? "List" : listInput.text;
        manager.RemoveItem(listName, itemInput.text);
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
                sb.AppendFormat("{0}. {1} ({2})\n", item.position, item.name, item.quantity);
            }
        }
        displayText.text = sb.ToString();
    }
}
