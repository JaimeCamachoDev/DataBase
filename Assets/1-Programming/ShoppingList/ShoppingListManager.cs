using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ShoppingListManager : MonoBehaviour
{
    public List<ShoppingList> lists = new List<ShoppingList>();

    public event Action ListsChanged;

    bool suppressEvents = false;
    string SavePath => Path.Combine(Application.persistentDataPath, "shoppingLists.json");

    void Awake()
    {
        LoadFromDisk();
    }

    void NotifyChanged()
    {
        if (!suppressEvents)
            ListsChanged?.Invoke();
        SaveToDisk();
    }

    public void BeginUpdate() => suppressEvents = true;

    public void EndUpdate()
    {
        suppressEvents = false;
        NotifyChanged();
    }

    public void Clear()
    {
        lists.Clear();
        NotifyChanged();
    }

    public void AddList(string name)
    {
        lists.Add(new ShoppingList { name = name });
        NotifyChanged();
    }

    public void AddItem(string listName, string itemName, int quantity, int position = -1, int row = -1, int column = -1, bool completed = false, string id = null, string updated = null)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null)
        {
            list = new ShoppingList { name = listName };
            lists.Add(list);
        }

        var item = new ShoppingItem
        {
            id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id,
            name = itemName,
            quantity = quantity,
            completed = completed,
            listName = listName,
            row = row,
            column = column,
            updated = string.IsNullOrEmpty(updated) ? DateTime.UtcNow.ToString("o") : updated
        };

        if (position >= 0 && position <= list.items.Count)
            list.items.Insert(position, item);
        else
            list.items.Add(item);

        for (int i = 0; i < list.items.Count; i++)
            list.items[i].position = i;

        NotifyChanged();
    }

    public void SetItemCompleted(string listName, string itemId, bool completed)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null) return;

        var item = list.items.Find(i => i.id == itemId);
        if (item == null) return;

        item.completed = completed;
        item.updated = DateTime.UtcNow.ToString("o");
        NotifyChanged();
    }

    public void UpdateItem(string listName, ShoppingItem item, string newName, int quantity, bool completed)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null) return;

        var target = list.items.Find(i => i == item);
        if (target == null) return;

        target.name = newName;
        target.quantity = quantity;
        target.completed = completed;
        target.updated = DateTime.UtcNow.ToString("o");
        NotifyChanged();
    }

    public void RemoveItem(string listName, string itemId)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null) return;

        var item = list.items.Find(i => i.id == itemId);
        if (item == null) return;

        list.items.Remove(item);

        for (int i = 0; i < list.items.Count; i++)
            list.items[i].position = i;

        NotifyChanged();
    }

    void SaveToDisk()
    {
        try
        {
            var wrapper = new ListWrapper { lists = this.lists };
            var json = JsonUtility.ToJson(wrapper);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving lists: {e.Message}");
        }
    }

    void LoadFromDisk()
    {
        if (!File.Exists(SavePath))
            return;
        try
        {
            string json = File.ReadAllText(SavePath);
            var wrapper = JsonUtility.FromJson<ListWrapper>(json);
            if (wrapper != null && wrapper.lists != null)
            {
                BeginUpdate();
                lists = wrapper.lists;
                EndUpdate();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading lists: {e.Message}");
        }
    }

    [Serializable]
    class ListWrapper
    {
        public List<ShoppingList> lists;
    }
}
