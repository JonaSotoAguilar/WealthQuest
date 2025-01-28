using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Firebase.Extensions;

public class FirebaseService : MonoBehaviour
{
    public static FirebaseService Instance;

    // Firebase variable
    [Header("Firebase")]
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore firestore;
    private Coroutine syncProfileCoroutine;
    [HideInInspector] public DependencyStatus dependencyStatus;

    public bool logged = false;

    #region Initialization

    private void Awake()
    {
        CreateInstance();
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
        //Logout(); // Uncomment to test login
        if (user != null)
        {
            var reloadUser = user.ReloadAsync();

            yield return new WaitUntil(() => reloadUser.IsCompleted);

            AutoLogin();
        }
        else
        {
            MenuManager.Instance.OpenLoginMenu();
        }
    }

    private void AutoLogin()
    {
        if (user != null)
        {
            logged = true;
            ProfileUser.LoadProfile(user.UserId);
            MenuManager.Instance.OpenGameMenu();
        }
        else
        {
            MenuManager.Instance.OpenLoginMenu();
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
        StartCoroutine(LoginAsync());
    }

    private IEnumerator LoginAsync()
    {
        LoginManager.Instance.loginButton.interactable = false;
        string email = LoginManager.Instance.emailLoginField.text;
        string password = LoginManager.Instance.passwordLoginField.text;
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

            StartCoroutine(LoadProfile(user.UserId, (success, profileServer) =>
            {
                if (success)
                {
                    logged = true;
                    ProfileUser.LoadFirebaseProfile(user.UserId, user.DisplayName, profileServer);
                    ProfileUser.LoadProfile(user.UserId);
                    MenuManager.Instance.OpenGameMenu();
                }
                else
                {
                    LoginManager.Instance.loginButton.interactable = true;
                    LoginManager.Instance.warningLoginText.text = "No se pudo cargar el perfil del usuario.";
                }
            }));
        }
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

    #endregion

    #region Registration

    public void Register()
    {
        StartCoroutine(RegisterAsync());
    }

    private IEnumerator RegisterAsync()
    {
        LoginManager.Instance.registerButton.interactable = false;
        string name = LoginManager.Instance.nameRegisterField.text;
        string email = LoginManager.Instance.emailRegisterField.text;
        string password = LoginManager.Instance.passwordRegisterField.text;

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
                string birthDate = LoginManager.Instance.birthDatePicker.text;
                string gender = LoginManager.Instance.genderDropdown.options[LoginManager.Instance.genderDropdown.value].text;
                CreateProfile(user.UserId, birthDate, gender);

                LoginManager.Instance.warningRegisterText.text = "";
                MenuManager.Instance.OpenLoginMenu();
            }
        }
    }

    private void CreateProfile(string userId, string birthDate, string gender)
    {
        int age = CalculateAge(birthDate);
        //int role = LoginManager.Instance.roleDropdown.value;
        ProfileData profileData = new ProfileData(birthDate, gender, age);

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

    private int CalculateAge(string birthDate)
    {
        DateTime birth = DateTime.Parse(birthDate);
        DateTime now = DateTime.Now;
        int age = now.Year - birth.Year;
        if (now.Month < birth.Month || (now.Month == birth.Month && now.Day < birth.Day))
            age--;
        return age;
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

    #region Sync Profile

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

                if (ProfileUser.role != profileServer.Role)
                    updates.Add("Role", ProfileUser.role);

                if (ProfileUser.level > profileServer.Level)
                    updates.Add("Level", ProfileUser.level);
                else if (ProfileUser.level < profileServer.Level)
                    ProfileUser.UpdateLevel(profileServer.Level);

                if (ProfileUser.xp > profileServer.Xp)
                    updates.Add("Xp", ProfileUser.xp);
                else if (ProfileUser.xp < profileServer.Xp)
                    ProfileUser.UpdateXp(profileServer.Xp);

                if (ProfileUser.averageScore > profileServer.AverageScore)
                    updates.Add("AverageScore", ProfileUser.averageScore);
                else if (ProfileUser.averageScore < profileServer.AverageScore)
                    ProfileUser.UpdateAverageScoreUser(profileServer.AverageScore);

                if (ProfileUser.bestScore > profileServer.BestScore)
                    updates.Add("BestScore", ProfileUser.bestScore);
                else if (ProfileUser.bestScore < profileServer.BestScore)
                    ProfileUser.UpdateBestScoreUser(profileServer.BestScore);

                if (ProfileUser.playedGames > profileServer.PlayedGames)
                    updates.Add("PlayedGames", ProfileUser.playedGames);
                else if (ProfileUser.playedGames < profileServer.PlayedGames)
                    ProfileUser.UpdatePlayedGames(profileServer.PlayedGames);

                if (ProfileUser.financeLevel > profileServer.FinanceLevel)
                    updates.Add("FinanceLevel", ProfileUser.financeLevel);
                else if (ProfileUser.financeLevel < profileServer.FinanceLevel)
                    ProfileUser.UpdateFinanceLevel(profileServer.FinanceLevel);

                int currentAge = CalculateAge(profileServer.BirthDate);
                if (currentAge > profileServer.Age)
                    updates.Add("Age", currentAge);

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

    public async Task UpdateHistory(string userId, List<FinishGameData> historyFirebase)
    {
        // Obtener el historial local
        List<FinishGameData> localHistory = ProfileUser.history;
        int lastGameLocal = localHistory.Count;
        int lastGameFirebase = historyFirebase.Count;

        // Si los tamaños son iguales, no hay nada que actualizar
        if (lastGameLocal == lastGameFirebase) return;

        if (lastGameFirebase > lastGameLocal)
        {
            // Historial remoto tiene más juegos, sincroniza al local
            int newGamesCount = lastGameFirebase - lastGameLocal;
            var newGames = historyFirebase.GetRange(lastGameLocal, newGamesCount);

            foreach (var game in newGames)
            {
                ProfileUser.history.Add(game);
                await SaveService.SaveHistory(game, 0); // Guardar en local
            }
            PlayerPrefs.SetInt("gameId", lastGameFirebase);
        }
        else if (lastGameLocal > lastGameFirebase)
        {
            // Historial local tiene más juegos, sincroniza al remoto
            int newGamesCount = lastGameLocal - lastGameFirebase;
            var newGames = localHistory.GetRange(lastGameFirebase, newGamesCount);

            foreach (var game in newGames)
            {
                await SaveGameHistory(userId, game);
            }
            PlayerPrefs.SetInt("gameId", lastGameLocal);
        }
    }

    #endregion

    #region Forget Password

    public void forgetPasswordSubmit()
    {
        StartCoroutine(forgetPasswordSubmitAsync());
    }

    public IEnumerator forgetPasswordSubmitAsync()
    {
        string email = LoginManager.Instance.emailForgotField.text;

        var task = auth.SendPasswordResetEmailAsync(email);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
        {
            Debug.LogError("Error al enviar correo: " + task.Exception);
            LoginManager.Instance.warningForgotText.text = "Error al enviar correo.";
        }
        else
        {
            Debug.Log("Correo enviado correctamente");
            MenuManager.Instance.OpenLoginMenu();
        }
    }

    #endregion

    #region Sync Content

    public void UploadContent(Content content)
    {
        // Convertir Content a ContentData
        ContentData contentData = new ContentData(content);

        // Referencia al documento principal (Content)
        DocumentReference contentDocRef = FirebaseFirestore.DefaultInstance
            .Collection("contents")
            .Document(content.uid);

        // Subir el documento ContentData, incluyendo las preguntas
        contentDocRef.SetAsync(contentData).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log($"Content with ID '{content.uid}' uploaded successfully.");
            }
            else
            {
                Debug.LogError($"Error uploading content: {task.Exception}");
            }
        });
    }

    // Descargar contenido y preguntas


    // Obtener todos los contenidos




    // Actualizar el contenido y las preguntas

    // Sumar una descarga al contenido

    #endregion

    #region Diagnostic Test

    // Obtener un test aleatorio de Firestore
    public async Task<TestData> GetRandomTestAsync()
    {
        List<TestData> testList = new List<TestData>();

        try
        {
            CollectionReference testCollection = firestore.Collection("diagnosticTest");
            QuerySnapshot snapshot = await testCollection.GetSnapshotAsync();

            Debug.Log($"Se han encontrado {snapshot.Count} test(s) en Firestore.");
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                Debug.Log($"Document ID: {document.Id}");
                string documentId = document.Id;
                TestData test = document.ConvertTo<TestData>();
                Debug.Log($"Test: {test.Questions.Count} questions");
                testList.Add(test);
            }

            Debug.Log($"Se han obtenido {testList.Count} test(s) de Firestore.");

            if (testList.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, testList.Count);
                return testList[randomIndex];
            }
            else
            {
                Debug.LogWarning("No se encontraron tests en Firestore.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener los tests: {ex.Message}");
        }

        return null; // Retorna null si no hay tests o ocurre un error
    }

    // Subir resultados del test
    public void SaveTestResults(TestResultsData testResults)
    {
        string userId = auth.CurrentUser.UserId;

        // Crear un nuevo documento con un ID aleatorio en Firestore
        DocumentReference testResultsRef = firestore
            .Collection("users")
            .Document(userId)
            .Collection("testResults")
            .Document();

        // Subir el documento a Firestore
        testResultsRef.SetAsync(testResults).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error uploading test results: " + task.Exception?.Flatten().Message);
            }
            else if (task.IsCanceled)
            {
                Debug.LogWarning("Test results upload was canceled.");
            }
            else
            {
                Debug.Log("Test results uploaded successfully.");
            }
        });
    }


    #endregion

}