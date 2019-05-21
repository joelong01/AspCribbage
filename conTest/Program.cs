using CribbageModels;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace conTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Debugger.Break();
            
            TestHandScoring test = new TestHandScoring("https://localhost:44338");
            Console.Write("TestGetHand\t");
            await test.TestGetHandAsync();
            Console.Write("TestDoubleRuns\t");
            await test.TestDoubleRuns();
            Console.Write("TestFlush\t");
            await test.TestFlush();
            Console.Write("TestGetHandWithJack\t");
            await test.TestGetHandWithJack();
            Console.Write("TestSimpleRun\t");
            await test.TestSimpleRun();
            Console.Write("TestPairsAsync\t");
            await test.TestPairsAsync();
        }
    }

    public class TestHandScoring
    {
        HttpClient _client = new HttpClient();
        string _urlPrefix = "https://localhost:4438";
        private string BuildUrl(string urlIn)
        {

            string url = $"{_urlPrefix}{urlIn}";
            return url;
        }
        public TestHandScoring(string urlPrefix)
        {
            //string urlPrefix = Environment.GetEnvironmentVariable("ASPCRIBBAGE_URL_PREFIX");
            if (urlPrefix != "")
            {
                _urlPrefix = urlPrefix;
            }

            Console.WriteLine($"Using {_urlPrefix} for URLs ");

        }

        public async Task TestGetHandAsync()
        {
            try
            {
                string url = BuildUrl("/cribbage/getrandomhand/true");
                HttpResponseMessage response = await _client.GetAsync(url);
                Debug.Assert(response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.None,
                        

                    };
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    GetRandomHandResponse hand = JsonConvert.DeserializeObject<GetRandomHandResponse>(jsonResponse, settings);
                    AssertTrue(hand != null, "Error deserializing GetRandomHandResponse");
                    AssertTrue(hand.ComputerCards.Count == 6, "Computer needs 6 cards");
                    AssertTrue(hand.PlayerCards.Count == 6, "Player needs 6 cards");
                    AssertTrue(hand.ComputerCribCards.Count == 2, "Computer needs 2 cards to give to Crib");
                    foreach (var card in hand.ComputerCards)
                    {
                        AssertTrue(card.Owner == Owner.Computer, "Card needs to be owned by computer");
                    }
                    foreach (var card in hand.PlayerCards)
                    {
                        AssertTrue(card.Owner == Owner.Player, "Card needs to be owned by Player");
                    }

                    AssertTrue(hand.HisNobs == (hand.SharedCard.Ordinal == CardOrdinal.Jack), "if you cut a jack, Nobs must be true");
                    
                }
            }
            catch (Exception e)
            {
                OutputError(e.Message);
            }

            OutputInfo("PASSED");
        }

        private void OutputError(string error, [CallerMemberName] string cmb = "", [CallerLineNumber] int cln = 0)
        {
            var bg = Console.BackgroundColor;
            var fg = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }

        private void OutputInfo(string msg, [CallerMemberName] string cmb = "", [CallerLineNumber] int cln = 0)
        {
            var bg = Console.BackgroundColor;
            var fg = Console.ForegroundColor;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{cmb}.{cln}[{msg}]");
            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }

        private void AssertEqual(int a, int b, string msg = "", [CallerMemberName] string cmb = "", [CallerLineNumber] int cln = 0)
        {
            if (a != b)
            {
                OutputError($"Assertion failed [{a}={b}] {cmb}.{cln} [{msg}]");
                throw new Exception(msg);
            }
        }

        private void AssertTrue(bool flag, string msg = "", [CallerMemberName] string cmb = "", [CallerLineNumber] int cln = 0)
        {
            if (!flag)
            {
                OutputError($"Truth ssertion failed {msg} {cmb}.{cln} [{msg}]");
                throw new Exception(msg);
            }
        }

        /// <summary>
        ///     2 pairs and some fifteens.  also has Nibs
        /// </summary>

        public async Task TestPairsAsync()
        {
            string url = BuildUrl("/cribbage/scorehand/FiveOfHearts,FiveOfClubs,FourOfSpades,JackOfDiamonds/FourOfDiamonds/false");
            HttpResponseMessage response = await _client.GetAsync(url);
            Debug.Assert(response.IsSuccessStatusCode);
            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                ScoreResponse scoreRes = JsonConvert.DeserializeObject<ScoreResponse>(jsonResponse);
                AssertEqual(9, scoreRes.TotalScore);
                AssertEqual(5, scoreRes.Scores.Count);
                int nibsCount = 0;
                int pairs = 0;
                int fifteen = 0;
                foreach (var score in scoreRes.Scores)
                {
                    switch (score.ScoreName)
                    {
                        case ScoreName.HisNibs:
                            nibsCount++;
                            AssertEqual(2, score.Cards.Count);
                            break;
                        case ScoreName.Fifteen:
                            fifteen++;
                            AssertEqual(2, score.Cards.Count);
                            break;
                        case ScoreName.Pair:
                            AssertEqual(2, score.Cards.Count);
                            pairs++;
                            break;
                        default:
                            AssertTrue(false, "Unexpected score name in Score");
                            break;
                    }
                }

                AssertEqual(1, nibsCount);
                AssertEqual(2, pairs);
                AssertEqual(2, fifteen);
            }

            OutputInfo("PASSED");
        }

        /// <summary>
        ///     the classic 4,4,5,5,6 hand
        /// </summary>        
        public async Task TestDoubleRuns()
        {
            try
            {
                string url = BuildUrl("/cribbage/scorehand/FiveOfHearts,FiveOfClubs,FourOfSpades,SixOfClubs/FourOfDiamonds/false");
                HttpResponseMessage response = await _client.GetAsync(url);
                AssertTrue(response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    ScoreResponse scoreRes = JsonConvert.DeserializeObject<ScoreResponse>(jsonResponse);
                    AssertEqual(24, scoreRes.TotalScore);
                    AssertEqual(10, scoreRes.Scores.Count);
                    int runs = 0;
                    int pairs = 0;
                    int fifteen = 0;
                    foreach (var score in scoreRes.Scores)
                    {
                        switch (score.ScoreName)
                        {
                            case ScoreName.Run:
                                runs++;
                                AssertEqual(3, score.Cards.Count);
                                break;
                            case ScoreName.Fifteen:
                                fifteen++;
                                AssertEqual(3, score.Cards.Count);
                                break;
                            case ScoreName.Pair:
                                AssertEqual(2, score.Cards.Count);
                                pairs++;
                                break;
                            default:
                                AssertTrue(false, "Unexpected score name in Score");
                                break;
                        }
                    }

                    AssertEqual(4, runs);
                    AssertEqual(2, pairs);
                    AssertEqual(4, fifteen);
                }
            }
            catch (Exception e)
            {
                OutputError("FAILED " + e.Message);
            }

            OutputInfo("PASSED");
        }
        /// <summary>
        ///     this tests a 5 card run that is also a flush and has 1 fifteen 
        /// </summary>
        public async Task TestSimpleRun()
        {
            try
            {
                string url = BuildUrl("/cribbage/scorehand/AceOfClubs,TwoOfClubs,ThreeOfClubs,FiveOfClubs/FourOfClubs/false");
                HttpResponseMessage response = await _client.GetAsync(url);
                AssertTrue(response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    ScoreResponse scoreRes = JsonConvert.DeserializeObject<ScoreResponse>(jsonResponse);
                    AssertEqual(12, scoreRes.TotalScore);
                    AssertEqual(3, scoreRes.Scores.Count);
                    int runs = 0;
                    int flush = 0;
                    int fifteen = 0;
                    foreach (var score in scoreRes.Scores)
                    {
                        switch (score.ScoreName)
                        {
                            case ScoreName.Run:
                                runs++;
                                AssertEqual(5, score.Cards.Count);
                                break;
                            case ScoreName.Fifteen:
                                fifteen++;
                                AssertEqual(5, score.Cards.Count);
                                break;
                            case ScoreName.Flush:
                                AssertEqual(5, score.Cards.Count);
                                flush++;
                                break;
                            default:
                                AssertTrue(false, "Unexpected score name in Score");
                                break;
                        }
                    }

                    AssertEqual(1, runs);
                    AssertEqual(1, flush);
                    AssertEqual(1, fifteen);
                }
            }
            catch (Exception e)
            {
                OutputError("FAILED " + e.Message);
            }
            OutputInfo("PASSED");
        }
        /// <summary>
        ///     this will test 4 and 5 card flushes for both crib and regular hand
        /// </summary>
        public async Task TestFlush()
        {
            try
            {
                string url = BuildUrl("/cribbage/scorehand/AceOfClubs,NineOfClubs,ThreeOfClubs,SevenOfClubs/KingOfSpades/false");
                HttpResponseMessage response = await _client.GetAsync(url);
                AssertTrue(response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    ScoreResponse scoreRes = JsonConvert.DeserializeObject<ScoreResponse>(jsonResponse);
                    AssertEqual(4, scoreRes.TotalScore);
                    AssertEqual(1, scoreRes.Scores.Count);
                    int flush = 0;
                    foreach (var score in scoreRes.Scores)
                    {
                        switch (score.ScoreName)
                        {
                            case ScoreName.Flush:
                                AssertEqual(4, score.Cards.Count);
                                flush++;
                                break;
                            default:
                                AssertTrue(false, "Unexpected score name in Score");
                                break;
                        }
                    }

                    AssertEqual(1, flush);

                    url = BuildUrl("/cribbage/scorehand/AceOfClubs,NineOfClubs,ThreeOfClubs,SevenOfClubs/KingOfSpades/true");
                    response = await _client.GetAsync(url);
                    AssertTrue(response.IsSuccessStatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        jsonResponse = await response.Content.ReadAsStringAsync();
                        scoreRes = JsonConvert.DeserializeObject<ScoreResponse>(jsonResponse);
                        AssertEqual(0, scoreRes.TotalScore);

                    }

                    url = BuildUrl("/cribbage/scorehand/AceOfClubs,NineOfClubs,ThreeOfClubs,SevenOfClubs/KingOfClubs/true");
                    response = await _client.GetAsync(url);
                    AssertTrue(response.IsSuccessStatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        jsonResponse = await response.Content.ReadAsStringAsync();
                        scoreRes = JsonConvert.DeserializeObject<ScoreResponse>(jsonResponse);
                        AssertEqual(5, scoreRes.TotalScore);
                        AssertEqual(1, scoreRes.Scores.Count);
                    }
                }
            }
            catch (Exception e)
            {
                OutputError("FAILED " + e.Message);
            }
            OutputInfo("PASSED");
        }
        /// <summary>
        ///     Makes sure that when a jack is cut, a score comes back
        ///     Also verify that the count of the hands returned is correct
        /// </summary>
        public async Task TestGetHandWithJack()
        {
            try
            {
                string url = BuildUrl("/cribbage/getrandomhand/true/46,17,10,35,43,44,1,38,14,3,7,50,10");
                HttpResponseMessage response = await _client.GetAsync(url);
                AssertTrue(response.IsSuccessStatusCode);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    GetRandomHandResponse handResponse = JsonConvert.DeserializeObject<GetRandomHandResponse>(jsonResponse);
                    AssertEqual(6, handResponse.PlayerCards.Count);
                    AssertEqual(6, handResponse.ComputerCards.Count);
                    AssertEqual(2, handResponse.ComputerCribCards.Count);
                    AssertTrue(handResponse.HisNobs);
                }
            }
            catch (Exception e)
            {
                OutputError("FAILED " + e.Message);
                throw (e);
            }

            OutputInfo("PASSED");
        }
    }
}
