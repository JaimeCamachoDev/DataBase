<header>

![Banner](https://github.com/user-attachments/assets/5b933a56-0ece-452a-99c0-1a641485a6b9)
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

- **ShoppingItem**: almacena el nombre del artículo y la cantidad.
- **ShoppingList**: agrupa varios `ShoppingItem` bajo un nombre de lista.
- **ShoppingListManager**: permite crear listas y añadir elementos desde código.
- **GoogleSheetsShoppingListLoader**: lee una hoja de cálculo publicada en formato CSV e incorpora los datos al `ShoppingListManager`.


El componente permite indicar en el inspector los encabezados de columna que corresponden al nombre de la lista, el artículo y la cantidad. De este modo puedes usar títulos personalizados en la primera fila de tu hoja de cálculo. Si no incluyes la columna de lista, todos los elementos se añadirán a la lista indicada en `defaultListName`.
Estos componentes sirven como base para desarrollar la funcionalidad de la aplicación sin necesidad de recompilar cada vez que cambien los datos.

### Gestión de la lista desde la interfaz

Se han añadido dos scripts para manipular las listas en tiempo de ejecución:

- **ShoppingListUI**: muestra el contenido del `ShoppingListManager` y permite añadir o eliminar elementos mediante campos de entrada y botones.
- **GoogleSheetsShoppingListWriter**: envía los cambios a un script web para actualizar la hoja de cálculo mediante una petición `POST` en formato JSON.

Vincula estos componentes a tu panel de UI para editar las listas desde Unity y mantener la hoja de cálculo sincronizada.

   
<footer>
   
## Después de crear el repositorio desde la plantilla, asegúrate de revisar lo siguiente:

### 📸 Social Preview
- [ ] Sube una imagen `preview.png` personalizada en `Settings → Social Preview`.


### 🎨 Personalización visual
- [ ] Cambiar imagen del banner de portada.
- [ ] Dejar Topics necesarios.


</footer>

