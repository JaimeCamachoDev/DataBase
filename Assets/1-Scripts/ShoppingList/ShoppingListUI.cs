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
    public Transform itemContainer;
    public GameObject itemPrefab;
    public GoogleSheetsShoppingListWriter writer;

    void Start()
    {
        if (manager != null)
            manager.ListsChanged += RebuildItems;
        RebuildItems();
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.ListsChanged -= RebuildItems;
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
        if (writer != null)
            writer.UploadList(manager);
    }

    public void RemoveItem()
    {
        if (manager == null) return;
        string listName = string.IsNullOrEmpty(listInput.text) ? "List" : listInput.text;
        manager.RemoveItem(listName, itemInput.text);
        if (writer != null)
            writer.UploadList(manager);
    }

    public void RebuildItems()
    {
        if (manager == null || itemContainer == null || itemPrefab == null) return;

        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        foreach (var list in manager.lists)
        {
            foreach (var item in list.items)
            {
                GameObject go = Instantiate(itemPrefab, itemContainer);
                go.transform.SetSiblingIndex(item.position);
                var ui = go.GetComponentInChildren<ShoppingListItemUI>();
                if (ui != null)
                    ui.Setup(manager, writer, list.name, item);
            }
        }
    }
}
