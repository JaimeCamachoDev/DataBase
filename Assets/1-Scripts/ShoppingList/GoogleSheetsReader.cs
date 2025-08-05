using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"
using System.Text;

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
            yield break;
        }

        string[] lines = request.downloadHandler.text.Split('\n');
        if (lines.Length == 0)
            yield break;

        string[] headers = ParseCsvLine(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values = ParseCsvLine(lines[i]);
            StringBuilder row = new StringBuilder();

            for (int j = 0; j < headers.Length && j < values.Length; j++)
            {
                if (j > 0) row.Append(", ");
                row.AppendFormat("{0}: {1}", headers[j].Trim(), values[j].Trim());
            }

            if (row.Length > 0)
                Debug.Log(row.ToString());
        }
    }

    private string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        StringBuilder field = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Length = 0;
            }
            else if (c != '\r')
            {
                field.Append(c);
            }
        }

        fields.Add(field.ToString());
        return fields.ToArray();
    }
}