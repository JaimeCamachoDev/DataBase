using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"
using System.IO;
using Microsoft.VisualBasic.FileIO;

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
    [Tooltip("Column used for the item completion state (optional)")]
    public string completedHeader = "Completed";
    [Tooltip("Column used for the item id (optional)")]
    public string idHeader = "Id";
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

        using (var reader = new StringReader(request.downloadHandler.text))
        using (var parser = new TextFieldParser(reader))
        {
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            if (parser.EndOfData)
                yield break;

            // Trim the header line and each column title to avoid issues with
            // Windows line endings or extra whitespace that prevent column
            // detection (e.g. "Units\r" not matching "Units").
            string[] headers = parser.ReadFields();
            for (int i = 0; i < headers.Length; i++)
                headers[i] = StripQuotes(headers[i]);

            int listCol = System.Array.IndexOf(headers, listHeader);
            int itemCol = System.Array.IndexOf(headers, itemHeader);
            int qtyCol = System.Array.IndexOf(headers, quantityHeader);
            int posCol = System.Array.IndexOf(headers, positionHeader);
            int completedCol = System.Array.IndexOf(headers, completedHeader);
            int idCol = System.Array.IndexOf(headers, idHeader);

            manager.BeginUpdate();
            manager.Clear();

            int row = 1; // header row already read
            while (!parser.EndOfData)
            {
                string[] values = parser.ReadFields();
                if (values == null || values.Length == 0)
                    continue;

                row++;

                string listName = listCol >= 0 && listCol < values.Length ? StripQuotes(values[listCol]) : defaultListName;
                string itemName = itemCol >= 0 && itemCol < values.Length ? StripQuotes(values[itemCol]) : string.Empty;
                string qtyStr = qtyCol >= 0 && qtyCol < values.Length ? StripQuotes(values[qtyCol]) : "0";
                string posStr = posCol >= 0 && posCol < values.Length ? StripQuotes(values[posCol]) : "-1";
                string completedStr = completedCol >= 0 && completedCol < values.Length ? StripQuotes(values[completedCol]) : "false";
                string id = idCol >= 0 && idCol < values.Length ? StripQuotes(values[idCol]) : null;

                int qty = 0;
                int.TryParse(qtyStr, out qty);
                int pos = -1;
                int.TryParse(posStr, out pos);
                bool completed = false;
                bool.TryParse(completedStr, out completed);

                if (string.IsNullOrEmpty(itemName))
                    continue;

                int column = itemCol >= 0 ? itemCol + 1 : -1;
                manager.AddItem(listName, itemName, qty, pos, row, column, completed, id);
            }

            manager.EndUpdate();
        }

        Debug.Log("Loaded shopping lists from sheet");
    }

    private static string StripQuotes(string value)
    {
        return value.Trim().Trim('"');
    }
}
