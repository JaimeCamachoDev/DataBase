using System;
using System.Collections.Generic;

/// <summary>
/// Container for a group of <see cref="ShoppingItem"/> objects belonging to
/// the same list name.
/// </summary>
[Serializable]
public class ShoppingList
{
    /// <summary>Display name of the list.</summary>
    public string name;

    /// <summary>Items that belong to this list.</summary>
    public List<ShoppingItem> items = new List<ShoppingItem>();
}
