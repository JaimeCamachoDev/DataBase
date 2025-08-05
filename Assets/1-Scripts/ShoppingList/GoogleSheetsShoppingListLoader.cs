using CsvHelper;
using CsvHelper.Configuration;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking; // requires the built‑in "Unity Web Request" package

/// <summary>
/// Downloads a public Google Sheet in CSV format and fills a
/// <see cref="ShoppingListManager"/> with the parsed items.
/// </summary>
public class GoogleSheetsShoppingListLoader : MonoBehaviour
{
    /// <summary>Manager that will receive the loaded data.</summary>
    [Tooltip("Manager to receive loaded lists")]
    public ShoppingListManager manager;

    /// <summary>Public CSV export link of the Google Sheet.</summary>
    [Tooltip("Public Google Sheets export link")]
    public string sheetUrl;

    [Header("Column titles")] // allows using custom headers
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

    /// <summary>Reload data every N seconds (0 disables auto refresh).</summary>
    [Tooltip("Reload data every N seconds (0 disables auto refresh)")]
    public float refreshInterval = 0f;

    void Start()
    {
        if (manager != null && !string.IsNullOrEmpty(sheetUrl))
        {
            // Initial load of the sheet
            Refresh();

            // Optionally reload periodically
            if (refreshInterval > 0f)
                StartCoroutine(RefreshPeriodically());
        }
        else
        {
            Debug.LogWarning("Loader requires a manager and sheet URL");
        }
    }

    /// <summary>Manually trigger a refresh of the sheet.</summary>
    public void Refresh()
    {
        if (manager != null)
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

    /// <summary>Downloads the CSV and fills the manager with the parsed rows.</summary>
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
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        }))
        {
            // Nothing to do if the sheet is empty
            if (!csv.Read())
                yield break;

            csv.ReadHeader();
            var headers = csv.HeaderRecord;
            for (int i = 0; i < headers.Length; i++)
                headers[i] = StripQuotes(headers[i]);

            int listCol = System.Array.IndexOf(headers, listHeader);
            int itemCol = System.Array.IndexOf(headers, itemHeader);
            int qtyCol = System.Array.IndexOf(headers, quantityHeader);
            int posCol = System.Array.IndexOf(headers, positionHeader);
            int completedCol = System.Array.IndexOf(headers, completedHeader);
            int idCol = System.Array.IndexOf(headers, idHeader);

            // Avoid spamming change events while rebuilding the list
            manager.BeginUpdate();
            manager.Clear();

            int row = 1; // header row already read
            while (csv.Read())
            {
                var values = csv.Context.Record;
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

    static string StripQuotes(string value) => value == null ? null : value.Trim().Trim('"');
}

