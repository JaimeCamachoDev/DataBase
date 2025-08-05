using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"
using System.IO;
using Microsoft.VisualBasic.FileIO;

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
            using (var reader = new StringReader(data))
            using (var parser = new TextFieldParser(reader))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                if (parser.EndOfData)
                {
                    Debug.LogWarning("Sheet is empty");
                    yield break;
                }

                // Use the first row as column names
                string[] headers = parser.ReadFields();

                while (!parser.EndOfData)
                {
                    string[] values = parser.ReadFields();
                    if (values == null || values.Length == 0)
                        continue;

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
}
