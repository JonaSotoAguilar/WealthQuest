using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using Mirror.BouncyCastle.Bcpg;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameNetManager : NetworkBehaviour
{
    private static GameNetManager instance;
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
    private List<PlayerNetManager> playersNet = new List<PlayerNetManager>();
    private PlayerNetManager currPlayer;

    // SyncVars
    [SyncVar] private string content = "Default";
    [SyncVar] private string timePlayed = "00:00:00";
    [SyncVar(hook = nameof(OnChangeYear))] private int currentYear = 0;
    [SyncVar] private int turnPlayer = 0;
    private int readyBanner = 0;

    # region Getters

    public static GameData Data { get => instance.gameData; }
    public static List<PlayerNetManager> Players { get => instance.playersNet; }
    public static PlayerNetManager CurrentPlayer { get => instance.currPlayer; }
    public static PlayerNetManager GetPlayer(string clientID) => instance.playersNet.Find(player => player.Data.UID == clientID);
    public static int CurrentYear { get => instance.currentYear; }

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

    public static void PlayerJoined(PlayerNetManager player)
    {
        if (instance == null) return;

        instance.playersNet.Add(player);
    }

    [Server]
    public static void InitializeGame()
    {
        if (instance == null) return;

        instance.UpdateYear(Data.currentYear);
        instance.RpcActiveUI(true);

        if (instance.gameData.timePlayed == "00:00:00" && instance.gameData.playersData.Count >= 1)
            instance.StartSelection();
        else
            StartGame();
    }

    [Server]
    public static void StartGame()
    {
        if (instance == null) return;

        // 1. Position players
        instance.InitializePosition();

        // 2. Status game
        instance.status = GameStatus.Playing;
        instance.currPlayer = instance.playersNet[Data.turnPlayer];

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
        List<Square> squares = new List<Square>();
        List<int> positions = new List<int>();
        foreach (var player in playersNet)
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
        instance.NextYear();

        if (instance.status == GameStatus.Finish) return;
        instance.NextPlayer();
        instance.SaveGame();
        instance._camera.CurrentCamera(instance.currPlayer.transform);
        instance.StartTurn();
    }

    [Server]
    private void NextPlayer()
    {
        RpcResetPlayerHUD(currPlayer.Data.UID);
        int nexTurn = (gameData.turnPlayer + 1) % gameData.playersData.Count;
        UpdateTurnPlayer(nexTurn);
        currPlayer = playersNet[nexTurn];
    }

    [Server]
    private void NextYear()
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
        foreach (var player in playersNet)
            player.Data.ProccessFinances();

        UpdateYear(newYear);
    }

    [Server]
    private void FinishGame()
    {
        // 1. Calculate winner
        PlayerNetManager winner = playersNet[0];
        foreach (var player in playersNet)
        {
            player.Data.SetFinalScore();
            if (player.Data.FinalScore > winner.Data.FinalScore)
                winner = player;
        }

        // 2. Announce winner (Cinematic)


        // 3. Save History
        RpcSaveHistory();

        // 4. Close game (Return to main menu)
        LoadMenu();
    }

    [ClientRpc]
    private void RpcSaveHistory()
    {
        RelayService.Instance.GameFinished();
        int score = GetPlayer(ProfileUser.uid).Data.FinalScore;
        FinishGameData finishData = new FinishGameData(currentYear, timePlayed, content, score);
        ProfileUser.SaveGame(finishData, 3);
    }

    [Server]
    private void LoadMenu()
    {
        RelayService.Instance.FinishGame();
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
        await GameUIManager.SetPlayerTurn(clientID, true, false);
        CmdReadyNextPlayer(clientID);
    }

    [Command(requiresAuthority = false)]
    private void CmdReadyNextPlayer(string clientID)
    {
        readyBanner++;

        if (readyBanner == playersNet.Count)
        {
            readyBanner = 0;
            currPlayer.StartTurn();
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

        // Actualizar tiempo jugado
        TimeSpan totalSpan = TimeSpan.Parse(gameData.timePlayed);
        TimeSpan timeSpan = totalSpan + currentSpan;
        timePlayed = timeSpan.ToString(@"hh\:mm\:ss");
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
        turnPlayer = newTurn;
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
            instance.RpcActiveUI(false);
            cinematicDirector.Play();
            cinematicDirector.stopped += OnIntroCinematicEnd;
        }
        else
        {
            if (isClient && isServer) StartGame();
        }
    }

    [Server]
    private void OnIntroCinematicEnd(PlayableDirector director)
    {
        if (director == cinematicDirector)
        {
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

}