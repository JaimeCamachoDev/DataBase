<header>

<img width="2416" height="2042" alt="Captura de pantalla 2023-09-22 a las 11 58 53" src="https://github.com/user-attachments/assets/c4be976a-a943-4879-bbd9-dccc1f5ae655" />
<img width="3792" height="1312" alt="Captura de pantalla 2023-09-22 a las 11 59 32" src="https://github.com/user-attachments/assets/6dd315df-4d73-4f74-92cf-5854f105f358" />

# **DataBase**

_**Repositorio destinado a albergar los datos para acceder desde los poryectos en Unity**_

</header>
## 📊 Acceso a datos desde Google Sheets

Este repositorio incluye un script de ejemplo (`GoogleSheetsReader`) para leer una hoja de cálculo pública de Google desde Unity. De esta forma puedes modificar el contenido de la hoja y reflejar los cambios en el juego sin recompilar.

1. En tu hoja de cálculo selecciona **Archivo → Publicar en la Web** y copia el enlace de exportación en formato CSV.
2. Coloca ese enlace en el campo **sheetUrl** del componente `GoogleSheetsReader`.
3. Asegúrate de tener activado el paquete integrado **Unity Web Request** en *Package Manager* o añade la dependencia `com.unity.modules.unitywebrequest` al `manifest.json`.
4. Ejecuta la escena y el script descargará el contenido, mostrándolo en la consola. El script interpreta la primera fila como los encabezados de columna y mostrará cada registro usando esos títulos. Puedes adaptarlo para actualizar cualquier variable de tu juego.

## 🛒 Estructura base para una app de listas de la compra

Se ha añadido un conjunto de scripts en `Assets/1-Scripts/ShoppingList` que proporcionan la estructura mínima para una aplicación de listas:

- **ShoppingItem**: almacena el nombre del artículo, la cantidad, la lista a la que pertenece, la posición dentro de la lista, la fecha de última modificación y su ubicación (fila y columna) en la hoja de cálculo.
- **ShoppingList**: agrupa varios `ShoppingItem` bajo un nombre de lista.
- **ShoppingListManager**: permite crear listas y añadir elementos desde código.
- **GoogleSheetsShoppingListLoader**: lee una hoja de cálculo publicada en formato CSV e incorpora los datos al `ShoppingListManager`, además de enviar los cambios a un script de Apps Script para mantener la hoja sincronizada.

Estos componentes sirven como base para desarrollar la funcionalidad de la aplicación sin necesidad de recompilar cada vez que cambien los datos.

### Gestión de la lista desde la interfaz

Se han añadido scripts para manipular y visualizar las listas en tiempo de ejecución:

- **ShoppingListUI**: instancia un prefab por cada artículo del `ShoppingListManager` dentro de un contenedor y permite añadir o eliminar elementos mediante campos de entrada y botones.
- **ShoppingListItemUI**: usa TextMeshPro para mostrar los datos del item y, gracias al evento `onDelete` de `SwipeToDeleteItem`, al eliminarse también actualiza el gestor y la hoja de cálculo. Los elementos completados se muestran tachados y en gris para dar feedback visual. Si no se le asignan referencias al gestor, las buscará en la escena al iniciarse.
- **Rebuild automático**: `ShoppingListUI` ahora detecta el componente `ShoppingListItemUI` aunque se encuentre en un objeto hijo del prefab instanciado.

> **⚠️ IMPORTANTE:** cada vez que edites el script de Google Apps debes crear un **nuevo deployment** y **actualizar la URL** en Unity para que los cambios surtan efecto. Si olvidas este paso, la aplicación seguirá usando la versión anterior del script.

Vincula estos componentes a tu panel de UI, asigna el prefab de item y tendrás la interfaz sincronizada con la hoja de cálculo de Google.

### Sincronización en tiempo real con Apps Script + servidor WebSocket

Para reducir la latencia entre dispositivos se añade un pequeño servidor (`realtime-server.js`) que recibe las notificaciones del script de Apps Script y las retransmite a través de WebSockets.

1. Ejecuta `npm install` y luego `node realtime-server.js` para ponerlo en marcha.
2. Añade una columna **Updated** en la hoja de cálculo (ISO 8601) para llevar control de versiones.
3. Copia la URL pública de `http://<host>:3000/sync` en la constante `MANAGER_URL` de `AppsScript.gs` y vuelve a desplegar el script.
4. En Unity asigna el campo **Manager WebSocket Url** del `GoogleSheetsShoppingListLoader` con `ws://<host>:3000`.
5. Cada edición de la hoja enviará los datos al servidor, que los emitirá a todos los clientes conectados; éstos recargarán la lista al instante sin necesidad de reiniciar la app.

Este flujo evita el polling constante y mantiene sincronizadas todas las instancias con una latencia mínima.
