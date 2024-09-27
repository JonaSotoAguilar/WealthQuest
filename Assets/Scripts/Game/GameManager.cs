using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq; // Asegúrate de importar LINQ

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance
    public Camera playerCamera; // Cámara para el jugador
    public Camera diceCamera; // Cámara para el dado
    private int turnPlayer = 0; // Turno del jugador
    public GameState gameState = GameState.EnCurso; // Estado del juego
    private bool initTurn = true;  // Controla si es posible iniciar el turno

    // Singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Inicialización
    private void Start()
    {
        CanvasManager.instance.UpdateHUD(FindObjectsByType<PlayerController>(FindObjectsSortMode.None)
            .FirstOrDefault(p => p.GetPlayerIndex() == turnPlayer)); // Actualizar el HUD
    }

    // Actualizar el turno
    public void UpdateTurn()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None); // Obtener jugadores una sola vez

        if (players.All(p => p.GetPlayerState() != GameState.EnCurso))  // Si ningún jugador está en curso
        {
            gameState = GameState.Finalizado; // Cambiar el estado del juego a Finalizado
            Debug.Log("Todos los jugadores han terminado. El juego ha finalizado.");
        }
        else
        {
            // Buscar el siguiente jugador en curso
            PlayerController player;
            do
            {
                turnPlayer = (turnPlayer + 1) % players.Length; // Cambiar al siguiente jugador en el array
                player = players.FirstOrDefault(p => p.GetPlayerIndex() == turnPlayer); // Obtener jugador con indice igual al turno actual
            } while (player.GetPlayerState() != GameState.EnCurso); // Solo pasar si está en curso

            CanvasManager.instance.UpdateHUD(player); // Actualizar el HUD 
            initTurn = true; // Permite iniciar el siguiente turno
        }
    }

    // Cambiar a la vista del dado
    public void ChangeDiceView()
    {
        playerCamera.enabled = false;
        diceCamera.enabled = true;
    }

    // Cambiar a la vista del jugador
    public void ChangePlayerView()
    {
        playerCamera.enabled = true;
        diceCamera.enabled = false;
    }

    // Comprobar si es posible jugar el turno
    public bool CanPlayTurn(int playerIndex) => initTurn && playerIndex == turnPlayer;

    //public bool InitTurn { get => initTurn; set => initTurn = value; }

    public bool GetInitTurn()
    {
        return initTurn;
    }

    public void SetInitTurn(bool value)
    {
        initTurn = value;
    }
}

