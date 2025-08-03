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
    [Tooltip("Column used for the item position (optional)")]
    public string positionHeader = "Position";
    [Tooltip("Name used when no list column is present")]
    public string defaultListName = "List";

    [Tooltip("Reload data every N seconds (0 disables auto refresh)")]
    public float refreshInterval = 0f;

    void Start()
    {
        if (manager != null && !string.IsNullOrEmpty(sheetUrl))
        {
            Refresh();
            if (refreshInterval > 0f)
                StartCoroutine(RefreshPeriodically());
        }
        else
            Debug.LogWarning("Loader requires a manager and sheet URL");
    }

    public void Refresh()
    {
        if (manager == null) return;
        StartCoroutine(Load());
    }

    IEnumerator RefreshPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            Refresh();
        }
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
        int posCol = System.Array.IndexOf(headers, positionHeader);

        manager.BeginUpdate();
        manager.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');
            string listName = listCol >= 0 && listCol < values.Length ? StripQuotes(values[listCol]) : defaultListName;

            string itemName = itemCol >= 0 && itemCol < values.Length ? StripQuotes(values[itemCol]) : string.Empty;
            string qtyStr = qtyCol >= 0 && qtyCol < values.Length ? StripQuotes(values[qtyCol]) : "0";
            string posStr = posCol >= 0 && posCol < values.Length ? StripQuotes(values[posCol]) : "-1";
            int qty = 0;
            int.TryParse(qtyStr, out qty);
            int pos = -1;
            int.TryParse(posStr, out pos);

            if (string.IsNullOrEmpty(itemName))
                continue;

            int row = i + 1; // 1-based row index including header
            int column = itemCol >= 0 ? itemCol + 1 : -1;
            manager.AddItem(listName, itemName, qty, pos, row, column);
        }

        manager.EndUpdate();
        Debug.Log("Loaded shopping lists from sheet");
    }

    private static string StripQuotes(string value)
    {
        return value.Trim().Trim('"');
    }
}
