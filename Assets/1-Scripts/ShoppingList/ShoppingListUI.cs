using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShoppingListUI : MonoBehaviour
{
    [Header("References")]
    public ShoppingListManager manager;
    public TMP_InputField listInput;
    public TMP_InputField itemInput;
    public TMP_InputField quantityInput;
    public TMP_InputField positionInput;
    public Transform itemContainer;
    public Transform completedItemContainer;
    public GameObject itemPrefab;
    public ShoppingListItemEditorUI editor;

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
        string itemName = string.IsNullOrEmpty(itemInput.text) ? "Item" : itemInput.text;
        int qty = 0;
        if (!int.TryParse(quantityInput.text, out qty))
            qty = 0;
        int pos = -1;
        if (positionInput != null && !int.TryParse(positionInput.text, out pos))
            pos = -1;
        manager.AddItem(listName, itemName, qty, pos);
        itemInput.text = string.Empty;
        quantityInput.text = string.Empty;
        if (positionInput != null)
            positionInput.text = string.Empty;
    }

    public void RemoveItem()
    {
        if (manager == null) return;
        string listName = string.IsNullOrEmpty(listInput.text) ? "List" : listInput.text;
        var list = manager.lists.Find(l => l.name == listName);
        var item = list != null ? list.items.Find(i => i.name == itemInput.text) : null;
        if (item != null)
            manager.RemoveItem(listName, item.id);
    }

    public void RebuildItems()
    {
        if (manager == null || itemContainer == null || itemPrefab == null) return;

        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);
        if (completedItemContainer != null)
        {
            foreach (Transform child in completedItemContainer)
                Destroy(child.gameObject);
        }

        foreach (var list in manager.lists)
        {
            foreach (var item in list.items)
            {
                Transform parent = item.completed && completedItemContainer != null ? completedItemContainer : itemContainer;
                GameObject go = Instantiate(itemPrefab, parent);
                go.transform.SetSiblingIndex(item.position);
                var ui = go.GetComponentInChildren<ShoppingListItemUI>();
                if (ui != null)
                    ui.Setup(manager, list.name, item, editor);
            }
        }

        Canvas.ForceUpdateCanvases();
        var parentRect = itemContainer != null ? itemContainer.parent as RectTransform : null;
        if (itemContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer as RectTransform);
        if (completedItemContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(completedItemContainer as RectTransform);
        if (parentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
    }
}
