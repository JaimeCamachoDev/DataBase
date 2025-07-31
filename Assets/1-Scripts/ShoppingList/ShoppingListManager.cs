using System.Collections.Generic;
using UnityEngine;

public class ShoppingListManager : MonoBehaviour
{
    public List<ShoppingList> lists = new List<ShoppingList>();

    public void AddList(string name)
    {
        lists.Add(new ShoppingList { name = name });
    }

    public void AddItem(string listName, string itemName, int quantity)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null)
        {
            list = new ShoppingList { name = listName };
            lists.Add(list);
        }
        list.items.Add(new ShoppingItem { name = itemName, quantity = quantity });
    }
}
