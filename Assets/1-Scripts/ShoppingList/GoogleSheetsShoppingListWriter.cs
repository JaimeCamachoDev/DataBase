using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Sends the contents of <see cref="ShoppingListManager"/> to an Apps Script
/// endpoint so the backing Google Sheet can be updated.
/// </summary>
public class GoogleSheetsShoppingListWriter : MonoBehaviour
{
    /// <summary>URL of the published Google Apps Script that receives the data.</summary>
    [Tooltip("URL of the Apps Script that updates the sheet")]
    public string scriptUrl;

    /// <summary>Manager providing the lists to upload.</summary>
    [Tooltip("Manager whose data will be uploaded")]
    public ShoppingListManager manager;

    // Flags to ensure only one upload runs at a time
    bool uploadInProgress;
    bool pendingUpload;

    void Start()
    {
        // Keep running even if the app loses focus so pending uploads finish.
        Application.runInBackground = true;

        // Auto‑assign the manager if none was manually set in the Inspector.
        if (manager == null)
            manager = FindAnyObjectByType<ShoppingListManager>();

        // Subscribe to changes so we upload whenever the local data changes.
        if (manager != null)
            manager.ListsChanged += OnListsChanged;
    }

    void OnDestroy()
    {
        if (manager != null)
            manager.ListsChanged -= OnListsChanged;
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            QueueUpload();
    }

    void OnApplicationQuit() => QueueUpload();

    // Called whenever the manager notifies a change.
    void OnListsChanged() => QueueUpload();

    /// <summary>Queues an upload if one isn't already running.</summary>
    void QueueUpload()
    {
        if (manager == null || string.IsNullOrEmpty(scriptUrl))
            return;

        pendingUpload = true;
        if (!uploadInProgress)
            StartCoroutine(UploadSequential());
    }

    /// <summary>Runs queued uploads sequentially.</summary>
    IEnumerator UploadSequential()
    {
        while (pendingUpload)
        {
            pendingUpload = false;
            uploadInProgress = true;
            yield return UploadCoroutine(manager.lists);
            uploadInProgress = false;

            // Small delay to prevent hammering the server if many updates occur.
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>Serializes the lists and posts them to the Apps Script.</summary>
    IEnumerator UploadCoroutine(List<ShoppingList> lists)
    {
        var serializableLists = new List<SerializableList>();
        foreach (var list in lists)
        {
            var sList = new SerializableList
            {
                name = list.name,
                items = new List<SerializableItem>()
            };

            foreach (var item in list.items)
            {
                sList.items.Add(new SerializableItem
                {
                    id = item.id,
                    name = item.name,
                    quantity = item.quantity,
                    position = item.position,
                    completed = item.completed
                });
            }

            serializableLists.Add(sList);
        }

        var wrapper = new Wrapper { lists = serializableLists };
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
        public List<SerializableList> lists;
    }

    [System.Serializable]
    class SerializableList
    {
        public string name;
        public List<SerializableItem> items;
    }

    [System.Serializable]
    class SerializableItem
    {
        public string id;
        public string name;
        public int quantity;
        public int position;
        public bool completed;
    }
}
