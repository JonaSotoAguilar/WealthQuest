using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareExpense : Square
{
    public override void GetCards(PlayerController player)
    {
        selectedCards = GameManager.Instance.GameData.GetRandomExpenseCards(2).Cast<CardBase>().ToList();
    }
}
