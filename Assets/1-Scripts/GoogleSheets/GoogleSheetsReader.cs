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
            Debug.Log($"Sheet data:\n{data}");
            // TODO: parse CSV data as needed
        }
    }
}
