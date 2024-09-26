using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Camera playerCamera;
    public Camera diceCamera;
    public PlayerMovement playerMovement;
    public DiceController diceController;
    public bool isQuestionActive = false; // Nueva bandera para verificar si hay una pregunta activa
    public GameData gameData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        playerCamera.enabled = true;
        diceCamera.enabled = false;

        gameData.InitializePlayers(1); // Initialize with 1 player (Test)
        HUDManager.instance.ActualizarHUD(gameData.players[gameData.currentPlayer]);
    }

    void Update()
    {
        // No permitir lanzar los dados si el juego ha terminado o si una pregunta está activa
        if (gameData.gameState == GameState.Finalizado || gameData.players[gameData.currentPlayer].playerState == GameState.Finalizado)
        {
            Debug.Log("El juego ha finalizado.");
            return;
        }

        // No permitimos acciones mientras el dado se esté moviendo
        if (diceController.isDiceLaunched || isQuestionActive)
        {
            return;
        }

        // Lógica para lanzar el dado (solo cuando corresponda)
        if (Input.GetKeyDown(KeyCode.Space) && playerCamera.enabled && !playerMovement.IsMoving())
        {
            LaunchDice();
        }
    }

    public void LaunchDice()
    {
        if (gameData.gameState == GameState.Finalizado)
        {
            Debug.Log("El juego ha terminado. No se puede lanzar dados.");
            return;
        }

        playerCamera.enabled = false;
        diceCamera.enabled = true;
        diceController.LaunchDice();
    }

    public void FinishDiceRoll(int result)
    {
        if (gameData.gameState == GameState.Finalizado)
        {
            Debug.Log("El juego ha terminado. No se puede mover jugadores.");
            return;
        }

        playerCamera.enabled = true;
        diceCamera.enabled = false;

        playerMovement.MoverJugador(result);

        // Actualiza el turno en GameData
        gameData.UpdateTurn();
    }
}