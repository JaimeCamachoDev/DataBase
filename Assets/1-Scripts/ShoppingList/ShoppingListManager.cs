using System.Collections.Generic;
using UnityEngine;

public class ShoppingListManager : MonoBehaviour
{
    public List<ShoppingList> lists = new List<ShoppingList>();

    public void AddList(string name)
    {
        lists.Add(new ShoppingList { name = name });
    }

    public void AddItem(string listName, string itemName, int quantity, int position = -1)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null)
        {
            list = new ShoppingList { name = listName };
            lists.Add(list);
        }

        var item = new ShoppingItem { name = itemName, quantity = quantity };

        if (position >= 0 && position <= list.items.Count)
            list.items.Insert(position, item);
        else
            list.items.Add(item);

        for (int i = 0; i < list.items.Count; i++)
            list.items[i].position = i;
    }

    public void RemoveItem(string listName, string itemName)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null) return;

        var item = list.items.Find(i => i.name == itemName);
        if (item == null) return;

        list.items.Remove(item);

        for (int i = 0; i < list.items.Count; i++)
            list.items[i].position = i;
    }
}
