using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;


namespace CribbageModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ScoreName
    {
        Fifteen,
        Run,
        Pair,
        ThreeOfaKind,
        FourOfAKind,
        HisNibs,
        HisNobs,
        CountedRun,
        ThirtyOne,
        Go,
        Flush,
        LastCard
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Owner
    {
        Player,
        Computer,
        Shared,
        Crib,
        Uninitialized
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HandType
    {
        Crib = 0,
        Hand = 1
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlayerType
    {
        Player = 0,
        Computer = 1
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Open
    {
        Up,
        Left,
        Right,
        Down
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ScoreType
    {
        Count,
        Hand,
        Crib,
        Cut,
        Saved,
        Unspecified
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatType
    {
        Count,
        Min,
        Max,
        Average,
        Total
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatViewType
    {
        Game,
        Hand,
        Crib,
        Counting
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum StatName
    {
        Ignored,
        Saved,

        /* Game stats */
        WonDeal,
        GamesStarted,
        GamesWon,
        GamesLost,
        TotalHandsPlayed,
        TotalCribsPlayed,
        TotalCountingSessions,
        SmallestWinMargin,
        LargestWinMargin,
        SkunkWins,
        CutAJack,

        /* Hand stats */
        HandMostPoints,
        HandTotalPoints,
        HandAveragePoints,
        Hand0Points,
        HandJackOfTheSameSuit,
        HandPairs,
        Hand3OfAKind,
        Hand4OfAKind,
        Hand4CardFlush,
        Hand5CardFlush,
        Hand3CardRun,
        Hand4CardRun,
        Hand5CardRun,
        Hand15s,

        /* Crib stats */
        CribMostPoints,
        CribTotalPoints,
        CribAveragePoints,
        Crib0Points,
        CribJackOfTheSameSuit,
        CribPairs,
        Crib3OfAKind,
        Crib4OfAKind,
        Crib5CardFlush,
        Crib3CardRun,
        Crib4CardRun,
        Crib5CardRun,
        Crib15s,

        /* Counting stats */
        CountingMostPoints,
        CountingTotalPoints,
        CountingAveragePoints,
        CountingPair,
        Counting3OfAKind,
        Counting4OfAKind,
        Counting3CardRun,
        Counting4CardRun,
        Counting5CardRun,
        Counting6CardRun,
        Counting7CardRun,
        CountingLastCard,
        CountingHit31,
        CountingGo,
        CountingHit15,
        CountingMuggins3CardRunLost
    }

   
    public class ScoreCollection
    {
        public ScoreCollection()
        {
            Accepted = false;
            Scores = new List<ScoreInstance>();
            Total = 0;
            ScoreType = ScoreType.Unspecified;
            ActualScore = 0;
        }

        public ScoreCollection(string s)
        {
            Scores = new List<ScoreInstance>();
            Load(s);
        }

        public List<ScoreInstance> Scores { get; set; }

        public ScoreType ScoreType { get; set; }

        public int Total { get; set; }
        public bool Accepted { get; set; }

        public int ActualScore { get; set; } // Total might be wrong because of Muggins

        //
        //  one line. everything afer the "=" sign.
        public string Save()
        {
            var s = $"{ScoreType}-{Total}-{Accepted}-{ActualScore}|";
            foreach (var scoreInstance in Scores)
            {
                s += scoreInstance.Save() + "|";
            }

            return s;
        }

        private bool Load(string s)
        {
            char[] sep1 = { '|' };
            char[] sep2 = { '-' };

            var tokens = s.Split(sep1, StringSplitOptions.RemoveEmptyEntries);
            var tokens2 = tokens[0].Split(sep2, StringSplitOptions.RemoveEmptyEntries);

            ScoreType = (ScoreType)Enum.Parse(typeof(ScoreType), tokens2[0]);
            Total = Convert.ToInt32(tokens2[1]);
            Accepted = Convert.ToBoolean(tokens2[2]);
            ActualScore = Convert.ToInt32(tokens2[3]);

            for (var i = 1; i < tokens.Length; i++)
            {
                var scoreInstance = new ScoreInstance(tokens[i]);
                Scores.Add(scoreInstance);
            }

            return true;
        }

        public string LogString()
        {
            var s = "";

            foreach (var p in Scores)
            {
                s += string.Format("|{0}, {1}, {2}|-", p.Description, p.Count, p.Score);
            }

            s += string.Format("Total: {0}", Total);

            return s;
        }

        public string Format(bool includeHeader = true, bool formatForMessagebox = true, bool smallFormat = false)
        {
            var story = "";
            var tabs = "\t\t";
            var tab = "\t";

            var line = "";
            int len;

            if (smallFormat)
            {
                foreach (var p in Scores)
                {
                    line = string.Format("{0}{1}{2}\n", p.Description, tabs, p.Score);
                    story += line;
                }

                story += string.Format("\nTotal:\t{0}", Total);
                return story;
            }

            if (includeHeader)
            {
                story = formatForMessagebox ? string.Format("{0}\t\t{1}\t\t{2}\n", "Type", "Count", "Score") : string.Format("{0}\t\t{1}\t{2}\n", "Type", "     Count", "      Score");
            }

            foreach (var p in Scores)
            {
                len = p.Description.Length;
                if (len > 5 && !formatForMessagebox || p.Description == "Saved")
                {
                    line = $"{p.Description}{tabs}{p.Count}{tabs}{p.Score}\n";
                }
                else
                {
                    line = formatForMessagebox ? $"{p.Description}{tabs}{p.Count}{tabs}{p.Score}\n" : $"{p.Description}{tabs + tab}{p.Count}{tabs}{p.Score}\n";
                }

                story += line;
            }

            story += string.Format("\n\t\t\tTotal:\t{0}", Total);
            return story;
        }


        internal ScoreInstance GetScoreType(StatName statName)
        {
            foreach (var s in Scores)
            {
                if (s.ScoreType == statName)
                {
                    return s;
                }
            }

            return null;
        }
    }

    public class ScoreInstance
    {
        public ScoreInstance(string s)
        {
            Cards = new List<int>();
            Load(s);
        }

        public ScoreInstance(StatName name, int count, int score, List<int> cards)
        {
            ScoreType = name;
            Count = count;
            Score = score;
            Cards = new List<int>(cards);
        }

        public ScoreInstance(StatName name, int count, int score, int cardIndex)
        {
            ScoreType = name;
            Count = count;
            Score = score;
            Cards = new List<int>
            {
                cardIndex
            };
        }

        public string Description
        {
            get
            {
                return "TODO:  fix description";
            }
        }

        public int Count { get; set; }
        public int Score { get; set; }
        public StatName ScoreType { get; set; }


        public int ActualScore { get; set; }
        public StatName ActualScoreType { get; set; }


        public List<int> Cards { get; set; }

        //  put all state on one line
        public string Save()
        {
            var s = $"{Count},{Score},{ScoreType},{ActualScore},{ActualScoreType},";
            foreach (var i in Cards)
            {
                s += string.Format("{0},", i);
            }

            return s;
        }

        public bool Load(string s)
        {
            char[] sep1 = { ',' };

            var tokens = s.Split(sep1, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 7)
            {
                return false;
            }

            Count = Convert.ToInt32(tokens[0]);
            Score = Convert.ToInt32(tokens[1]);
            ScoreType = (StatName)Enum.Parse(typeof(StatName), tokens[2]);

            ActualScore = Convert.ToInt32(tokens[4]);
            ActualScoreType = (StatName)Enum.Parse(typeof(StatName), tokens[5]);

            for (var i = 7; i < tokens.Length; i++)
            {
                Cards.Add(Convert.ToInt32(tokens[i]));
            }

            return true;
        }
    }



    public class Score
    {

        public static string[] ScoreDescription =
       {
            "fifteen", "run", "pair", "three of a kind", "four of a kind",
            "jack of the same suit", "cut a jack", "a run", "hit 31", "hit go", "flush",
            "last card"
        };

        public static string[] PlayerScoreDescription =
        {
            "scored fifteen", "got a run", "scored a pair", "scored three of a kind", "scored four of a kind",
            "have jack of the same suit", "cut a jack", "have a run", "hit 31", "hit go", "have a flush",
            "have last card"
        };

        public static string[] ComputerScoreDescription =
        {
            "scored fifteen", "got a run", "scored a pair", "scored three of a kind", "scored four of a kind",
            "has jack of the same suit", "cut a jack", "has a run", "hit 31", "hit go", "has a flush", "has last card"
        };

        private Score()
        {
        }

        public List<Card> Cards { get; } = new List<Card>();

        public Score(ScoreName scoreName, int value, List<Card> cards)
        {
            ScoreName = scoreName;
            Value = value;
            if (cards != null)
                Cards.AddRange(cards);
        }

        public ScoreName ScoreName { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return string.Format($"{Score.PlayerScoreDescription[(int)ScoreName]} for {Value}");
        }

        public string ToString(PlayerType player)
        {
            if (player == PlayerType.Player)
            {
                return string.Format($"{Score.PlayerScoreDescription[(int)ScoreName]} for {Value}");
            }

            return string.Format($"{Score.ComputerScoreDescription[(int)ScoreName]} for {Value}");
        }
    }
}