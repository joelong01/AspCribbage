using System.Collections.Generic;

namespace CribbageModels
{
    public enum CardName
    {
        AceOfClubs = 0,
        TwoOfClubs = 1,
        ThreeOfClubs = 2,
        FourOfClubs = 3,
        FiveOfClubs = 4,
        SixOfClubs = 5,
        SevenOfClubs = 6,
        EightOfClubs = 7,
        NineOfClubs = 8,
        TenOfClubs = 9,
        JackOfClubs = 10,
        QueenOfClubs = 11,
        KingOfClubs = 12,
        AceOfDiamonds = 13,
        TwoOfDiamonds = 14,
        ThreeOfDiamonds = 15,
        FourOfDiamonds = 16,
        FiveOfDiamonds = 17,
        SixOfDiamonds = 18,
        SevenOfDiamonds = 19,
        EightOfDiamonds = 20,
        NineOfDiamonds = 21,
        TenOfDiamonds = 22,
        JackOfDiamonds = 23,
        QueenOfDiamonds = 24,
        KingOfDiamonds = 25,
        AceOfHearts = 26,
        TwoOfHearts = 27,
        ThreeOfHearts = 28,
        FourOfHearts = 29,
        FiveOfHearts = 30,
        SixOfHearts = 31,
        SevenOfHearts = 32,
        EightOfHearts = 33,
        NineOfHearts = 34,
        TenOfHearts = 35,
        JackOfHearts = 36,
        QueenOfHearts = 37,
        KingOfHearts = 38,
        AceOfSpades = 39,
        TwoOfSpades = 40,
        ThreeOfSpades = 41,
        FourOfSpades = 42,
        FiveOfSpades = 43,
        SixOfSpades = 44,
        SevenOfSpades = 45,
        EightOfSpades = 46,
        NineOfSpades = 47,
        TenOfSpades = 48,
        JackOfSpades = 49,
        QueenOfSpades = 50,
        KingOfSpades = 51,
        BackOfCard = 52,
        Uninitialized = 53
    }

    public enum CardOrdinal
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King
    }

    public enum Suit
    {
        Clubs = 1,
        Diamonds = 2,
        Hearts = 3,
        Spades = 4
    }




    public class Card
    {
        public CardName CardName { get; internal set; }
        public CardOrdinal Ordinal { get; internal set; }
        public Suit Suit { get; internal set; }
        public int Rank { get; internal set; } // 1...13
        public int Value { get; internal set; } // 1..10
        public Owner Owner { get; set; }


        public Card(CardName card, CardOrdinal ordinal, int rank, int value, Suit suit, Owner owner)
        {
            CardName = card;
            Ordinal = ordinal;
            Rank = rank;
            Value = value;
            Suit = suit;
        }

        public static Card CardNameToCard(CardName card)
        {
            int ordinal = (int)card % 13 + 1;
            Suit suit = (Suit)((int)card / 13 + 1);
            return new Card(card, (CardOrdinal)ordinal, ordinal, ordinal < 10 ? ordinal : 10, suit, Owner.Uninitialized);
        }

        public Card(CardName name)
        {
            CardName = name;
        }

        public Card() // called by the XAML editor
        {
        }

        public object Tag { get; set; } = null;
        public bool IsEnabled { get; set; } = true;

        public override string ToString()
        {
            return CardName.ToString();
        }

        public static string CardsToString(List<Card> cards)
        {
            string s = "";
            foreach (Card c in cards)
            {
                s += c + "-";
            }

            return s;
        }

        public static int CompareCardsByRank(Card x, Card y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }

            // If x is not null...
            // 
            if (y == null)
            {
                // ...and y is null, x is greater.
                return 1;
            }

            return x.Rank - y.Rank;
        }

        public static int CompareCardsBySuit(Card x, Card y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }

            // If x is not null...
            // 
            if (y == null)
            {
                // ...and y is null, x is greater.
                return 1;
            }

            return (int)y.Suit - (int)x.Suit;
        }

        public static int CompareCardNamesByValue(CardName x, CardName y)
        {
            return (int)x - (int)y;
        }
    }
}