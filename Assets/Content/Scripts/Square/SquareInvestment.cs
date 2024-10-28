using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareInvestment : Square
{
    public override void GetCards(PlayerController player)
    {
        selectedCards = GameManager.Instance.GameData.GetRandomInvestmentCards(2).Cast<CardBase>().ToList();
    }
}