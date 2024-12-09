using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareExpense : Square
{
    public override List<Card> GetCards()
    {
        return data.GetRandomExpenseCards(2).Cast<Card>().ToList();
    }
}
