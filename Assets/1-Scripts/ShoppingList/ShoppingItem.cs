using System;

/// <summary>
/// Represents a single entry in a shopping list and mirrors the columns in
/// the Google Sheet. Instances of this class are serialized to JSON when
/// communicating with the Apps Script.
/// </summary>
[Serializable]
public class ShoppingItem
{
    /// <summary>Unique identifier used to match items between Unity and the sheet.</summary>
    public string id;

    /// <summary>Name of the product.</summary>
    public string name;

    /// <summary>Number of units to buy.</summary>
    public int quantity;

    /// <summary>Whether the item has been marked as completed.</summary>
    public bool completed;

    /// <summary>Position within its list for UI ordering.</summary>
    public int position;

    /// <summary>Name of the list this item belongs to.</summary>
    public string listName;

    /// <summary>Row and column inside the Google Sheet (1-based indexes).</summary>
    public int row;
    public int column;
}
