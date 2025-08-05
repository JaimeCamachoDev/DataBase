using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetsShoppingListWriter : MonoBehaviour
{
    [Tooltip("URL of the Apps Script that updates the sheet")]
    public string scriptUrl;

    [Tooltip("Manager whose data will be uploaded")]
    public ShoppingListManager manager;

    bool uploadInProgress;
    bool pendingUpload;

    void Start()
    {
        Application.runInBackground = true;
        if (manager == null)
            manager = FindAnyObjectByType<ShoppingListManager>();
        if (manager != null)
        {
            manager.ListsChanged += OnListsChanged;
            // Push current data so new columns like Position/Completed exist
            QueueUpload();
        }
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

    void OnApplicationQuit()
    {
        QueueUpload();
    }

    void OnListsChanged() => QueueUpload();

    void QueueUpload()
    {
        if (manager == null || string.IsNullOrEmpty(scriptUrl)) return;
        pendingUpload = true;
        if (!uploadInProgress)
            StartCoroutine(UploadSequential());
    }

    IEnumerator UploadSequential()
    {
        while (pendingUpload)
        {
            pendingUpload = false;
            uploadInProgress = true;
            yield return UploadCoroutine(manager.lists);
            uploadInProgress = false;
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator UploadCoroutine(List<ShoppingList> lists)
    {
        var serializableLists = new List<SerializableList>();
        foreach (var list in lists)
        {
            var sList = new SerializableList { name = list.name, items = new List<SerializableItem>() };
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
