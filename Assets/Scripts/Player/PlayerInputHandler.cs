using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
using System.Linq;

[System.Serializable]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput; // Entrada del jugador
    private PlayerController player; // Controlador del jugador
    private DiceController diceController; // Controlador del dado
    //private int numPlayers = 2; // Número de jugadores

    // Initialization
    private void Awake()
    {
        diceController = FindFirstObjectByType<DiceController>();
        playerInput = GetComponent<PlayerInput>();
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        var index = playerInput.playerIndex;
        player = players.FirstOrDefault(p => p.GetPlayerIndex() == index);
    }

    // Jugar turno
    public void PlayTurn(CallbackContext context)
    {
        // Comprobar si es posible jugar el turno
        if (GameManager.Instance.CanPlayTurn(player.GetPlayerIndex()))
        {
            GameManager.Instance.SetInitTurn(false); // Ya inició el turno
            StartCoroutine(ThrowDice());
        }
    }

    // Lanzar el dado
    public IEnumerator ThrowDice()
    {
        // Esperar hasta que el dado se detenga y obtener el resultados
        GameManager.Instance.ChangeDiceView(); // Cambiar a la vista del jugador
        diceController.LaunchDice(); // Lanzar el dado
        while (!diceController.IsDiceStopped())
        {
            yield return null;
        }

        StartCoroutine(MovePlayer()); // Mover al jugador actual
    }

    // Mover al jugador actual
    private IEnumerator MovePlayer()
    {
        // Mover al jugador actual y esperar hasta que se detenga
        GameManager.Instance.ChangePlayerView();
        player.MovePlayer(diceController.diceRoll);
        while (!player.IsPlayerStopped())
        {
            yield return null;
        }

        StartCoroutine(PlaySquare());
    }

    // Jugar casilla
    private IEnumerator PlaySquare()
    {
        // Esperar hasta que la casilla se detenga
        Square square = SquareManager.Instance.Squares[player.CurrentPlayerPosition()].GetComponent<Square>();

        // Aquí pasamos el jugador y su dispositivo
        square.ActiveSquare(player); // Pasamos el dispositivo del jugador actual

        while (!square.IsSquareStopped())
        {
            yield return null;
        }

        CanvasManager.instance.UpdateHUD(player); // Actualizar el HUD
        GameManager.Instance.UpdateTurn(); // Actualizar el turno
    }
}
