using System;

[Serializable]
public class ShoppingItem
{
    public string id;
    public string name;
    public int quantity;
    public bool completed;
    public int position;
    // Name of the list this item belongs to
    public string listName;
    // Row and column location inside the Google Sheet (1-based indexes)
    public int row;
    public int column;
    // ISO 8601 timestamp of last modification
    public string updated;
}
