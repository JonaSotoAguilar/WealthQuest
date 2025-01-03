# WealthQuest

WealthQuest es un juego tipo tablero diseñado para promover la educación financiera de manera entretenida. Ofrece una experiencia interactiva y educativa, combinando elementos de estrategia y administración de recursos.

---

## Características

1. **Educación Financiera**: Aprende conceptos básicos y avanzados de finanzas mientras juegas.
2. **Integración con bGames** (opcional): WealthQuest puede interactuar con el framework de bGames para utilizar puntos y ofrecer beneficios adicionales.
3. **Captura de datos con Fintual Sensor** (opcional): Este proyecto incluye el sensor de Fintual para la captura y análisis de datos financieros, disponible en el siguiente repositorio: [bGames-FintualSensor](https://github.com/JonaSotoAguilar/bGames-FintualSensor.git).

---

## Requisitos

1. **Unity**: Versión compatible con el proyecto (recomendado: Unity 6000.0.17f1 o superior).
2. **Firebase**:
   - **Autenticación**
   - **Firestore** (para almacenamiento de datos)
3. **Base de datos bGames** (obligatorio si se utiliza bGames):
   - Importar los datos específicos del juego WealthQuest a MySQL usando el script `bGames_WealthQuest_backup.sql` disponible en el repositorio del sensor de Fintual.

---

## Instalación

### 1. Configuración de Firebase

1. Crea un proyecto en [Firebase Console](https://console.firebase.google.com/).
2. Descarga los archivos de configuración:
   - `google-services.json` (Android).
   - `GoogleService-Info.plist` (iOS).
3. Configura Firebase en Unity siguiendo los pasos detallados en su [documentación oficial](https://firebase.google.com/docs/unity/setup?hl=es-419).

### 2. Configuración del Sensor de Fintual (Opcional)

1. Clona el repositorio [bGames-FintualSensor](https://github.com/JonaSotoAguilar/bGames-FintualSensor.git).
2. Sigue las instrucciones del repositorio para integrarlo en tu proyecto Unity.

### 3. Configuración de la Base de Datos bGames (Obligatorio si se utiliza bGames)

1. Asegúrate de tener un servidor MySQL configurado.
2. Importa el script `bGames_WealthQuest_backup.sql` desde el repositorio del sensor de Fintual a la base de datos de bGames.
3. Verifica que los datos específicos de WealthQuest estén correctamente poblados en la base de datos.

---

## Funcionalidades Adicionales

### Consumo de Puntos de bGames

Para que WealthQuest pueda consumir puntos de bGames:
1. Asegurate de tener los microservicios de bGames ejecutados.
2. Asegúrate de que los datos de WealthQuest estén sincronizados con el sistema bGames mediante el script mencionado.

### Captura de Datos Financieros (Opcional)

El sensor de Fintual proporciona herramientas avanzadas para capturar y analizar datos financieros directamente en el juego. Revisa su documentación para más detalles.

---

## Soporte

Si tienes preguntas o necesitas ayuda, consulta los siguientes recursos:

- [Documentación de Firebase para Unity](https://firebase.google.com/docs/unity/setup?hl=es-419)
- [Repositorio del Sensor de Fintual](https://github.com/JonaSotoAguilar/bGames-FintualSensor.git)
