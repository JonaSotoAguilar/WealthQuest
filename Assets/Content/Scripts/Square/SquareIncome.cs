using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareIncome : Square
{
    public override List<Card> GetCards()
    {
        return data.GetRandomIncomeCards(2).Cast<Card>().ToList();
    }
}
