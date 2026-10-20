using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShoppingListItemEditorUI : MonoBehaviour
{
    public ShoppingListManager manager;
    public TMP_InputField nameInput;
    public TMP_InputField quantityInput;
    public Toggle completedToggle;
    public Button applyButton;

    private ShoppingListItemUI currentItemUI;

    void Awake()
    {
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplyChanges);
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (applyButton != null)
            applyButton.onClick.RemoveListener(ApplyChanges);
    }

    public void EditItem(ShoppingListItemUI ui)
    {
        currentItemUI = ui;
        if (manager == null)
            manager = FindAnyObjectByType<ShoppingListManager>();

        if (currentItemUI != null)
        {
            if (nameInput != null)
                nameInput.text = currentItemUI.item.name;
            if (quantityInput != null)
                quantityInput.text = currentItemUI.item.quantity.ToString();
            if (completedToggle != null)
                completedToggle.isOn = currentItemUI.item.completed;
            gameObject.SetActive(true);
        }
    }

    void ApplyChanges()
    {
        if (currentItemUI == null || manager == null)
        {
            gameObject.SetActive(false);
            return;
        }

        int qty = currentItemUI.item.quantity;
        if (quantityInput != null && !int.TryParse(quantityInput.text, out qty))
            qty = currentItemUI.item.quantity;

        string newName = nameInput != null ? nameInput.text : currentItemUI.item.name;
        bool completed = completedToggle != null && completedToggle.isOn;

        manager.UpdateItem(currentItemUI.listName, currentItemUI.item, newName, qty, completed);
        currentItemUI.Refresh();
        gameObject.SetActive(false);
        currentItemUI = null;
    }
}
