using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Camera playerCamera;
    public Camera diceCamera;
    public PlayerMovement playerMovement;
    public DiceController diceController;

    public PlayerStats[] jugadores;  // Array de jugadores, cada uno con sus stats
    private int jugadorActual = 0;   // Índice del jugador actual

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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && playerCamera.enabled && !playerMovement.IsMoving())
        {
            LaunchDice();
        }
    }

    public void LaunchDice()
    {
        playerCamera.enabled = false;
        diceCamera.enabled = true;
        diceController.LaunchDice();
    }

    public void FinishDiceRoll(int result)
    {
        playerCamera.enabled = true;
        diceCamera.enabled = false;

        // Mover al jugador actual
        playerMovement.MoverJugador(result);

        // Actualizar las estadísticas del jugador actual
        jugadores[jugadorActual].ActualizarTurno();

        // Actualizar el HUD del jugador actual
        HUDManager.instance.ActualizarHUD(jugadores[jugadorActual]);

        // Pasar el turno al siguiente jugador
        CambiarTurno();
    }

    private void CambiarTurno()
    {
        jugadorActual = (jugadorActual + 1) % jugadores.Length; // Cambia al siguiente jugador en la lista
        //Debug.Log("Es el turno del jugador: " + jugadorActual);
    }
}
