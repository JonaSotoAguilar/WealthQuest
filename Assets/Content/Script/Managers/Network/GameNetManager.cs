using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using Mirror.BouncyCastle.Bcpg;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameNetManager : NetworkBehaviour
{
    private static GameNetManager instance;

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
    private List<PlayerNetManager> playersNet = new List<PlayerNetManager>();
    private PlayerNetManager currPlayer;

    // SyncVars
    [SyncVar] private string content = "Default";
    [SyncVar] private string timePlayed = "00:00:00";
    [SyncVar(hook = nameof(OnChangeYear))] private int currentYear = 0;
    [SyncVar] private bool isClosed = false;
    private int readyPlayer = 0;

    # region Getters

    public static GameData Data { get => instance.gameData; }
    public static List<PlayerNetManager> Players { get => instance.playersNet; }
    public static PlayerNetManager CurrentPlayer { get => instance.currPlayer; }
    public static PlayerNetManager GetPlayer(string clientID) => instance.playersNet.Find(player => player.Data.UID == clientID);
    public static bool IsHost => instance.isServer && instance.isClient;

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
        GameUIManager.SetLocal(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static void PlayerJoined(PlayerNetManager player)
    {
        if (instance == null) return;

        instance.playersNet.Add(player);
    }

    [Server]
    public static void InitializeGame()
    {
        if (instance == null) return;

        instance.StartCoroutine(instance.StartGameNet());
    }

    private IEnumerator StartGameNet()
    {
        yield return new WaitForSeconds(1.2f);

        instance.UpdateYear(Data.currentYear);
        instance.RpcActiveUI(true);

        if (instance.gameData.timePlayed != "00:00:00")
        {
            StartGame();
        }
        else if (instance.gameData.playersData.Count > 1)
            instance.StartSelection();
        else
            instance.StartIntroCinematic();
    }

    [Server]
    public static void StartGame()
    {
        if (instance == null) return;

        // 1. Player turn
        instance.currPlayer = instance.playersNet[Data.turnPlayer];

        // 2. Position players
        instance.InitializePosition();

        // 3. Camera
        instance._camera.CurrentCamera(instance.currPlayer.transform);

        // 4. Start game
        instance.content = instance.gameData.content;
        instance.currentTime = DateTime.Now;
        instance.StartTurn();
    }

    [Server]
    private void InitializePosition()
    {
        int position;
        foreach (var player in playersNet)
        {
            position = player.Data.Position;
            SquareManager.Squares[position].AddPlayer(player.gameObject);
        }

        position = currPlayer.Data.Position;
        SquareManager.Squares[position].RemovePlayer(currPlayer.gameObject);
    }
    #endregion

    #region Turn Management

    [Server]
    private void StartTurn()
    {
        RpcNextPlayer(currPlayer.Data.UID);
    }

    [Server]
    public static void FinishTurn()
    {
        if (instance == null) return;

        instance.UpdateTime();
        instance.NextTurn();
    }

    [Server]
    public static void UpdateNextTurn()
    {
        instance.NextPlayer();
        instance.SaveGame();
        instance.StartTurn();
    }

    [Server]
    private void NextPlayer()
    {
        // 1. Posiciona en esquina al jugador actual
        int position = currPlayer.Data.Position;
        SquareManager.Squares[position].AddPlayer(currPlayer.gameObject);

        // 2. Obtiene el siguiente jugador
        RpcResetPlayerHUD(currPlayer.Data.UID);
        int nexTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        UpdateTurnPlayer(nexTurn);
        currPlayer = playersNet[nexTurn];

        // 3. Posiciona en esquina y centro el centro al siguiente jugador
        position = currPlayer.Data.Position;
        SquareManager.Squares[position].RemovePlayer(currPlayer.gameObject);

        // 4. Centrar la cámara en el jugador actual
        instance._camera.CurrentCamera(instance.currPlayer.transform);
    }

    [Server]
    private void NextTurn()
    {
        int nextTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        if (nextTurn != gameData.initialPlayerIndex)
        {
            UpdateNextTurn();
        }
        else
        {
            NextYearProcess();
        }
    }

    [Server]
    private void NextYearProcess()
    {
        int newYear = gameData.currentYear + 1;
        if (newYear > gameData.yearsToPlay)
        {
            FinishGame();
        }
        else
        {
            foreach (var player in playersNet)
                player.Data.ProccessFinances();

            RpcNextYear();
            UpdateYear(newYear);
        }
    }

    [Server]
    private void FinishGame()
    {
        // 1. Calcular el ganador después de un pequeño retraso para asegurar sincronización
        StartCoroutine(FinishGameProcess());
    }

    [Server]
    private IEnumerator FinishGameProcess()
    {
        yield return new WaitForSeconds(0.5f);

        // 1. Calcular puntajes finales
        SetFinalScore();

        // 2. Notificar a los clientes que el juego ha terminado
        RpcFinishGame();
    }

    [Server]
    private void SetFinalScore()
    {
        // Calcular puntajes finales para todos los jugadores
        foreach (var player in playersNet)
        {
            player.Data.SetFinalScore();
        }

        // Ordenar jugadores por criterios: FinalScore, Points y Money
        List<PlayerNetManager> sortedPlayers = new List<PlayerNetManager>(playersNet);
        sortedPlayers.Sort((a, b) =>
        {
            // Comparar FinalScore (mayor a menor)
            int finalScoreComparison = b.Data.FinalScore.CompareTo(a.Data.FinalScore);
            if (finalScoreComparison != 0) return finalScoreComparison;

            // Comparar Points (mayor a menor)
            int pointsComparison = b.Data.Points.CompareTo(a.Data.Points);
            if (pointsComparison != 0) return pointsComparison;

            // Comparar Final Capital (mayor a menor)
            int capitalComparison = b.Data.GetFinalCapital().CompareTo(a.Data.GetFinalCapital());
            if (capitalComparison != 0) return capitalComparison;

            // Comparar Money 
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

    [ClientRpc]
    private void RpcFinishGame()
    {
        // Esperar antes de ejecutar SaveHistory para asegurar que los datos se han recibido
        Invoke(nameof(SaveHistory), 0.5f);
        _ = GameUIManager.ShowFinishGame();
    }

    private void SaveHistory()
    {
        Debug.Log("Time: " + timePlayed + " Year: " + currentYear);
        var data = GetPlayer(ProfileUser.uid).Data;
        int score = data.FinalScore;
        int level = data.Level;
        Debug.Log("Score: " + score + " Level: " + level);

        FinishGameData finishData = new FinishGameData(currentYear, timePlayed, content, score, level);
        ProfileUser.SaveGame(finishData, 3);
    }

    #endregion

    #region UI

    [ClientRpc]
    private void RpcResetPlayerHUD(string clientID)
    {
        GameUIManager.ResetPlayerTurn(clientID);
    }

    [ClientRpc]
    private void RpcNextPlayer(string clientID)
    {
        _ = ReadyNextPlayer(clientID);
    }

    [Client]
    private async Task ReadyNextPlayer(string clientID)
    {
        await GameUIManager.SetPlayerTurn(clientID, true);
        CmdReadyNextPlayer();
    }

    [Command(requiresAuthority = false)]
    private void CmdReadyNextPlayer()
    {
        readyPlayer++;

        if (readyPlayer == playersNet.Count)
        {
            readyPlayer = 0;
            currPlayer.StartTurn();
        }
    }

    [ClientRpc]
    private void RpcNextYear()
    {
        _ = ReadyNextYear();
    }

    [Client]
    private async Task ReadyNextYear()
    {
        await GameUIManager.ShowNextYear();
        CmdReadyNextYear();
    }

    [Command(requiresAuthority = false)]
    private void CmdReadyNextYear()
    {
        readyPlayer++;

        if (readyPlayer == playersNet.Count)
        {
            readyPlayer = 0;
            UpdateNextTurn();
        }
    }

    [ClientRpc]
    private void RpcActiveTurn(string clientID, bool active)
    {
        GameUIManager.ActiveTurn(clientID, active);
    }

    [ClientRpc]
    private void RpcActiveUI(bool active)
    {
        GameUIManager.ShowPanel(active);
    }

    #endregion

    #region Game Data

    [Server]
    private void SaveGame()
    {
        StartCoroutine(SaveService.SaveGame(Data, 3));
    }

    [Server]
    private void UpdateTime()
    {
        // Calcular tiempo transcurrido
        DateTime timeNow = DateTime.Now;
        TimeSpan currentSpan = timeNow - currentTime;
        currentTime = timeNow;

        // Sumar tiempo al tiempo total del juego
        TimeSpan totalSpan = TimeSpan.Parse(gameData.timePlayed);
        TimeSpan newTimeSpan = totalSpan + currentSpan;

        // Asignar el nuevo valor y sincronizar con todos los clientes
        timePlayed = newTimeSpan.ToString(@"hh\:mm\:ss");
        gameData.timePlayed = timePlayed;
    }


    [Server]
    private void UpdateYear(int newYear)
    {
        currentYear = newYear;
        gameData.currentYear = newYear;
    }

    private void OnChangeYear(int oldYear, int newYear)
    {
        GameUIManager.ChangeYear(newYear);
    }

    [Server]
    private void UpdateTurnPlayer(int newTurn)
    {
        gameData.turnPlayer = newTurn;
    }

    [Server]
    private void UpdateInitialPlayer(int newInitial)
    {
        gameData.initialPlayerIndex = newInitial;
    }

    #endregion

    #region Animation

    [Server]
    private void StartIntroCinematic()
    {
        if (cinematicDirector != null)
        {
            RpcPauseDisable(true);
            instance.RpcActiveUI(false);

            // Reproducir la cinemática
            cinematicDirector.Play();

            // Ajustar la velocidad de reproducción del Timeline
            StartCoroutine(AdjustTimelineSpeed());

            // Registrar un evento para cuando termine
            cinematicDirector.stopped += OnIntroCinematicEnd;
        }
        else
        {
            if (isClient && isServer) StartGame();
        }
    }

    private IEnumerator AdjustTimelineSpeed()
    {
        // Esperar hasta que el PlayableGraph esté inicializado y válido
        yield return new WaitUntil(() => cinematicDirector.playableGraph.IsValid());

        // Aumentar la velocidad de reproducción
        cinematicDirector.playableGraph.GetRootPlayable(0).SetSpeed(2f);
    }

    [Server]
    private void OnIntroCinematicEnd(PlayableDirector director)
    {
        if (director == cinematicDirector)
        {
            RpcPauseDisable(false);
            cinematicDirector.stopped -= OnIntroCinematicEnd;
            instance.RpcActiveUI(true);
            StartGame();
        }
    }

    [Server]
    private void StartSelection()
    {
        if (playersNet.Count == 0)
        {
            Debug.LogError("No hay jugadores configurados.");
            return;
        }

        // Spawnear la flecha
        spawnedArrow = Instantiate(arrowPrefab);
        NetworkServer.Spawn(spawnedArrow);

        // Mover la flecha entre los Huds
        StartCoroutine(MoveArrow());
    }

    [Server]
    private IEnumerator MoveArrow()
    {
        int hudIndex = 0;
        float delay = 0.2f; // Tiempo entre movimientos

        // Obtener la lista de HUDs desde GameUIManager
        List<Transform> playerHuds = GameUIManager.HUDsList();

        // Acceder a la imagen de la flecha (hija de spawnedArrow)
        GameObject arrowImage = spawnedArrow.transform.GetChild(0).gameObject;

        RpcSoundArrow();
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
        currPlayer = playersNet[selectedPlayerIndex];
        RpcActiveTurn(currPlayer.Data.UID, true);

        // Modificar al jugador inicial
        UpdateTurnPlayer(selectedPlayerIndex);
        UpdateInitialPlayer(selectedPlayerIndex);

        // Despues de un tiempo, destruir la flecha
        StopRpcSoundArrow();
        yield return new WaitForSeconds(2f);
        DespawnArrow();
    }

    [Server]
    public void DespawnArrow()
    {
        if (spawnedArrow != null)
        {
            NetworkServer.Destroy(spawnedArrow);
            spawnedArrow = null;
        }
        StartIntroCinematic();
    }

    [ClientRpc]
    private void RpcPauseDisable(bool active)
    {
        PauseMenu.SetPauseDisabled(active);
    }

    #endregion

    #region Audio

    [ClientRpc]
    private void RpcSoundArrow()
    {
        AudioManager.PlaySoundArrow();
    }

    [ClientRpc]
    private void StopRpcSoundArrow()
    {
        AudioManager.StopSoundSFX();
    }

    #endregion

    #region Client Disconnection

    public static void Return()
    {
        if (instance.isClient && instance.isServer)
        {
            ServerClose();
        }
        else if (instance.isClient)
        {
            ServerClose();
        }
    }

    public static void ServerClose()
    {
        instance.CmdServerClose();
    }

    [Command(requiresAuthority = false)]
    private void CmdServerClose()
    {
        if (isClosed) return;
        isClosed = true;
        StartCoroutine(ServerCloseProcess());
    }

    [Server]
    private IEnumerator ServerCloseProcess()
    {
        Debug.Log("Server Close Process");
        RpcMessageClose();
        yield return new WaitForSeconds(1.2f);
        RelayService.Instance.FinishGame();
    }

    [ClientRpc]
    private void RpcMessageClose()
    {
        GameUIManager.OpenMessagePopup("El servidor ha cerrado la conexión.\nVolviendo al menú principal.");
    }

    #endregion

}