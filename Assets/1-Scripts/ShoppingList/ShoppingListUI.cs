using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShoppingListUI : MonoBehaviour
{
    [Header("References")]
    public ShoppingListManager manager;
    //public TMP_InputField listInput;
    //public TMP_InputField itemInput;
    //public TMP_InputField quantityInput;
    //public TMP_InputField positionInput;
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



        string listName = "Lista";
        string itemName = "Nuevo Item";
        int qty = 1;
        int pos = -1;


        manager.AddItem(listName, itemName, qty, pos);
        //itemInput.text = string.Empty;
        //quantityInput.text = string.Empty;
        //if (positionInput != null)
        //    positionInput.text = string.Empty;
    }

    public void RemoveItem()
    {
        if (manager == null) return;
        string listName = "Lista";
        var list = manager.lists.Find(l => l.name == listName);
        var item = list != null && list.items.Count > 0 ? list.items[list.items.Count - 1] : null;
        if (item != null)
            manager.RemoveItem(listName, item.id);
    }

    public void RebuildItems()
    {
        if (manager == null || itemContainer == null || itemPrefab == null) return;

        // Start from a clean slate and recreate the entire UI based on the
        // current data stored in the manager.
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
