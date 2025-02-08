using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class GameLocalManager : MonoBehaviour
{
    private static GameLocalManager instance;

    [Header("Components")]
    [SerializeField] private CameraManager _camera;
    [SerializeField] private PlayableDirector cinematicDirector;

    [Header("Status")]
    [SerializeField] private GameData gameData;
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

        if (instance.gameData.timePlayed == "00:00:00" && instance.gameData.playersData.Count > 1)
            instance.StartSelection();
        else
            instance.StartIntroCinematic();
    }

    private static void StartGame()
    {
        if (instance == null) return;

        // 1.  Player turn
        instance.currPlayer = instance.playersLocal[Data.turnPlayer];

        // 2. Position players
        instance.InitializePosition();

        // 3. Camera
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // 4. Start game
        instance.currentTime = DateTime.Now;
        _ = instance.StartTurn();
    }

    private void InitializePosition()
    {
        int position;
        foreach (var player in playersLocal)
        {
            position = player.Data.Position;
            SquareManager.Squares[position].AddPlayer(player.gameObject);
        }

        position = currPlayer.Data.Position;
        SquareManager.Squares[position].RemovePlayer(currPlayer.gameObject);
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

        instance.UpdateTime();
        instance.NextYear();
    }

    private static void UpdateNextTurn()
    {
        instance.NextPlayer();
        instance.SaveGame();
        _ = instance.StartTurn();
    }

    private void NextPlayer()
    {
        // 1. Posicionar jugador en esquina
        int position = currPlayer.Data.Position;
        SquareManager.Squares[position].AddPlayer(currPlayer.gameObject);

        // 2. Obtiene el siguiente jugador
        GameUIManager.ResetPlayerTurn(currPlayer.Data.UID);
        int nexTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        UpdateTurnPlayer(nexTurn);
        currPlayer = playersLocal[nexTurn];

        // 3. Posiciona en el centro al siguiente jugador
        position = currPlayer.Data.Position;
        SquareManager.Squares[position].RemovePlayer(currPlayer.gameObject);

        // 4. Centrar la cámara en el jugador actual
        instance._camera.CurrentCamera(instance.currPlayer.transform);
    }

    private void NextYear()
    {
        if (instance == null) return;

        int nextTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        if (nextTurn != gameData.initialPlayerIndex)
        {
            UpdateNextTurn();
        }
        else
        {
            _ = NextYearProcess();
        }
    }

    private async Task NextYearProcess()
    {
        int newYear = gameData.currentYear + 1;
        if (newYear > gameData.yearsToPlay)
        {
            FinishGame();
        }
        else
        {
            // Procesar finanzas de cada jugador
            foreach (var player in playersLocal)
                player.Data.ProccessFinances();

            // Esperar a que termine la animación de Next Year antes de continuar
            await GameUIManager.ShowNextYear();
            UpdateYear(newYear);
            UpdateNextTurn();
        }
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

        // Ordenar jugadores por criterios: FinalScore, Points y Money
        List<PlayerLocalManager> sortedPlayers = new List<PlayerLocalManager>(playersLocal);
        sortedPlayers.Sort((a, b) =>
        {
            // Comparar FinalScore (mayor a menor)
            int finalScoreComparison = b.Data.FinalScore.CompareTo(a.Data.FinalScore);
            if (finalScoreComparison != 0) return finalScoreComparison;

            // Si hay empate en FinalScore, comparar Points (mayor a menor)
            int pointsComparison = b.Data.Points.CompareTo(a.Data.Points);
            if (pointsComparison != 0) return pointsComparison;

            // Si hay empate en Points, comparar Money (mayor a menor)
            return b.Data.Money.CompareTo(a.Data.Money);
        });

        // Asignar posiciones considerando empates
        int currentRank = 1; // Comenzar desde la posición 1
        int playersAtRank = 0; // Número de jugadores en la posición actual
        int previousFinalScore = sortedPlayers[0].Data.FinalScore;
        int previousPoints = sortedPlayers[0].Data.Points;
        int previousMoney = sortedPlayers[0].Data.Money;

        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            var player = sortedPlayers[i];

            // Verificar si se rompe el empate al comparar criterios
            if (player.Data.FinalScore < previousFinalScore ||
                (player.Data.FinalScore == previousFinalScore && player.Data.Points < previousPoints) ||
                (player.Data.FinalScore == previousFinalScore && player.Data.Points == previousPoints && player.Data.Money < previousMoney))
            {
                // Avanzar en el ranking si hay diferencia en algún criterio
                currentRank += playersAtRank;
                playersAtRank = 0; // Reiniciar el contador de jugadores en la posición actual
            }

            // Asignar la posición actual al jugador
            player.Data.SetResultPosition(currentRank);
            playersAtRank++;

            // Actualizar los criterios previos
            previousFinalScore = player.Data.FinalScore;
            previousPoints = player.Data.Points;
            previousMoney = player.Data.Money;
        }
    }

    private void ShowResults()
    {
        _ = GameUIManager.ShowFinishGame();
    }

    private void SaveHistory()
    {
        int score = GetPlayer(ProfileUser.uid).Data.FinalScore;
        int level = GetPlayer(ProfileUser.uid).Data.Level;
        FinishGameData finishData = new FinishGameData(gameData.currentYear, gameData.timePlayed, gameData.content, score, level);
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
        if (cinematicDirector != null && cinematicDirector.playableAsset != null)
        {
            PauseMenu.SetPauseDisabled(true);
            GameUIManager.ShowPanel(false);

            // Reproducir la cinemática
            cinematicDirector.Play();

            // Ajustar la velocidad del Timeline después de asegurarse de que el gráfico esté válido
            StartCoroutine(AdjustTimelineSpeed());

            // Registrar un evento para cuando termine
            cinematicDirector.stopped += OnIntroCinematicEnd;
        }
        else
        {
            StartGame(); // Si no hay cinemática, comienza el juego directamente
        }
    }

    private IEnumerator AdjustTimelineSpeed()
    {
        // Esperar hasta que el PlayableGraph esté inicializado y válido
        yield return new WaitUntil(() => cinematicDirector.playableGraph.IsValid());

        // Aumentar la velocidad de reproducción
        cinematicDirector.playableGraph.GetRootPlayable(0).SetSpeed(1.5f);
    }


    private void OnIntroCinematicEnd(PlayableDirector director)
    {
        if (director == cinematicDirector)
        {
            PauseMenu.SetPauseDisabled(false);
            // Desregistrar el evento
            cinematicDirector.stopped -= OnIntroCinematicEnd;

            // Continuar con el flujo del juego
            GameUIManager.ShowPanel(true);
            StartGame();
        }
    }

    private void StartSelection()
    {
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
