using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
    #region Methods Getters & Setters

    public GameData GameData { get; }
    public Square[] Squares { get; }
    public List<IPlayer> Players { get; }
    public IPlayer CurrPlayer { get; set; }
    public IPlayer LocalPlayer { get; set; }

    #endregion
}
