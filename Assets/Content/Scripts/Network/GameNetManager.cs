using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class GameNetManager : NetworkBehaviour
{
    private static GameNetManager instance;
    enum GameStatus { None, Playing, Finish }

    [SerializeField] private GameData gameData;
    [SerializeField] private Square[] squares;

    [Header("Components")]
    [SerializeField] private CameraManager _camera;

    [Header("Status")]
    [SerializeField] private GameStatus status = GameStatus.None;
    [SerializeField] private DateTime currTime;

    private List<PlayerNetManager> playersNet = new List<PlayerNetManager>();
    private PlayerNetManager currPlayer;

    public static GameData Data { get => instance.gameData; }
    public static Square[] Squares { get => instance.squares; }

    [SerializeField] private Button btnStartGame;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        InitializeSquares();
        btnStartGame.onClick.AddListener(() => CmdStartGame());
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        gameData.ClearGameData();
        StartCoroutine(gameData.LoadCardsAndQuestions("Default"));
    }

    #region Initialization

    private void InitializeSquares()
    {
        Transform containerSquares = GameObject.Find("Squares").transform;
        squares = new Square[containerSquares.childCount];
        for (int i = 0; i < squares.Length; i++)
            squares[i] = containerSquares.GetChild(i).GetComponent<Square>();
    }

    public static void PlayerJoined(PlayerNetManager player)
    {
        instance.playersNet.Add(player);

        //FIXME: Cambiar UID
        Data.playersData.Add(new PlayerData(player.Data.UID, 0, player.Data.Nickname, player.Data.CharacterID));
    }

    public static PlayerNetManager GetPlayer(string clientID)
    {
        return instance.playersNet.Find(player => player.Data.UID == clientID);
    }

    [Command(requiresAuthority = false)]
    public void CmdStartGame()
    {
        InitializeGame();
    }

    [Server]
    public static void InitializeGame()
    {
        //1. Update year
        GameUINetManager.UpdateYear(Data.currentYear);
        //2. Position players
        instance.InitializePosition();
        //3. Status game
        instance.status = GameStatus.Playing;
        instance.currTime = DateTime.Now;

        instance.currPlayer = instance.playersNet[Data.turnPlayer];
        instance._camera.CurrentCamera(instance.currPlayer.transform);
        //4. Start game
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
            Square currSquare = Squares[pos];
            currSquare.players.Add(player.Movement);
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

    [Server]
    private void StartTurn()
    {
        currPlayer.StartTurn();
    }

    [Server]
    private void NextPlayer()
    {
        int nextIndex = (Data.turnPlayer + 1) % Data.playersData.Count;
        Data.turnPlayer = nextIndex;
        currPlayer = playersNet[nextIndex];
    }


    #endregion
}