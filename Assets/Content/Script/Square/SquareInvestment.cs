using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareInvestment : Square
{
    public override List<Card> GetCards()
    {
        return data.GetRandomInvestmentCards(2).Cast<Card>().ToList();
    }
}