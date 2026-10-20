using System;
using System.Collections.Generic;

[Serializable]
public class ShoppingList
{
    public string name;
    public List<ShoppingItem> items = new List<ShoppingItem>();
}
