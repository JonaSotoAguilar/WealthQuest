using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror.Examples.Basic;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    // Firebase variable
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore firestore;
    private Coroutine syncProfileCoroutine;

    // Login Variables
    [Space]
    [Header("Login")]
    [SerializeField] private TMP_InputField emailLoginField;
    [SerializeField] private TMP_InputField passwordLoginField;

    // Registration Variables
    [Space]
    [Header("Registration")]
    [SerializeField] private TMP_InputField nameRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField confirmPasswordRegisterField;

    [Space]
    [Header("Content")]
    [SerializeField] private Content content;

    #region Initialization

    private void Awake()
    {
        CreateInstance();
        content.InitializateLocalContent();
        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private void CreateInstance()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyStatusTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => dependencyStatusTask.IsCompleted);

        dependencyStatus = dependencyStatusTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoLogin());
        }
        else
        {
            Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private IEnumerator CheckForAutoLogin()
    {
        //Logout();
        if (user != null)
        {
            var reloadUser = user.ReloadAsync();

            yield return new WaitUntil(() => reloadUser.IsCompleted);

            AutoLogin();
        }
        else
        {
            LoginManager.Instance.OpenLoginPanel();
        }
    }

    private void AutoLogin()
    {
        //FIXME: Revisar si se podra crear perfil solo en local
        if (user != null)
        {
            ProfileUser.LoadProfile(user.UserId);
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
        else
        {
            LoginManager.Instance.OpenLoginPanel();
        }
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    #endregion

    #region Login

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        LoginManager.Instance.loginButton.interactable = false;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Inicio de sesión fallido! ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Correo inválido";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Contraseña incorrecta";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Correo faltante";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Contraseña faltante";
                    break;
                default:
                    failedMessage = "Inicio de sesión fallido.";
                    break;
            }

            LoginManager.Instance.warningLoginText.text = failedMessage;
            LoginManager.Instance.loginButton.interactable = true;
        }
        else
        {
            user = loginTask.Result.User;
            LoginManager.Instance.warningLoginText.text = "";

            StartCoroutine(LoadProfile(user.UserId, async (success, profileServer) =>
            {
                if (success)
                {
                    await ProfileUser.LoadFirebaseProfile(user.UserId, user.DisplayName, profileServer);
                    ProfileUser.LoadProfile(user.UserId);
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
                }
                else
                {
                    LoginManager.Instance.loginButton.interactable = true;
                    LoginManager.Instance.warningLoginText.text = "No se pudo cargar el perfil del usuario.";
                }
            }));
        }
    }

    #endregion

    #region Registration

    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        LoginManager.Instance.registerButton.interactable = false;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Regitro fallido! ";

            switch (authError)
            {
                case AuthError.EmailAlreadyInUse:
                    failedMessage += "Correo ya en uso.";
                    break;
                case AuthError.InvalidEmail:
                    failedMessage += "Correo inválido.";
                    break;
                case AuthError.WeakPassword:
                    failedMessage += "Contraseña débil.";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Correo faltante.";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Contraseña faltante.";
                    break;
                default:
                    failedMessage += "Error desconocido.";
                    break;
            }

            LoginManager.Instance.registerButton.interactable = true;
            LoginManager.Instance.warningRegisterText.text = failedMessage;
        }
        else
        {
            // Get The User After Registration Success
            user = registerTask.Result.User;

            UserProfile userProfile = new UserProfile { DisplayName = name };

            var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

            yield return new WaitUntil(() => updateProfileTask.IsCompleted);

            if (updateProfileTask.Exception != null)
            {
                Debug.LogError("Failed to update user profile: " + updateProfileTask.Exception);
                user.DeleteAsync();
                LoginManager.Instance.registerButton.interactable = true;
                LoginManager.Instance.warningRegisterText.text = "No se pudo actualizar el perfil del usuario. Inténtalo nuevamente.";
            }
            else
            {
                CreateProfile(user.UserId);

                LoginManager.Instance.warningRegisterText.text = "";
                LoginManager.Instance.OpenLoginPanel();
            }
        }
    }

    #endregion

    #region Logout

    private void Logout()
    {
        auth.SignOut();
        user = null;
        Debug.Log("User Signed Out");
    }

    #endregion

    #region Database

    private void CreateProfile(string userId)
    {
        ProfileData profileData = new();

        firestore.Collection("users").Document(userId).SetAsync(profileData).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Profile created successfully for " + name);
            }
            else
            {
                Debug.LogError("Failed to create profile: " + task.Exception);
            }
        });
    }

    private IEnumerator LoadProfile(string userId, Action<bool, ProfileData> callback = null)
    {
        var docRef = firestore.Collection("users").Document(userId);

        var task = docRef.GetSnapshotAsync();

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("Error al cargar perfil remoto: " + task.Exception);
            callback?.Invoke(false, null);
        }
        else
        {
            var snapshot = task.Result;

            if (snapshot.Exists)
            {
                ProfileData profileServer = snapshot.ConvertTo<ProfileData>();
                callback?.Invoke(true, profileServer);
            }
            else
            {
                Debug.LogError("No se encontró perfil remoto para el usuario: " + userId);
                callback?.Invoke(false, null);
            }
        }
    }

    public void UpdateProfile(string userId)
    {
        if (syncProfileCoroutine != null) return;

        syncProfileCoroutine = StartCoroutine(SyncProfile(userId));
    }

    private IEnumerator SyncProfile(string userId)
    {
        // Esperar hasta que haya conexión a internet
        yield return new WaitUntil(() => Application.internetReachability != NetworkReachability.NotReachable);

        yield return LoadProfile(userId, (success, profileServer) =>
        {
            if (success)
            {
                var updates = new Dictionary<string, object>();

                // Comparar y sincronizar datos
                if (ProfileUser.username != user.DisplayName)
                    StartCoroutine(UpdateDisplayName(ProfileUser.username));

                if (ProfileUser.level > profileServer.Level)
                    updates.Add("Level", ProfileUser.level);

                if (ProfileUser.xp > profileServer.Xp)
                    updates.Add("Xp", ProfileUser.xp);

                if (ProfileUser.averageScore > profileServer.AverageScore)
                    updates.Add("AverageScore", ProfileUser.averageScore);

                if (ProfileUser.bestScore > profileServer.BestScore)
                    updates.Add("BestScore", ProfileUser.bestScore);

                if (ProfileUser.playedGames > profileServer.PlayedGames)
                    updates.Add("PlayedGames", ProfileUser.playedGames);

                if (ProfileUser.bGamesProfile != null && ProfileUser.bGamesProfile.id != profileServer.BGamesId)
                    updates.Add("BGamesId", ProfileUser.bGamesProfile.id);

                if (updates.Count == 0) return;

                firestore.Collection("users").Document(userId).UpdateAsync(updates).ContinueWith(task =>
                    {
                        if (task.IsCompleted)
                        {
                            Debug.Log("Perfil sincronizado correctamente.");
                        }
                        else
                        {
                            Debug.LogError("Error al sincronizar perfil: " + task.Exception);
                        }
                    });
            }
            else
            {
                Debug.LogError("No se pudo sincronizar el perfil remoto.");
            }
        });
        syncProfileCoroutine = null;
    }

    private IEnumerator UpdateDisplayName(string newDisplayName)
    {
        if (auth.CurrentUser != null)
        {
            // Crear el objeto UserProfile con el nuevo DisplayName
            UserProfile profile = new UserProfile
            {
                DisplayName = newDisplayName
            };

            // Actualizar el perfil del usuario
            var updateTask = auth.CurrentUser.UpdateUserProfileAsync(profile);

            yield return new WaitUntil(() => updateTask.IsCompleted);

            if (updateTask.Exception != null)
            {
                Debug.LogError("Failed to update display name: " + updateTask.Exception);
            }
            else
            {
                Debug.Log("Display name updated successfully to: " + newDisplayName);
            }
        }
        else
        {
            Debug.LogError("No user is currently logged in.");
        }
    }

    public async Task SaveGameHistory(string userId, FinishGameData gameData)
    {
        // Esperar hasta que haya conexión a internet
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("Esperando conexión a internet...");
            await Task.Delay(1000);
        }

        var docRef = firestore.Collection("users").Document(userId).Collection("gameHistory").Document($"game_{gameData.gameID}");

        var data = new Dictionary<string, object>
        {
            { "GameID", gameData.GameID },
            { "Years", gameData.Years },
            { "TimePlayed", gameData.TimePlayed },
            { "Date", gameData.Date },
            { "Content", gameData.Content },
            { "Score", gameData.Score }
        };

        try
        {
            await docRef.SetAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving game history: {e.Message}");
        }
    }

    public async Task<List<FinishGameData>> LoadGameHistory(string userId)
    {
        // Esperar hasta que haya conexión a internet
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("Esperando conexión a internet...");
            await Task.Delay(1000); // Esperar 1 segundo antes de volver a comprobar
        }

        List<FinishGameData> gameHistory = new List<FinishGameData>();

        try
        {
            // Referencia a la subcolección "gameHistory" del usuario
            CollectionReference gameHistoryRef = firestore.Collection("users").Document(userId).Collection("gameHistory");

            // Obtener todos los documentos de la subcolección
            QuerySnapshot snapshot = await gameHistoryRef.GetSnapshotAsync();

            // Iterar por cada documento y convertirlo a FinishGameData
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    FinishGameData gameData = doc.ConvertTo<FinishGameData>();
                    gameHistory.Add(gameData);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading game history: {e.Message}");
        }

        return gameHistory;
    }

    //FIXME: Comparar historial de juegos local y remoto
    public async Task UpdateHistory(string userId, List<FinishGameData> historyFirebase)
    {
        // Obtener el historial local
        List<FinishGameData> localHistory = ProfileUser.history;
        int lastGameLocal = localHistory.Count;
        int lastGameFirebase = historyFirebase.Count;
        Debug.Log($"Local: {lastGameLocal}, Firebase: {lastGameFirebase}");

        // Si los tamaños son iguales, no hay nada que actualizar
        if (lastGameLocal == lastGameFirebase) return;

        if (lastGameFirebase > lastGameLocal)
        {
            // Historial remoto tiene más juegos, sincroniza al local
            int newGamesCount = lastGameFirebase - lastGameLocal;
            var newGames = historyFirebase.GetRange(lastGameLocal, newGamesCount);
            Debug.Log($"New games: {newGames.Count}");

            foreach (var game in newGames)
            {
                Debug.Log($"Game {game.gameID}: {game.date}");
                ProfileUser.history.Add(game);
                await SaveSystem.SaveHistory(game, 0); // Guardar en local
            }
            PlayerPrefs.SetInt("gameId", lastGameFirebase);
            Debug.Log("GameId updated:" + lastGameFirebase);
        }
        else if (lastGameLocal > lastGameFirebase)
        {
            // Historial local tiene más juegos, sincroniza al remoto
            int newGamesCount = lastGameLocal - lastGameFirebase;
            var newGames = localHistory.GetRange(lastGameFirebase, newGamesCount);
            Debug.Log($"New games: {newGames.Count}");

            foreach (var game in newGames)
            {
                await SaveGameHistory(userId, game); // Guardar en Firebase
            }
            PlayerPrefs.SetInt("gameId", lastGameLocal);
            Debug.Log("GameId updated:" + lastGameLocal);
        }
    }

    #endregion

}