using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"

public class GoogleSheetsShoppingListLoader : MonoBehaviour
{
    [Tooltip("Manager to receive loaded lists")]
    public ShoppingListManager manager;

    [Tooltip("Public Google Sheets export link")]
    public string sheetUrl;

    [Header("Column titles")]                // allows using custom headers
    [Tooltip("Column used for the list name (optional)")]
    public string listHeader = "List";
    [Tooltip("Column used for the item name")]
    public string itemHeader = "Item";
    [Tooltip("Column used for the item quantity")]
    public string quantityHeader = "Units";
    [Tooltip("Name used when no list column is present")]
    public string defaultListName = "List";

    void Start()
    {
        if (manager != null && !string.IsNullOrEmpty(sheetUrl))
            StartCoroutine(Load());
        else
            Debug.LogWarning("Loader requires a manager and sheet URL");
    }

    IEnumerator Load()
    {
        UnityWebRequest request = UnityWebRequest.Get(sheetUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error reading sheet: {request.error}");
            yield break;
        }

        string[] lines = request.downloadHandler.text.Split('\n');
        if (lines.Length == 0)
            yield break;

        string[] headers = lines[0].Split(',');
        int listCol = System.Array.IndexOf(headers, listHeader);
        int itemCol = System.Array.IndexOf(headers, itemHeader);
        int qtyCol = System.Array.IndexOf(headers, quantityHeader);

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');
            string listName = listCol >= 0 && listCol < values.Length ? values[listCol].Trim() : defaultListName;
            string itemName = itemCol >= 0 && itemCol < values.Length ? values[itemCol].Trim() : string.Empty;
            string qtyStr = qtyCol >= 0 && qtyCol < values.Length ? values[qtyCol].Trim() : "0";
            int qty = 0;
            int.TryParse(qtyStr, out qty);

            if (string.IsNullOrEmpty(itemName))
                continue;

            manager.AddItem(listName, itemName, qty);
        }

        Debug.Log("Loaded shopping lists from sheet");
    }
}
