using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareEvent : Square
{
    public override void GetCards() => selectedCards = GameManager.Instance.GameData.GetRandomEventCards(1).Cast<CardBase>().ToList();
}
