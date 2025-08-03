using UnityEngine;
using UnityEngine.UI;

public class ShoppingListItemUI : MonoBehaviour
{
    public Text nameText;
    public Text quantityText;
    public SwipeToDeleteItem swipe;

    // Expose the data this prefab represents
    public ShoppingListManager manager;
    public string listName;
    public ShoppingItem item;

    void Awake()
    {
        if (swipe == null)
            swipe = GetComponentInChildren<SwipeToDeleteItem>();
        if (swipe != null)
        {
            swipe.onDelete.AddListener(OnDelete);
            swipe.onComplete.AddListener(OnComplete);
        }
    }

    void Start()
    {
        if (manager == null)
            manager = FindAnyObjectByType<ShoppingListManager>();
        Refresh();
    }

    public void Setup(ShoppingListManager manager, string listName, ShoppingItem item)
    {
        if (manager != null)
            this.manager = manager;
        this.listName = listName;
        this.item = item;
        if (this.item != null)
            this.item.listName = listName;

        Refresh();
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
        }
    }

    void OnComplete()
    {
        if (manager != null)
        {
            manager.SetItemCompleted(listName, item.name, true);
            Refresh();
        }
    }

    void OnDestroy()
    {
        if (swipe != null)
        {
            swipe.onDelete.RemoveListener(OnDelete);
            swipe.onComplete.RemoveListener(OnComplete);
        }
    }
}
