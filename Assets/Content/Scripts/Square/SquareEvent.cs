using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SquareEvent : Square
{
    public override List<Card> GetCards()
    {
        return data.GetRandomEventCards(1).Cast<Card>().ToList();
    }
}
