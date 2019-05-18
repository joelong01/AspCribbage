namespace AspCribbage.Models
{
    public enum CardNames
    {
        AceOfClubs = 0, TwoOfClubs = 1, ThreeOfClubs = 2, FourOfClubs = 3, FiveOfClubs = 4, SixOfClubs = 5, SevenOfClubs = 6, EightOfClubs = 7, NineOfClubs = 8, TenOfClubs = 9, JackOfClubs = 10, QueenOfClubs = 11, KingOfClubs = 12,
        AceOfDiamonds = 13, TwoOfDiamonds = 14, ThreeOfDiamonds = 15, FourOfDiamonds = 16, FiveOfDiamonds = 17, SixOfDiamonds = 18, SevenOfDiamonds = 19, EightOfDiamonds = 20, NineOfDiamonds = 21, TenOfDiamonds = 22, JackOfDiamonds = 23, QueenOfDiamonds = 24, KingOfDiamonds = 25,
        AceOfHearts = 26, TwoOfHearts = 27, ThreeOfHearts = 28, FourOfHearts = 29, FiveOfHearts = 30, SixOfHearts = 31, SevenOfHearts = 32, EightOfHearts = 33, NineOfHearts = 34, TenOfHearts = 35, JackOfHearts = 36, QueenOfHearts = 37, KingOfHearts = 38,
        AceOfSpades = 39, TwoOfSpades = 40, ThreeOfSpades = 41, FourOfSpades = 42, FiveOfSpades = 43, SixOfSpades = 44, SevenOfSpades = 45, EightOfSpades = 46, NineOfSpades = 47, TenOfSpades = 48, JackOfSpades = 49, QueenOfSpades = 50, KingOfSpades = 51,
        BlackJoker = 52, RedJoker = 53, BackOfCard = 54, Uninitialized = 55
    };

    public enum CardOrdinal
    {
        Uninitialized = 0, Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
    };

    public enum Location { Unintialized, Deck, Discarded, Computer, Player, Crib };
    public enum Owner { Computer, Player, Shared, Unknown };
    public enum Suit { Uninitialized = 0, Clubs = 1, Diamonds = 2, Hearts = 3, Spades = 4 };

    public class CardModel
    {
        public CardNames CardName { get; internal set; }
        public CardOrdinal Ordinal { get; internal set; }
        public Suit Suit { get; internal set; }
        public int Rank { get; internal set; } // 1...13
        public int Value { get; internal set; } // 1..10

        public Location Location { get; set; }
        public Owner Owner { get; set; }

        public CardModel(CardNames card, CardOrdinal ordinal, int rank, int value, Suit suit, Owner owner)
        {
            CardName = card;
            Ordinal = ordinal;
            Rank = rank;
            Value = value;
            Suit = suit;
            Owner = owner;
        }

        public static CardModel CardNameToCard(CardNames card)
        {
            int ordinal = (int)card % 13 + 1;
            Suit suit = (Suit)((int)((int)card / 13) + 1);
            return new CardModel(card, (CardOrdinal)ordinal, ordinal, ordinal < 10 ? ordinal : 10, suit, Owner.Unknown);            
        }
    }
}
