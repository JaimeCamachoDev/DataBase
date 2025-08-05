using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Central store of all shopping lists. Handles persistence and notifies
/// listeners whenever the data changes.
/// </summary>
public class ShoppingListManager : MonoBehaviour
{
    /// <summary>All lists currently loaded in memory.</summary>
    public List<ShoppingList> lists = new List<ShoppingList>();

    /// <summary>Event fired whenever the contents of <see cref="lists"/> change.</summary>
    public event Action ListsChanged;

    // Prevents event spam during bulk updates
    bool suppressEvents = false;

    // Indicates that data changed while events were suppressed
    bool needsSave = false;

    // Location on disk where data is saved between sessions
    string SavePath => Path.Combine(Application.persistentDataPath, "shoppingLists.json");

    void Awake() => LoadFromDisk();

    /// <summary>Helper that notifies listeners and persists data to disk.</summary>
    void NotifyChanged()
    {
        if (!suppressEvents)
            ListsChanged?.Invoke();
        else
            needsSave = true;

        SaveToDisk();
    }

    /// <summary>Call before performing many changes to silence events temporarily.</summary>
    public void BeginUpdate()
    {
        suppressEvents = true;
        needsSave = false;
    }

    /// <summary>Re-enables events and optionally saves once if changes occurred.</summary>
    public void EndUpdate()
    {
        suppressEvents = false;
        if (needsSave)
        {
            needsSave = false;
            SaveToDisk();
        }

        ListsChanged?.Invoke();
    }

    /// <summary>Remove all lists and notify listeners.</summary>
    public void Clear()
    {
        lists.Clear();
        NotifyChanged();
    }

    /// <summary>Create a new empty list.</summary>
    public void AddList(string name)
    {
        lists.Add(new ShoppingList { name = name });
        NotifyChanged();
    }

    /// <summary>Add a new item to the specified list.</summary>
    public void AddItem(string listName, string itemName, int quantity, int position = -1, int row = -1, int column = -1, bool completed = false, string id = null)
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
            column = column
        };

        if (position >= 0 && position <= list.items.Count)
            list.items.Insert(position, item);
        else
            list.items.Add(item);

        for (int i = 0; i < list.items.Count; i++)
            list.items[i].position = i;

        NotifyChanged();
    }

    /// <summary>Toggle the completion state of an item.</summary>
    public void SetItemCompleted(string listName, string itemId, bool completed)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null) return;

        var item = list.items.Find(i => i.id == itemId);
        if (item == null) return;

        item.completed = completed;
        NotifyChanged();
    }

    /// <summary>Modify the fields of an existing item.</summary>
    public void UpdateItem(string listName, ShoppingItem item, string newName, int quantity, bool completed)
    {
        var list = lists.Find(l => l.name == listName);
        if (list == null) return;

        var target = list.items.Find(i => i == item);
        if (target == null) return;

        target.name = newName;
        target.quantity = quantity;
        target.completed = completed;
        NotifyChanged();
    }

    /// <summary>Remove an item from its list.</summary>
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

    /// <summary>Serialize the lists to disk so they persist between sessions.</summary>
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

    /// <summary>Load previously saved lists from disk.</summary>
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
