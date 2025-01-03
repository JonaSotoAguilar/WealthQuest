# Proyecto WealthQuest

Este proyecto utiliza Firebase como respaldo para autenticación, base de datos y almacenamiento. Para que el proyecto funcione correctamente, es necesario instalar el SDK de Firebase y añadir los paquetes correspondientes al proyecto Unity.

## Requisitos previos

1. **Unity**: Instala Unity Hub y asegúrate de tener una versión de Unity compatible con Firebase.
2. **Firebase Console**: Crea un proyecto en [Firebase Console](https://console.firebase.google.com/).
3. **Configuración del proyecto Firebase**: Descarga y configura el archivo `google-services.json` (Android) y/o `GoogleService-Info.plist` (iOS).

---

## Instalación

### 1. Descargar el SDK de Firebase

1. Accede al [sitio oficial de Firebase para Unity](https://firebase.google.com/docs/unity/setup?hl=es-419).
2. Descarga el **Firebase SDK for Unity**.
3. Extrae el archivo descargado y localiza los paquetes necesarios para el proyecto:
   - `FirebaseAuth.unitypackage` (Autenticación)
   - `FirebaseFirestore.unitypackage` (Base de datos Firestore)

### 2. Importar los paquetes a Unity

1. Abre tu proyecto en Unity.
2. Ve al menú `Assets > Import Package > Custom Package...`.
3. Selecciona y abre `FirebaseAuth.unitypackage`.
4. Repite el proceso con `FirebaseFirestore.unitypackage`.
5. Asegúrate de que todas las dependencias necesarias están marcadas y haz clic en `Import`.

### 3. Configurar Firebase en Unity

1. Agrega el archivo de configuración descargado desde Firebase Console a tu proyecto Unity:
   - Para Android: Coloca el archivo `google-services.json` en la carpeta `Assets`.
   - Para iOS: Coloca el archivo `GoogleService-Info.plist` en la carpeta `Assets`.

2. Abre el menú `Firebase > Open Config Window` y sigue las instrucciones para configurar Firebase en tu proyecto.

### 4. Configurar las reglas de Firestore

En la Firebase Console, configura las reglas de Firestore para que coincidan con los requisitos de seguridad de tu proyecto:

```firestore
rules_version = '2';

service cloud.firestore {
  match /databases/{database}/documents {
    match /users/{userId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
    match /users/{userId}/gameHistory/{gameId} {
      allow read, write: if request.auth != null && request.auth.uid == userId;
    }
  }
}
```

---

## Verificación

1. Ejecuta el proyecto en Unity.
2. Verifica que puedes iniciar sesión, guardar y recuperar datos de Firestore.

---

## Soporte
Si tienes algún problema durante la configuración o ejecución del proyecto, consulta la [documentación de Firebase para Unity](https://firebase.google.com/docs/unity/setup?hl=es-419) o abre un issue en este repositorio.
