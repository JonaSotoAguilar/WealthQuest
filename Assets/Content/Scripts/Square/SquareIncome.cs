using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareIncome : Square
{
    public override List<CardBase> GetCards()
    {
        return data.GetRandomIncomeCards(1).Cast<CardBase>().ToList();
    }

}
