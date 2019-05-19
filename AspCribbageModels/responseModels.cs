using System.Collections.Generic;

namespace CribbageModels
{ 
    public class CutCardsResponse
    {

        public CutCardsResponse(Card Player, Card computer, string url)
        {
            this.ComputerCard = computer;
            this.PlayerCard = Player;
            this.RepeatUrl = url;
        }

        public Card PlayerCard { get; internal set; }
        public Card ComputerCard { get; internal set; }
        public string RepeatUrl { get; internal set; }

    }
    public class ScoreResponse
    {

        public ScoreResponse(int score, List<Score> scoreList)
        {
            this.TotalScore = score;
            this.Scores = scoreList;
        }

        public int TotalScore { get; set; } = 0;
        public List<Score> Scores { get; set; } = null;
    }

    public class CountedCardResponse
    {
        public CountedCardResponse(Card card, int score, List<Score> scores)
        {
            CountedCard = card;
            TotalScore = score;
            Scores = scores;
        }
        public Card CountedCard { get; set; }
        public int TotalScore { get; set; } = 0;
        public List<Score> Scores { get; set; } = null;

    }

    public class GetRandomHandResponse
    {
        public List<Card> PlayerCards { get; internal set; }
        public List<Card> ComputerCards { get; internal set; }
        public List<Card> ComputerCribCards { get; internal set; }
        public Card SharedCard { get; internal set; }
        public bool HisNobs { get; internal set; }
        public string RepeatUrl { get; internal set; }

        public GetRandomHandResponse(List<Card> pCards, List<Card> cCards, List<Card> crib, Card shared, bool nobs, string url)
        {
            this.PlayerCards = pCards;
            this.ComputerCards = cCards;
            this.ComputerCribCards = crib;
            this.SharedCard = shared;
            this.HisNobs = nobs;
            this.RepeatUrl = url;
        }
    }
}
