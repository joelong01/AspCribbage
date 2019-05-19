
using Cribbage;
using CribbageModels;
using CribbagePlayers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspCribbage.Controllers
{

    [Route("cribbage/")]
    [ApiController]
    public class CribbageController : ControllerBase
    {
        private Deck _deck = new Deck();

        // GET: cribbage/help
        [HttpGet("help")]
        [ActionName("GetHelp")]
        public ActionResult<string> GetHelp()
        {
            return "help url: www.google.com";
        }


        // GET: cribbage/suits
        [HttpGet("suits")]
        [ActionName("GetSuits")]
        public ActionResult<Suit[]> GetSuits()
        {
            return (Suit[])Enum.GetValues(typeof(Suit));
        }

        // GET: cribbage/suits
        [HttpGet("ordinals")]
        [ActionName("GetOrdinals")]
        public ActionResult<CardOrdinal[]> GetOrdinals()
        {
            return (CardOrdinal[])Enum.GetValues(typeof(CardOrdinal));
        }

        // GET: api/Card
        [HttpGet("allcards")]
        [ActionName("GetDeck")]
        public ActionResult<List<Card>> GetDeck()
        {
            return _deck.Cards;
        }


        // GET: api/Card/AceOfClubs
        [HttpGet("card/{name}")]
        [ActionName("GetCard")]
        public ActionResult<Card> GetCard(string name)
        {
            CardName cardName = (CardName)Enum.Parse(typeof(CardName), name);
            return Card.CardNameToCard(cardName);
        }


        //
        //  cut the cards to see who goes first
        //
        //  sample URLs:
        //             https://localhost:44338/cribbage/cutcards
        //
        //  returns: the two cut cards 
        //
        [HttpGet("cutcards")]
        [ActionName("GetCutCards")]
        public async Task<ActionResult<CutCardsResponse>> GetCutCardsAsync()
        {

            string url = "https://www.random.org/sequences/?min=0&max=51&col=1&format=plain&rnd=new";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string randomInts = await response.Content.ReadAsStringAsync();
                string[] values = randomInts.Split("\n", StringSplitOptions.RemoveEmptyEntries);

                Card playerCard = Card.CardNameToCard((CardName)int.Parse(values[0]));
                playerCard.Owner = Owner.Computer;

                Card computerCard = Card.CardNameToCard((CardName)int.Parse(values[1]));
                computerCard.Owner = Owner.Player;


                string repeatUrl = $"cribbage/cutcards/{playerCard.CardName},{computerCard.CardName}";
                return new CutCardsResponse(playerCard, computerCard, repeatUrl);

            }

            return StatusCode(503, "Unable to call random.org");
        }

        //
        //  cut the cards to see who goes first, but takes in the repeat URL to return the same response as the original call
        //
        //  sample URLs:
        //             https://localhost:44338/cribbage/cutcards/AceOfClubs,KingOfSpades
        //
        //  returns: the two cut cards 
        //
        [HttpGet("cutcards/{cardCSV}")]
        [ActionName("GetCutCards")]
        public ActionResult<CutCardsResponse> GetCutCards(string cardCSV)
        {
            (bool success, List<Card> cards, string badCard) = ParseCards(cardCSV);
            if (!success)
            {
                return NotFound($"Bad Card in hand: {badCard}");
            }

            cards[0].Owner = Owner.Computer;
            cards[1].Owner = Owner.Player;
            string repeatUrl = $"cribbage/cutcards/{cards[0].CardName},{cards[1].CardName}";
            return new CutCardsResponse(cards[0], cards[1], repeatUrl);
        }

        //
        //  cut the cards to see who goes first - pass in the random numbers that you get the same result
        //  the last time the api was called. useful for testing.
        //
        //  sample URLs:
        //              https://localhost:44338/cribbage/cutcards/AceOfHearts/KingOfSpades
        //
        //  returns: the two cut cards 
        //
        [HttpGet("cutcards/{card1}/{card2}")]
        [ActionName("GetCutCards")]
        public ActionResult<CutCardsResponse> GetCutCards(string card1, string card2)
        {
            List<Card> cards = new List<Card>();

            bool ret1 = Enum.TryParse<CardName>(card1, true, out CardName name1);
            bool ret2 = Enum.TryParse<CardName>(card2, true, out CardName name2);
            if (!ret1 && !ret2)
            {

                return NotFound($"Bad Card Names: {card1}, {card2}");
            }
            if (!ret1)
            {
                return NotFound($"Bad Card: {card1}");
            }
            if (!ret2)
            {
                return NotFound($"Bad Card: {card2}");
            }
            cards.Add(Card.CardNameToCard(name1));
            cards.Add(Card.CardNameToCard(name2));
            cards[0].Owner = Owner.Computer;
            cards[1].Owner = Owner.Player;
            string repeatUrl = $"cribbage/cutcards/{cards[0].CardName},{cards[1].CardName}";
            return new CutCardsResponse(cards[0], cards[1], repeatUrl);


        }

        //
        //  score the hand (or crib)
        //
        //  sample URLs:
        //              https://localhost:44338/cribbage/scorehand/FiveOfHearts,FiveOfClubs,FiveOfSpades,JackOfDiamonds/FourOfDiamonds/false
        //              https://localhost:44338/cribbage/scorehand/FiveOfHearts,SixOfHearts,SevenOfHearts,EightOfHearts/NineOfDiamonds/false  (should be a flush)
        //              https://localhost:44338/cribbage/scorehand/FiveOfHearts,SixOfHearts,SevenOfHearts,EightOfHearts/NineOfDiamonds/true   (no flush - need 5 of same suit in crib)
        //              https://localhost:44338/cribbage/scorehand/FiveOfHearts,SixOfHearts,SevenOfHearts,EightOfHearts/NineOfHearts/true     (should be a flush)
        //              https://localhost:44338/cribbage/scorehand/FiveOfHearts,SixOfHearts,FourOfHearts,FourOFClubs/SixOfDiamonds/true     (bad card)
        //              https://localhost:44338/cribbage/scorehand/FiveOfHearts,SixOfHearts,FourOfHearts,FourOfClubs/SixOfDiamonds/true     (double double run with 15s - 24 points)
        //              https://localhost:44338/cribbage/scorehand/ThreeOfSpades,TwoOfSpades,QueenOfHearts,QueenOfClubs/AceOfHearts/false
        //
        [HttpGet("scorehand/{handCSV}/{sharedCardName}/{isCrib}")]
        [ActionName("ScoreHand")]
        public ActionResult<ScoreResponse> ScoreHand(string handCSV, string sharedCardName, bool isCrib)
        {
            (bool success, List<Card> hand, string badCard) = ParseCards(handCSV);
            if (!success)
            {
                return NotFound($"Bad Card in hand: {badCard}");
            }

            if (!Enum.TryParse<CardName>(sharedCardName, true, out CardName sharedCard))
            {
                return NotFound($"Invalid Shared Card Name: {sharedCardName}");
            }

            int score = CardScoring.ScoreHand(hand, Card.CardNameToCard(sharedCard), isCrib ? HandType.Crib : HandType.Hand, out List<Score> scoreList);

            return new ScoreResponse(score, scoreList);

        }
        //
        //
        //  URL example:
        //                 https://localhost:44338/cribbage/scorecountedcards/AceOfSpades/0
        //    
        [HttpGet("scorecountedcards/{playedCard}/{currentCount}")]
        [ActionName("ScoreCountedCard")]
        public ActionResult<ScoreResponse> ScoreCountedCard(string playedCard, int currentCount)
        {
            //
            //  if there are no counted cards, there can be no score. this is here for completeness
            //  and is called only when the first card is played.  this can easily be optimized away
            return new ScoreResponse(0, null);
        }

        //
        //  URL examples:
        //                 https://localhost:44338/cribbage/scorecountedcards/AceOfHearts/1/AceOfSpades
        //                 https://localhost:44338/cribbage/scorecountedcards/AceOfClubs/2/AceOfHearts,AceOfSpades
        //                 https://localhost:44338/cribbage/scorecountedcards/TwoOfClubs/13/AceOfHearts,ThreeOfClubs,FiveOfDiamonds,FourOfClubs
        //
        //  Note: this is a GET for /scorecountedcards just like above, it just has one more parameter 
        //
        [HttpGet("scorecountedcards/{playedCardName}/{currentCount}/{countedCardsCSV}")]
        [ActionName("ScoreCountedCard")]
        public ActionResult<ScoreResponse> ScoreCountedCard(string playedCardName, int currentCount, string countedCardsCSV)
        {
            (bool success, List<Card> countedCards, string badCard) = ParseCards(countedCardsCSV);
            if (!success)
            {
                return NotFound($"Bad Card in counted cards: {badCard}");
            }

            ActionResult<Card> result = GetCard(playedCardName);
            Card playedCard = result.Value;
            int tempCount = 0;
            for (int i = 0; i < countedCards.Count; i++)
            {
                tempCount += countedCards[i].Value;
                if (tempCount > 31)
                {
                    tempCount = countedCards[i].Value;
                }
            }

            if (tempCount != currentCount)
            {
                return BadRequest($"Count should be {tempCount} not {currentCount}");
            }

            int score = CardScoring.ScoreCountingCardsPlayed(countedCards, playedCard, tempCount, out List<Score> scoreList);

            return new ScoreResponse(score, scoreList);
        }

        //
        //  given 6 cards, return 2.  if isMyCrib is true, then optimize to make the hand + crib as big as possible
        //
        //  sample URLs:
        //                  https://localhost:44338/cribbage/getcribcards/FiveOfHearts,FiveOfClubs,FiveOfSpades,JackOfDiamonds,SixOfClubs,FourOfDiamonds/false  
        //                  https://localhost:44338/cribbage/getcribcards/FiveOfHearts,FiveOfClubs,FiveOfSpades,JackOfDiamonds,SixOfClubs,FourOfDiamonds/true   
        //                  https://localhost:44338/cribbage/getcribcards/FourOfHearts,FiveOfHearts,SixOfSpades,JackOfHearts,QueenOfHearts,SixOfDiamonds/true  
        //                  https://localhost:44338/cribbage/getcribcards/FourOfHearts,FiveOfHearts,SixOfSpades,JackOfHearts,QueenOfHearts,SixOfDiamonds/false  
        //
        //
        [HttpGet("getcribcards/{handCSV}/{isMyCrib}")]
        [ActionName("GetCribCards")]
        public async Task<ActionResult<List<Card>>> GetCribCardsAsync(string handCSV, bool isMyCrib)
        {
            (bool success, List<Card> hand, string badCard) = ParseCards(handCSV);
            if (!success)
            {
                return NotFound($"Bad Card in hand: {badCard}");
            }

            if (hand.Count != 6)
            {
                return BadRequest($"Wrong number of cards.  Need 6, got {hand.Count}");
            }


            CountingPlayer player = new CountingPlayer(true);
            List<Card> cards = await player.SelectCribCards(hand, isMyCrib);
            return cards;
        }
        [HttpGet("getnextcountedcard/{cardsLeftCSV}/{currentCount}")]
        [ActionName("GetNextCountedCard")]
        public async Task<ActionResult<CountedCardResponse>> GetNextCountedCardAsync(string cardsLeftCSV, int currentCount)
        {
            return await GetNextCountedCardAsync(cardsLeftCSV, currentCount, "");
        }

        //
        //  URL example:
        //                 https://localhost:44338/cribbage/getnextcountedcard/AceOfSpades,AceOfHearts,TwoOfClubs,TenOfDiamonds/0
        //                 https://localhost:44338/cribbage/getnextcountedcard/FiveOfClubs,QueenOfDiamonds/25/ThreeOfDiamonds,TenOfClubs,TwoOfSpades,QueenOfSpades
        //
        //  Note that the last parameters contains all the cards that have already been counted, which means it starts empty, so there are two routes.
        //  I trim spaces, but Cards must be spelled correctly
        //    
        [HttpGet("getnextcountedcard/{cardsLeftCSV}/{currentCount}/{cardsPlayedCSV}")]
        [ActionName("GetNextCountedCard")]
        public async Task<ActionResult<CountedCardResponse>> GetNextCountedCardAsync(string cardsLeftCSV, int currentCount, string cardsPlayedCSV)
        {
            (bool success, List<Card> cardsLeft, string badCard) = ParseCards(cardsLeftCSV);
            if (!success)
            {
                return NotFound($"Bad Card in played cards: {badCard}");
            }
            List<Card> cardsPlayed = null;
            if (cardsPlayedCSV != "")
            {
                (success, cardsPlayed, badCard) = ParseCards(cardsPlayedCSV);
                if (!success)
                {
                    return NotFound($"Bad Card in played cards: {badCard}");
                }
            }

            CountingPlayer player = new CountingPlayer(true);
            Card toPlay = await player.GetCountCard(cardsPlayed, cardsLeft, currentCount);
            int score = CardScoring.ScoreCountingCardsPlayed(cardsPlayed, toPlay, currentCount, out List<Score> scoreList);
            return new CountedCardResponse(toPlay, score, scoreList);

        }

        //
        //
        //  returns a randomized hand of 13 cards.  calls random.org to get the random numbers.
        //  uses the background radiation of the universe.  go figure. 
        //  
        //
        //  URL examples:
        //                 https://localhost:44338/cribbage/getrandomhand/true
        //
        [HttpGet("getrandomhand/{isComputerCrib}")]
        [ActionName("GetRandomHandAsync")]
        public async Task<ActionResult<GetRandomHandResponse>> GetRandomHandAsync(bool isComputerCrib)
        {
            string url = "https://www.random.org/sequences/?min=0&max=51&col=1&format=plain&rnd=new";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string randomInts = await response.Content.ReadAsStringAsync();
                string[] values = randomInts.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                int[] sequence = Array.ConvertAll(values, int.Parse);
                return await GetRandomHandAsync(sequence, isComputerCrib);

            }

            return StatusCode(503, "Unable to call random.org");
        }

        //
        //
        //  this passes back in a sequence that was probably returned by the other getrandomhand call so we 
        //  can have repeatable results and debug when a hand is returned wrong.
        //  
        //
        //  URL examples:
        //                https://localhost:44338/cribbage/getrandomhand/true/46,17,10,35,43,44,1,38,14,3,7,50,19,
        //
        [HttpGet("getrandomhand/{isComputerCrib}/{sequenceCSV}")]
        [ActionName("GetRandomHandAsync")]
        public async Task<ActionResult<GetRandomHandResponse>> GetRandomHandAsync(bool isComputerCrib, string sequenceCSV)
        {
            string[] values = sequenceCSV.Split(",", StringSplitOptions.RemoveEmptyEntries);
            int[] sequence = Array.ConvertAll(values, int.Parse);
            if (sequence.Length != 13)
            {
                return BadRequest($"Need 13 numbers in the seqence and got {sequence.Length}");
            }
            return await GetRandomHandAsync(sequence, isComputerCrib);
        }
        private async Task<GetRandomHandResponse> GetRandomHandAsync(int[] randomSequence, bool isComputerCrib)
        {

            List<Card> playerCards = new List<Card>();
            List<Card> computerCards = new List<Card>();
            string csv = "";
            for (int i = 0; i < 6; i++)
            {
                Card card = _deck.Cards[randomSequence[i]];
                card.Owner = Owner.Player;
                playerCards.Add(card);
                csv += $"{randomSequence[i]},";
            }
            for (int i = 6; i < 12; i++)
            {
                Card card = _deck.Cards[randomSequence[i]];
                card.Owner = Owner.Computer;
                computerCards.Add(card);
                csv += $"{randomSequence[i]},";
            }
            CountingPlayer player = new CountingPlayer(true);
            List<Card> crib = await player.SelectCribCards(computerCards, isComputerCrib);

            Card sharedCard = _deck.Cards[randomSequence[12]];
            csv += $"{randomSequence[12]},";

            string repeatUrl = $"cribbage/getrandomhand/{isComputerCrib}/{csv}";

            return new GetRandomHandResponse(playerCards, computerCards, crib, sharedCard, ((int)(sharedCard.Ordinal) == 11), repeatUrl);

        }


        private (bool success, List<Card> hand, string badCard) ParseCards(string cardsCsv)
        {
            string[] cardNames = cardsCsv.Split(",", StringSplitOptions.RemoveEmptyEntries);
            List<Card> hand = new List<Card>();
            foreach (string name in cardNames)
            {
                if (Enum.TryParse<CardName>(name, true, out CardName cardName))
                {
                    hand.Add(Card.CardNameToCard(cardName));
                }
                else
                {
                    return (false, null, name);
                }
            }


            return (true, hand, "");
        }

        private string CardListToCsv(List<Card> list)
        {
            string ret = "";
            foreach (Card card in list)
            {
                ret += $"{card},";
            }

            return ret;
        }
    }
}
