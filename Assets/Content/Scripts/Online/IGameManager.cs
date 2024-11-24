using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
    #region Methods Getters & Setters

    public Square[] Squares { get; }
    public List<IPlayer> Players { get; }

    #endregion
}
