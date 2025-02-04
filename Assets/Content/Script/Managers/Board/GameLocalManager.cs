using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameLocalManager : MonoBehaviour
{
    private static GameLocalManager instance;
    enum GameStatus { None, Playing, Finish }

    [Header("Components")]
    [SerializeField] private CameraManager _camera;
    [SerializeField] private PlayableDirector cinematicDirector;

    [Header("Status")]
    [SerializeField] private GameData gameData;
    private GameStatus status = GameStatus.None;
    private DateTime currentTime;

    [Header("Animations")]
    [SerializeField] private GameObject arrowPrefab;
    private GameObject spawnedArrow;

    // Players
    private List<PlayerLocalManager> playersLocal = new List<PlayerLocalManager>();
    private PlayerLocalManager currPlayer;

    # region Getters

    public static GameData Data { get => instance.gameData; }
    public static List<PlayerLocalManager> Players { get => instance.playersLocal; }
    public static PlayerLocalManager CurrentPlayer { get => instance.currPlayer; }
    public static PlayerLocalManager GetPlayer(string clientID) => instance.playersLocal.Find(player => player.Data.UID == clientID);

    #endregion

    #region Initialization

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void PlayerJoined(PlayerLocalManager player)
    {
        if (instance == null) return;

        instance.playersLocal.Add(player);
    }

    public static void InitializeGame()
    {
        if (instance == null) return;

        instance.UpdateYear(Data.currentYear);
        GameUIManager.ShowPanel(true);

        // StartGame(); // Para pruebas
        if (instance.gameData.timePlayed == "00:00:00" && instance.gameData.playersData.Count > 1)
            instance.StartSelection();
        else
            StartGame();
    }

    private static void StartGame()
    {
        if (instance == null) return;

        // 1. Position players
        instance.InitializePosition();

        // 2. Status game
        instance.status = GameStatus.Playing;
        instance.currPlayer = instance.playersLocal[Data.turnPlayer];

        // 3. Camera
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // 4. Start game
        instance.currentTime = DateTime.Now;
        _ = instance.StartTurn();
    }

    private void InitializePosition()
    {
        List<Square> squares = new List<Square>();
        List<int> positions = new List<int>();
        foreach (var player in playersLocal)
        {
            int pos = player.Data.Position;
            Square currSquare = SquareManager.Squares[pos];
            currSquare.Players.Add(player.Movement);
            positions.Add(pos);
            if (!squares.Contains(currSquare)) squares.Add(currSquare);
        }

        int index = 0;
        foreach (var square in squares)
        {
            square.UpdateCornerPositions(positions[index]);
            index++;
        }
    }

    #endregion

    #region Turn Management

    private async Task StartTurn()
    {
        await GameUIManager.SetPlayerTurn(currPlayer.Data.UID, playersLocal.Count > 1);
        currPlayer.StartTurn();
    }

    public static void FinishTurn()
    {
        if (instance == null) return;

        _ = TaskFinishTurn();
    }

    public static async Task TaskFinishTurn()
    {
        instance.UpdateTime();
        // Esperar a que NextYear termine antes de continuar
        await instance.NextYear();

        // Si el juego ha terminado, detener la ejecución
        if (instance.status == GameStatus.Finish) return;

        instance.NextPlayer();
        instance.SaveGame();
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // Esperar a que StartTurn termine antes de continuar
        await instance.StartTurn();
    }

    private void NextPlayer()
    {
        GameUIManager.ResetPlayerTurn(currPlayer.Data.UID);
        int nexTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        UpdateTurnPlayer(nexTurn);
        currPlayer = playersLocal[nexTurn];
    }

    private async Task NextYear()
    {
        if (instance == null) return;

        int nextTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        if (nextTurn != gameData.initialPlayerIndex) return;

        int newYear = gameData.currentYear + 1;

        if (newYear > gameData.yearsToPlay)
        {
            status = GameStatus.Finish;
            instance.FinishGame();
            return;
        }

        // Procesar finanzas de cada jugador
        foreach (var player in playersLocal)
            player.Data.ProccessFinances();

        // Esperar a que termine la animación de Next Year antes de continuar
        await GameUIManager.ShowNextYear();
        UpdateYear(newYear);
    }

    private void FinishGame()
    {
        // 1. Calculate final score
        SetFinalScore();

        // 2. Save History
        SaveHistory();

        // 3. Announce winner (Animation)
        ShowResults();
    }

    private void SetFinalScore()
    {
        // Calcular puntajes finales para todos los jugadores
        foreach (var player in playersLocal)
        {
            player.Data.SetFinalScore();
        }

        // Ordenar jugadores por puntaje (de mayor a menor)
        List<PlayerLocalManager> sortedPlayers = new List<PlayerLocalManager>(playersLocal);
        sortedPlayers.Sort((a, b) => b.Data.FinalScore.CompareTo(a.Data.FinalScore));

        // Asignar posiciones considerando empates
        int currentRank = 1; // Comenzar desde la posición 1
        int playersAtRank = 0; // Número de jugadores en la posición actual
        int previousScore = sortedPlayers[0].Data.FinalScore; // Puntaje del primer jugador

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var player = sortedPlayers[i];

            if (player.Data.FinalScore < previousScore)
            {
                // Si el puntaje cambia, avanzar en el ranking
                currentRank += playersAtRank;
                playersAtRank = 0; // Reiniciar el contador de jugadores en la posición actual
            }

            // Asignar la posición actual al jugador
            player.Data.SetResultPosition(currentRank);
            playersAtRank++;
            previousScore = player.Data.FinalScore;
        }
    }

    private void ShowResults()
    {
        _ = GameUIManager.ShowFinishGame();
    }

    private void SaveHistory()
    {
        int score = GetPlayer(ProfileUser.uid).Data.FinalScore;
        FinishGameData finishData = new FinishGameData(gameData.currentYear, gameData.timePlayed, gameData.content, score);
        int slotData = gameData.mode == 0 ? 1 : 2;
        ProfileUser.SaveGame(finishData, slotData);
    }

    #endregion

    #region Game Data

    private void SaveGame()
    {
        int slot = gameData.mode == 0 ? 1 : 2;
        StartCoroutine(SaveService.SaveGame(Data, slot));
    }

    private void UpdateTime()
    {
        // Calcular tiempo transcurrido
        DateTime timeNow = DateTime.Now;
        TimeSpan currentSpan = timeNow - currentTime;
        currentTime = timeNow;

        // Actualizar tiempo jugado
        TimeSpan totalSpan = TimeSpan.Parse(gameData.timePlayed);
        TimeSpan timeSpan = totalSpan + currentSpan;
        gameData.timePlayed = timeSpan.ToString(@"hh\:mm\:ss");
    }

    private void UpdateYear(int newYear)
    {
        gameData.currentYear = newYear;
        GameUIManager.ChangeYear(gameData.currentYear);
    }

    private void UpdateTurnPlayer(int newTurn)
    {
        gameData.turnPlayer = newTurn;
    }

    private void UpdateInitialPlayer(int newInitial)
    {
        gameData.initialPlayerIndex = newInitial;
    }

    #endregion

    #region Animation

    private void StartIntroCinematic()
    {
        if (cinematicDirector != null)
        {
            GameUIManager.ShowPanel(false);
            cinematicDirector.Play(); // Reproduce la cinemática

            // Registrar un evento para cuando termine
            cinematicDirector.stopped += OnIntroCinematicEnd;
        }
        else
        {
            StartGame(); // Si no hay cinemática, comienza el juego directamente
        }
    }

    private void OnIntroCinematicEnd(PlayableDirector director)
    {
        if (director == cinematicDirector)
        {
            // Desregistrar el evento
            cinematicDirector.stopped -= OnIntroCinematicEnd;

            // Continuar con el flujo del juego
            GameUIManager.ShowPanel(true);
            StartGame();
        }
    }

    private void StartSelection()
    {
        if (playersLocal.Count == 0)
        {
            Debug.LogError("No hay jugadores configurados.");
            return;
        }
        else if (playersLocal.Count == 1)
        {
            StartIntroCinematic();
        }

        // Spawnear la flecha
        spawnedArrow = Instantiate(arrowPrefab);

        // Mover la flecha entre los Huds
        StartCoroutine(MoveArrow());
    }

    private IEnumerator MoveArrow()
    {
        int hudIndex = 0;
        float delay = 0.2f; // Tiempo entre movimientos

        // Obtener la lista de HUDs desde GameUIManager
        List<Transform> playerHuds = GameUIManager.HUDsList();

        // Acceder a la imagen de la flecha (hija de spawnedArrow)
        GameObject arrowImage = spawnedArrow.transform.GetChild(0).gameObject;

        AudioManager.PlaySoundArrow();
        while (delay > 0.05f)
        {
            // Mover la flecha al siguiente Hud
            Vector3 hudPosition = playerHuds[hudIndex].position;

            // Ajustar la posición de la flecha debajo del Hud (modificando Z o Y según sea necesario)
            arrowImage.transform.position = new Vector3(hudPosition.x, hudPosition.y - 200f, hudPosition.z);

            // Avanzar al siguiente Hud
            hudIndex = (hudIndex + 1) % playerHuds.Count;

            // Reducir la velocidad con el tiempo
            yield return new WaitForSeconds(delay);
            delay -= 0.01f;
        }

        // Seleccionar un jugador aleatorio
        int selectedPlayerIndex = Random.Range(0, playerHuds.Count);
        Transform selectedHud = playerHuds[selectedPlayerIndex];

        // Mover la flecha al jugador seleccionado
        Vector3 selectedHudPosition = selectedHud.position;
        arrowImage.transform.position = new Vector3(selectedHudPosition.x, selectedHudPosition.y - 200f, selectedHudPosition.z);
        currPlayer = playersLocal[selectedPlayerIndex];
        GameUIManager.ActiveTurn(currPlayer.Data.UID, true);

        // Modificar al jugador inicial
        UpdateTurnPlayer(selectedPlayerIndex);
        UpdateInitialPlayer(selectedPlayerIndex);

        // Despues de un tiempo, destruir la flecha
        AudioManager.StopSoundSFX();
        yield return new WaitForSeconds(2f);
        DespawnArrow();
    }

    public void DespawnArrow()
    {
        if (spawnedArrow != null)
        {
            Destroy(spawnedArrow);
            spawnedArrow = null;
        }
        StartIntroCinematic();
    }

    #endregion

}
