using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerControllerNetwork : PlayerNetwork
{
    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;
    private PlayerCanvas playerCanvas;
    //private PlayerMovement playerMovement;

}
