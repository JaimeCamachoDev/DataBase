using UnityEngine;
using UnityEngine.UI;

public class ShoppingListItemUI : MonoBehaviour
{
    public Text nameText;
    public Text quantityText;
    public SwipeToDeleteItem swipe;

    ShoppingListManager manager;
    GoogleSheetsShoppingListWriter writer;
    string listName;
    ShoppingItem item;

    public void Setup(ShoppingListManager manager, GoogleSheetsShoppingListWriter writer, string listName, ShoppingItem item)
    {
        this.manager = manager;
        this.writer = writer;
        this.listName = listName;
        this.item = item;

        if (nameText != null)
            nameText.text = item.name;
        if (quantityText != null)
            quantityText.text = item.quantity.ToString();

        if (swipe == null)
            swipe = GetComponentInChildren<SwipeToDeleteItem>();
        if (swipe != null)
            swipe.onDelete.AddListener(OnDelete);
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
