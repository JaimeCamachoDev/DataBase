<header>

![Banner](https://github.com/user-attachments/assets/5b933a56-0ece-452a-99c0-1a641485a6b9)

# **DataBase**

_**Repositorio destinado a albergar los datos para acceder desde los poryectos en Unity**_


</header>
## 📊 Acceso a datos desde Google Sheets

Este repositorio incluye un script de ejemplo (`GoogleSheetsReader`) para leer una hoja de cálculo pública de Google desde Unity. De esta forma puedes modificar el contenido de la hoja y reflejar los cambios en el juego sin recompilar.

1. En tu hoja de cálculo selecciona **Archivo → Publicar en la Web** y copia el enlace de exportación en formato CSV.
2. Coloca ese enlace en el campo **sheetUrl** del componente `GoogleSheetsReader`.
3. Asegúrate de tener activado el paquete integrado **Unity Web Request** en *Package Manager* o añade la dependencia `com.unity.modules.unitywebrequest` al `manifest.json`.
4. Ejecuta la escena y el script descargará el contenido, mostrándolo en la consola. Puedes adaptarlo para actualizar cualquier variable de tu juego.

   
<footer>
   
## Después de crear el repositorio desde la plantilla, asegúrate de revisar lo siguiente:

### 📸 Social Preview
- [ ] Sube una imagen `preview.png` personalizada en `Settings → Social Preview`.


### 🎨 Personalización visual
- [ ] Cambiar imagen del banner de portada.
- [ ] Dejar Topics necesarios.


</footer>

