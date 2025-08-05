using UnityEngine;
using UnityEngine.UI;

public class ShoppingListUI : MonoBehaviour
{
    [Header("Referencias")]
    public ShoppingListManager manager;
    public Transform itemContainer;
    public Transform completedItemContainer;
    public GameObject itemPrefab;

    void Start()
    {
        // Nos suscribimos al evento para refrescar la interfaz cuando cambien las listas
        if (manager != null)
            manager.ListsChanged += RebuildItems;
        // Construimos la lista inicial
        RebuildItems();
    }

    void OnDestroy()
    {
        // Cancelamos la suscripción al destruir el objeto
        if (manager != null)
            manager.ListsChanged -= RebuildItems;
    }

    public void AddItem()
    {
        // Añade un elemento con valores por defecto
        if (manager == null) return;
        manager.AddItem("List", "Item", 0, -1);
    }

    public void RebuildItems()
    {
        // Si faltan referencias no hacemos nada
        if (manager == null || itemContainer == null || itemPrefab == null) return;

        // Eliminamos elementos anteriores
        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);
        if (completedItemContainer != null)
        {
            foreach (Transform child in completedItemContainer)
                Destroy(child.gameObject);
        }

        // Instanciamos un prefab por cada item de cada lista
        foreach (var list in manager.lists)
        {
            foreach (var item in list.items)
            {
                Transform parent = item.completed && completedItemContainer != null ? completedItemContainer : itemContainer;
                GameObject go = Instantiate(itemPrefab, parent);
                go.transform.SetSiblingIndex(item.position);
                var ui = go.GetComponentInChildren<ShoppingListItemUI>();
                if (ui != null)
                    ui.Setup(manager, list.name, item);
            }
        }

        // Forzamos el recalculo del layout para que la UI se actualice
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
