using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Camera playerCamera;
    public Camera diceCamera;
    public PlayerMovement playerMovement;
    public DiceController diceController;

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
        if (Input.GetKeyDown(KeyCode.Space) && playerCamera.enabled && !playerMovement.IsMoving()) // Asegurarse de que el jugador no esté moviéndose
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
        playerMovement.MoverJugador(result);
    }
}
