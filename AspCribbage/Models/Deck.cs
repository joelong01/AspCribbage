using Cards;
using System;
using System.Collections.Generic;


namespace AspCribbage.Models
{
    public class Deck
    {   
        public static List<Card> GetDeck()
        {
            List<Card> cards = new List<Card>();

            foreach (var card in (CardName[])Enum.GetValues(typeof(CardName)))
            {
                if ((int)card < 52)
                {                    
                    cards.Add(Card.CardNameToCard(card));                    
                }
            }
                
            return cards;
        }
    }
}
