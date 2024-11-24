using FishNet.Object;
using UnityEngine;

public class PlayerNetManager : NetworkBehaviour
{
    [SerializeField] private PlayerNetData data;
    [SerializeField] private PlayerNetMovement movement;
    [SerializeField] private PlayerNetUI ui;

    public PlayerNetData Data { get => data; }
    public PlayerNetMovement Movement { get => movement; }

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameNetManager.PlayerJoined(data.UID, this);
        GameUINetManager.PlayerJoined(data.UID);
    }

    [Server]
    public void StartTurn()
    {
        //1. Centrar jugador
        movement.CenterPlayer(data.Position);

        //2. Crear pregunta
        ui.CreateQuestion();
    }
}
