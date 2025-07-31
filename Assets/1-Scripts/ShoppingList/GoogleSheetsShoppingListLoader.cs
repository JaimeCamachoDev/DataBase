using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"

public class GoogleSheetsShoppingListLoader : MonoBehaviour
{
    [Tooltip("Manager to receive loaded lists")]
    public ShoppingListManager manager;

    [Tooltip("Public Google Sheets export link")]
    public string sheetUrl;

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
        int listCol = System.Array.IndexOf(headers, "List");
        int itemCol = System.Array.IndexOf(headers, "Item");
        int qtyCol = System.Array.IndexOf(headers, "Units");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');
            string listName = listCol >= 0 && listCol < values.Length ? values[listCol].Trim() : string.Empty;
            string itemName = itemCol >= 0 && itemCol < values.Length ? values[itemCol].Trim() : string.Empty;
            string qtyStr = qtyCol >= 0 && qtyCol < values.Length ? values[qtyCol].Trim() : "0";
            int qty = 0;
            int.TryParse(qtyStr, out qty);

            if (!string.IsNullOrEmpty(listName) && !string.IsNullOrEmpty(itemName))
            {
                manager.AddItem(listName, itemName, qty);
            }
        }

        Debug.Log("Loaded shopping lists from sheet");
    }
}
