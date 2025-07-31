using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetsShoppingListWriter : MonoBehaviour
{
    [Tooltip("URL of the Apps Script that updates the sheet")] 
    public string scriptUrl;

    public void UploadList(ShoppingListManager manager)
    {
        if (manager != null && !string.IsNullOrEmpty(scriptUrl))
            StartCoroutine(UploadCoroutine(manager.lists));
    }

    IEnumerator UploadCoroutine(List<ShoppingList> lists)
    {
        var wrapper = new Wrapper { lists = lists };
        string json = JsonUtility.ToJson(wrapper);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(scriptUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError($"Error uploading list: {request.error}");
        else
            Debug.Log("Sheet updated");
    }

    [System.Serializable]
    class Wrapper
    {
        public List<ShoppingList> lists;
    }
}
