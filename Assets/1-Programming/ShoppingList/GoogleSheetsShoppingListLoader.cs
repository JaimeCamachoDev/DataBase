using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking; // requiere el paquete integrado "Unity Web Request"

public class GoogleSheetsShoppingListLoader : MonoBehaviour
{
    [Tooltip("Gestor que almacena las listas cargadas")]
    public ShoppingListManager manager;

    [Tooltip("Enlace público de exportación de Google Sheets")]
    public string sheetUrl;

    [Tooltip("URL del script de Apps Script para subir los cambios")]
    public string scriptUrl;

    [Tooltip("URL del servidor WebSocket que emite cambios en tiempo real")]
    public string managerWebSocketUrl;

    [Tooltip("Recargar datos cada N segundos (0 desactiva la recarga automática)")]
    public float refreshInterval = 0f;

    // Constantes con los nombres de las columnas esperadas en la hoja
    const string LIST_HEADER = "List";
    const string ITEM_HEADER = "Item";
    const string QUANTITY_HEADER = "Units";
    const string POSITION_HEADER = "Position";
    const string COMPLETED_HEADER = "Completed";
    const string ID_HEADER = "Id";
    const string UPDATED_HEADER = "Updated";
    const string DEFAULT_LIST_NAME = "List";

    // Variables de control para la subida de datos
    bool uploadInProgress;
    bool pendingUpload;

    ClientWebSocket webSocket;
    bool refreshRequested;

    void Start()
    {
        // Permite que la aplicación siga ejecutándose en segundo plano
        Application.runInBackground = true;

        // Si no se asigna un manager, intentamos localizar uno en la escena
        if (manager == null)
            manager = FindAnyObjectByType<ShoppingListManager>();

        if (manager != null)
        {
            // Cuando cambien las listas, programamos una subida
            manager.ListsChanged += OnListsChanged;

            // Si hay una URL válida, cargamos los datos iniciales
            if (!string.IsNullOrEmpty(sheetUrl))
            {
                Refresh();
                if (refreshInterval > 0f)
                    StartCoroutine(RefreshPeriodically());
            }

            // Conectamos al servidor de sincronización para recibir notificaciones
            if (!string.IsNullOrEmpty(managerWebSocketUrl))
                ConnectWebSocket();
        }
        else
        {
            Debug.LogWarning("El cargador necesita un ShoppingListManager y una URL de hoja");
        }
    }

    void OnDestroy()
    {
        // Dejamos de escuchar el evento al destruirse
        if (manager != null)
            manager.ListsChanged -= OnListsChanged;

        // Cerramos la conexión WebSocket si existe
        if (webSocket != null)
        {
            try { webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None); } catch {}
            webSocket.Dispose();
        }
    }

    void Update()
    {
        // Si se ha solicitado una recarga desde el hilo del WebSocket
        if (refreshRequested)
        {
            refreshRequested = false;
            Refresh();
        }
    }

    void OnApplicationPause(bool pause)
    {
        // Al pausar la aplicación se intenta subir la información pendiente
        if (pause)
            QueueUpload();
    }

    void OnApplicationQuit()
    {
        // Antes de salir también intentamos subir la información
        QueueUpload();
    }

    void OnListsChanged() => QueueUpload();

    public void Refresh()
    {
        // Inicia la descarga de la hoja de cálculo
        if (manager == null || string.IsNullOrEmpty(sheetUrl)) return;
        StartCoroutine(Load());
    }

    IEnumerator RefreshPeriodically()
    {
        // Recarga la hoja cada cierto tiempo
        while (true)
        {
            yield return new WaitForSeconds(refreshInterval);
            Refresh();
        }
    }

    async void ConnectWebSocket()
    {
        webSocket = new ClientWebSocket();
        try
        {
            await webSocket.ConnectAsync(new System.Uri(managerWebSocketUrl), CancellationToken.None);
            _ = ReceiveLoop();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error de WebSocket: {ex.Message}");
        }
    }

    async Task ReceiveLoop()
    {
        var buffer = new byte[1024];
        while (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                break;
            }

            // Al recibir cualquier mensaje pedimos refrescar la hoja
            refreshRequested = true;
        }
    }

    IEnumerator Load()
    {
        // Realiza la petición HTTP para obtener el CSV
        UnityWebRequest request = UnityWebRequest.Get(sheetUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error al leer la hoja: {request.error}");
            yield break;
        }

        string[] lines = request.downloadHandler.text.Split('\n');
        if (lines.Length == 0)
            yield break;

        // Procesa las cabeceras para localizar las columnas
        string[] headers = lines[0].Trim().Split(',');
        for (int i = 0; i < headers.Length; i++)
            headers[i] = StripQuotes(headers[i]);

        int listCol = System.Array.IndexOf(headers, LIST_HEADER);
        int itemCol = System.Array.IndexOf(headers, ITEM_HEADER);
        int qtyCol = System.Array.IndexOf(headers, QUANTITY_HEADER);
        int posCol = System.Array.IndexOf(headers, POSITION_HEADER);
        int completedCol = System.Array.IndexOf(headers, COMPLETED_HEADER);
        int idCol = System.Array.IndexOf(headers, ID_HEADER);
        int updatedCol = System.Array.IndexOf(headers, UPDATED_HEADER);

        manager.BeginUpdate();
        manager.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            string[] values = line.Split(',');
            string listName = listCol >= 0 && listCol < values.Length ? StripQuotes(values[listCol]) : DEFAULT_LIST_NAME;

            string itemName = itemCol >= 0 && itemCol < values.Length ? StripQuotes(values[itemCol]) : string.Empty;
            string qtyStr = qtyCol >= 0 && qtyCol < values.Length ? StripQuotes(values[qtyCol]) : "0";
            string posStr = posCol >= 0 && posCol < values.Length ? StripQuotes(values[posCol]) : "-1";
            string completedStr = completedCol >= 0 && completedCol < values.Length ? StripQuotes(values[completedCol]) : "false";
            int qty = 0;
            int.TryParse(qtyStr, out qty);
            int pos = -1;
            int.TryParse(posStr, out pos);
            bool completed = false;
            bool.TryParse(completedStr, out completed);

            if (string.IsNullOrEmpty(itemName))
                continue;

            int row = i + 1; // índice de fila empezando en 1 incluyendo la cabecera
            int column = itemCol >= 0 ? itemCol + 1 : -1;
            string id = idCol >= 0 && idCol < values.Length ? StripQuotes(values[idCol]) : null;
            string updated = updatedCol >= 0 && updatedCol < values.Length ? StripQuotes(values[updatedCol]) : null;
            manager.AddItem(listName, itemName, qty, pos, row, column, completed, id, updated);
        }

        manager.EndUpdate();
        Debug.Log("Listas de la compra cargadas desde la hoja");
    }

    void QueueUpload()
    {
        // Marca que hay cambios pendientes de subir
        if (manager == null || string.IsNullOrEmpty(scriptUrl)) return;
        pendingUpload = true;
        if (!uploadInProgress)
            StartCoroutine(UploadSequential());
    }

    IEnumerator UploadSequential()
    {
        // Subida secuencial de cambios acumulados
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
        // Prepara los datos en formato JSON
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
                    completed = item.completed,
                    updated = item.updated
                });
            }
            serializableLists.Add(sList);
        }

        var wrapper = new Wrapper { lists = serializableLists };
        string json = JsonUtility.ToJson(wrapper);

        // Enviamos la petición POST al Apps Script
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        UnityWebRequest request = new UnityWebRequest(scriptUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError($"Error al subir la lista: {request.error}");
        else
            Debug.Log("Hoja actualizada correctamente");
    }

    private static string StripQuotes(string value)
    {
        // Elimina espacios y comillas de un valor de texto
        return value.Trim().Trim('"');
    }

    // Clases auxiliares para serializar la información
    [System.Serializable]
    class Wrapper { public List<SerializableList> lists; }

    [System.Serializable]
    class SerializableList { public string name; public List<SerializableItem> items; }

    [System.Serializable]
    class SerializableItem { public string id; public string name; public int quantity; public int position; public bool completed; public string updated; }
}

