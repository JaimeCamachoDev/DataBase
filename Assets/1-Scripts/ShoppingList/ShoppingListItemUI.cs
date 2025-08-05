using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI for a single <see cref="ShoppingItem"/> entry and responds
/// to swipe gestures for delete/complete/edit actions.
/// </summary>
public class ShoppingListItemUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;
    public SwipeToDeleteItem swipe;

    // References to the data this visual element represents
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

    void Start() => Refresh();

    /// <summary>Initializes the UI with the provided data references.</summary>
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

    /// <summary>Updates texts to reflect the current item data.</summary>
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
            manager.RemoveItem(listName, item.id);
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
