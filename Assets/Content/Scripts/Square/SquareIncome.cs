using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareIncome : Square
{
    public override void GetCards(PlayerController player)
    {
        selectedCards = GameManager.Instance.GameData.GetRandomIncomeCards(2).Cast<CardBase>().ToList();
    }
}
