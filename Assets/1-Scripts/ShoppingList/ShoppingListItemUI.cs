using UnityEngine;
using UnityEngine.UI;

public class ShoppingListItemUI : MonoBehaviour
{
    public Text nameText;
    public Text quantityText;
    public SwipeToDeleteItem swipe;

    // Expose the data this prefab represents
    public ShoppingListManager manager;
    public GoogleSheetsShoppingListWriter writer;
    public string listName;
    public ShoppingItem item;

    public void Setup(ShoppingListManager manager, GoogleSheetsShoppingListWriter writer, string listName, ShoppingItem item)
    {
        this.manager = manager;
        this.writer = writer;
        this.listName = listName;
        this.item = item;
        this.item.listName = listName;

        Refresh();

        if (swipe == null)
            swipe = GetComponentInChildren<SwipeToDeleteItem>();
        if (swipe != null)
            swipe.onDelete.AddListener(OnDelete);
    }

    // Update texts to reflect the current item data
    public void Refresh()
    {
        if (nameText != null)
            nameText.text = item != null ? item.name : string.Empty;
        if (quantityText != null)
            quantityText.text = item != null ? item.quantity.ToString() : string.Empty;
    }

    void OnDelete()
    {
        if (manager != null)
        {
            manager.RemoveItem(listName, item.name);
            if (writer != null)
                writer.UploadList(manager);
        }
    }

    void OnDestroy()
    {
        if (swipe != null)
            swipe.onDelete.RemoveListener(OnDelete);
    }
}
