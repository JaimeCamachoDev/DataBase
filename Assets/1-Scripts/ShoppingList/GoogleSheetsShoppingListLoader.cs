using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


/// <summary>
/// Sincroniza la lista de la compra con una hoja de cálculo de Google.
/// 1. Descarga la hoja en formato CSV y rellena <see cref="ShoppingListManager"/>.
/// 2. Escucha cambios y los envía al Apps Script para mantener la hoja actualizada.
/// </summary>
public class GoogleSheetsShoppingListLoader : MonoBehaviour
{
    [Tooltip("Gestor que almacena la lista de la compra")]
    public ShoppingListManager manager;

    [Tooltip("Enlace público de exportación CSV de la hoja")]
    public string sheetUrl;

    [Tooltip("URL del Apps Script que actualiza la hoja")]
    public string scriptUrl;

    [Tooltip("Recargar datos cada N segundos (0 desactiva)")]
    public float refreshInterval = 0f;

    // Nombres fijos de las columnas de la hoja
    const string LIST = "List";
    const string ITEM = "Item";
    const string QTY = "Units";
    const string POS = "Position";
    const string DONE = "Completed";
    const string ID = "Id";
    const string DEFAULT_LIST = "List";

    // Flags para gestionar las subidas
    bool uploadInProgress;
    bool pendingUpload;

    void Start()
    {
        Application.runInBackground = true;

        if (manager == null)
            manager = FindAnyObjectByType<ShoppingListManager>();

        if (manager != null)
            manager.ListsChanged += OnListsChanged;

        if (manager != null && !string.IsNullOrEmpty(sheetUrl))
        {
            // Paso 1: descargar la hoja inicial
            Refresh();

            // Paso 2: recarga periódica opcional
            if (refreshInterval > 0f)
                StartCoroutine(RefreshPeriodically());
        }
        else
        {
            Debug.LogWarning("El cargador necesita un gestor y una URL de hoja");
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

    void OnApplicationQuit() => QueueUpload();

    // Se invoca cada vez que cambia la lista en memoria
    void OnListsChanged() => QueueUpload();

    /// <summary>Descarga manualmente la hoja.</summary>
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

    /// <summary>Descarga la hoja CSV y reconstruye las listas.</summary>
    IEnumerator Load()
    {
        UnityWebRequest request = UnityWebRequest.Get(sheetUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error leyendo hoja: {request.error}");
            yield break;
        }

        // Paso 3: dividir el CSV en líneas
        var lines = request.downloadHandler.text.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
            yield break;

        // Paso 4: leer cabeceras para localizar columnas
        var headers = lines[0].Split(',');
        for (int i = 0; i < headers.Length; i++)
            headers[i] = StripQuotes(headers[i]);

        int listCol = System.Array.IndexOf(headers, LIST);
        int itemCol = System.Array.IndexOf(headers, ITEM);
        int qtyCol = System.Array.IndexOf(headers, QTY);
        int posCol = System.Array.IndexOf(headers, POS);
        int completedCol = System.Array.IndexOf(headers, DONE);
        int idCol = System.Array.IndexOf(headers, ID);

        // Paso 5: reconstruir listas sin disparar eventos
        manager.BeginUpdate();
        manager.Clear();

        int sheetRow = 1; // fila de cabecera
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            sheetRow++;

            var values = line.Split(',');
            string listName = listCol >= 0 && listCol < values.Length ? StripQuotes(values[listCol]) : DEFAULT_LIST;
            string itemName = itemCol >= 0 && itemCol < values.Length ? StripQuotes(values[itemCol]) : string.Empty;
            string qtyStr = qtyCol >= 0 && qtyCol < values.Length ? StripQuotes(values[qtyCol]) : "0";
            string posStr = posCol >= 0 && posCol < values.Length ? StripQuotes(values[posCol]) : "-1";
            string completedStr = completedCol >= 0 && completedCol < values.Length ? StripQuotes(values[completedCol]) : "false";
            string id = idCol >= 0 && idCol < values.Length ? StripQuotes(values[idCol]) : null;

            if (string.IsNullOrEmpty(itemName))
                continue;

            int qty = 0; int.TryParse(qtyStr, out qty);
            int pos = -1; int.TryParse(posStr, out pos);
            bool completed = false; bool.TryParse(completedStr, out completed);

            int column = itemCol >= 0 ? itemCol + 1 : -1;
            manager.AddItem(listName, itemName, qty, pos, sheetRow, column, completed, id);
        }

        manager.EndUpdate();

        Debug.Log("Listas cargadas desde Google Sheets");
    }

    // ---- Código de subida ----

    void QueueUpload()
    {
        if (manager == null || string.IsNullOrEmpty(scriptUrl))
            return;

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

            // Pequeña pausa para evitar saturar el servidor
            yield return new WaitForSeconds(1f);
        }
    }

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
    }

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

        var wrapper = new Wrapper { lists = serializableLists };
        string json = JsonUtility.ToJson(wrapper);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(scriptUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError($"Error subiendo lista: {request.error}");
        else
            Debug.Log("Hoja actualizada");
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

    static string StripQuotes(string value) => value == null ? null : value.Trim().Trim('"');
}
