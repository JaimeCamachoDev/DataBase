using System.Collections;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

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
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim
            }))
            {
                if (!csv.Read())
                {
                    Debug.LogWarning("Sheet is empty");
                    yield break;
                }

                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                while (csv.Read())
                {
                    var record = csv.Context.Record;
                    if (record == null || record.Length == 0)
                        continue;

                    System.Text.StringBuilder row = new System.Text.StringBuilder();

                    for (int j = 0; j < headers.Length && j < record.Length; j++)
                    {
                        if (j > 0) row.Append(", ");
                        row.AppendFormat("{0}: {1}", headers[j].Trim(), record[j].Trim());
                    }

                    Debug.Log(row.ToString());
                }
            }
        }
    }
}