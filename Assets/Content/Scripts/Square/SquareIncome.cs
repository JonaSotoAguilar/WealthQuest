using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareIncome : Square
{
    public override void GetCards() => selectedCards = GameManager.Instance.GameData.GetRandomIncomeCards(1).Cast<CardBase>().ToList();
}
