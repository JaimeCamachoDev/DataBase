using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"

public class GoogleSheetsReader : MonoBehaviour
{
    [Tooltip("Public Google Sheets export link")] 
    public string sheetUrl = "https://docs.google.com/spreadsheets/d/13PsACix0amVNjdoSWasaFPId-VAtGShmd6gMxgkFNy8/export?format=csv";

    void Start()
    {
        StartCoroutine(GetSheetData());
    }

    IEnumerator GetSheetData()
    {
        UnityWebRequest request = UnityWebRequest.Get(sheetUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error reading sheet: {request.error}");
        }
        else
        {
            string data = request.downloadHandler.text;
            string[] lines = data.Split('\n');

            if (lines.Length == 0)
            {
                Debug.LogWarning("Sheet is empty");
                yield break;
            }

            // Use the first row as column names
            string[] headers = lines[0].Split(',');

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] values = line.Split(',');
                System.Text.StringBuilder row = new System.Text.StringBuilder();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    if (j > 0) row.Append(", ");
                    row.AppendFormat("{0}: {1}", headers[j].Trim(), values[j].Trim());
                }

                Debug.Log(row.ToString());
            }
        }
    }
}
