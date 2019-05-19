using CribbageModels;
using System;
using System.Collections.Generic;


namespace CribbageModels
{
    public class Deck
    {   
        public List<Card> Cards { get;  } = new List<Card>();

        public Deck()
        {
            
            foreach (var card in (CardName[])Enum.GetValues(typeof(CardName)))
            {
                if ((int)card < 52)
                {
                    Cards.Add(Card.CardNameToCard(card));                    
                }
            }
                
           
        }
    }
}
