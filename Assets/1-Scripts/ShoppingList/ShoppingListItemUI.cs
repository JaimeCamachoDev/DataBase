using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShoppingListItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;
    public SwipeToDeleteItem swipe;

    // Expose the data this prefab represents
    [SerializeField] private ShoppingListManager manager;
    [SerializeField] private ShoppingListItemEditorUI editor;
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
            swipe.onClick.AddListener(OnEdit);
        }
    }

    void Start()
    {
        Refresh();
    }

    public void Setup(ShoppingListManager manager, string listName, ShoppingItem item, ShoppingListItemEditorUI editor = null)
    {
        if (manager != null)
            this.manager = manager;
        if (editor != null)
            this.editor = editor;
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
        {
            nameText.text = item != null ? item.name : string.Empty;
            nameText.fontStyle = item != null && item.completed ? FontStyles.Strikethrough : FontStyles.Normal;
            nameText.color = item != null && item.completed ? Color.gray : Color.white;
        }
        if (quantityText != null)
        {
            quantityText.text = item != null ? item.quantity.ToString() : string.Empty;
            quantityText.fontStyle = item != null && item.completed ? FontStyles.Strikethrough : FontStyles.Normal;
            quantityText.color = item != null && item.completed ? Color.gray : Color.white;
        }
    }

    void OnDelete()
    {
        if (manager != null)
        {
            manager.RemoveItem(listName, item.id);
        }
    }

    void OnComplete()
    {
        if (manager != null)
        {
            // Toggle the completed state so repeated swipes can undo
            manager.SetItemCompleted(listName, item.id, !item.completed);
            Refresh();
        }
    }

    void OnEdit()
    {
        if (editor != null)
            editor.EditItem(this);
    }

    void OnDestroy()
    {
        if (swipe != null)
        {
            swipe.onDelete.RemoveListener(OnDelete);
            swipe.onComplete.RemoveListener(OnComplete);
            swipe.onClick.RemoveListener(OnEdit);
        }
    }
}
