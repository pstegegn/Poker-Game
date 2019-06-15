using System;
using System.Collections.Generic;

namespace Poker
{
    public enum Rank {_2 = 2, _3, _4, _5, _6, _7, _8, _9, _10, _J, _Q, _K, _A };
    public enum Suit { C=0, D, H, S };
    public enum  PokerHand { Unknown=0, HighCard, OnePair, ThreeOfAKind, Flush };

    class Card : IComparable
    {
        private Suit _suit;
        private Rank _rank;
        public Suit Suit { get {return _suit; } set {_suit = value; } }
        public Rank Rank { get {return _rank; } set {_rank = value; } }

        public Card(Suit suit, Rank rank){
            this._suit = suit;
            this._rank = rank;
        }

        public int CompareTo(object obj)
        {
            Card mc = (Card)obj;

            if (this.Rank < mc.Rank)
                return -1;

            if (this.Rank > mc.Rank)
                return 1;

            return 0;
        }
    }

    struct HAND
    {
        public PokerHand pokerHand;
        public Rank threeOfAKindRank;
        public Card[] threeOfAKind2cards;
        public Rank onePairRank;
        public Card[] OnePair3cards;
    }

    class Player
    {
        private string _name;
        private List<Card> _cards;
        private int _numCards = 5;

        public string Name { get {return _name; } set {_name = value; } }
        public List<Card> Cards {
            get {
            return _cards; 
            }
            set {
                _cards = value;
                _cards.Sort();
            } 
        }

        public int NumCards { get { return _numCards; } set { _numCards = value; } }

        public Player(string name, List<Card> cards)
        {
            _name = name;
            _cards = cards;
            _numCards = cards.Count;
            _cards.Sort();
        }

        public Player()
        {
        }

        public void display() {
            System.Console.Write("Name = {0}, Card = ", _name);
            foreach (Card card in _cards) {
                System.Console.Write("{0}{1} ", card.Rank, card.Suit);
            }
            System.Console.WriteLine("");
        }

        //determine poker hand 
        public HAND determinePokerHand()
        {
            HAND ahand = new HAND();

            if (isFlush()) { ahand.pokerHand = PokerHand.Flush; }
            else if (isThreeOfAKind(ref ahand)) { }
            else if (isOnePair(ref ahand)) { }
            else { ahand.pokerHand = PokerHand.HighCard; }

            return ahand;
        }

        private bool isOnePair(ref HAND hand)
        {
            Rank mOnePairRank = 0;
            bool returnStatus = false;
            for (int i = 0; i < _cards.Count - 1; i++)
            {
                Rank mRank = _cards[i].Rank;
                int rankCount = 1;
                for (int j = i + 1; j < _cards.Count; j++)
                {
                    if (mRank == _cards[j].Rank) rankCount++;
                    if (rankCount == 2) break;
                }

                if (rankCount == 2)
                {
                    hand.OnePair3cards = new Card[3];

                    mOnePairRank = mRank;
                    hand.pokerHand = PokerHand.OnePair;
                    hand.onePairRank = mRank;
                    int z = 0;
                    foreach (var card in _cards)
                    {
                        if (card.Rank != mRank && (z < 3))
                        {
                            hand.OnePair3cards[z] = card;
                            z++;
                        }
                    }

                    Array.Sort(hand.OnePair3cards);

                    returnStatus = true;
                }

            }
            return returnStatus;
        }

        private bool isThreeOfAKind(ref HAND hand)
        {
            for (int i = 0; i < _cards.Count - 2; i++)
            {
                Rank mRank = _cards[i].Rank;
                int rankCount = 1;
                for (int j = i + 1; j < _cards.Count; j++)
                {
                    if (mRank == _cards[j].Rank) rankCount++;
                    if (rankCount == 3) break;
                }

                if (rankCount == 3) {
                    hand.threeOfAKind2cards = new Card[2];

                    hand.pokerHand = PokerHand.ThreeOfAKind;
                    hand.threeOfAKindRank = mRank;
                    int z = 0;
                    foreach (var card in _cards)
                    {
                        if (card.Rank != mRank && (z < 2))
                        {
                            hand.threeOfAKind2cards[z] = card;
                            z++;
                        }
                    }
                    Array.Sort(hand.threeOfAKind2cards);

                    return true;
                }

            }
            return false;
        }

        private bool isFlush()
        {
            Suit suit = _cards[0].Suit;
            foreach (var card in _cards)
            {
                if (card.Suit != suit) return false;
            }
            return true;
        }
    }

    class PokerGame
    {
        private List <Player> _players;
        private int _numPlayers;
        private int _numCard = 5;

        public int NumCard { get { return _numCard; } set { _numCard = value; } }
        public int NumPlayers { get { return _numPlayers; } set { _numPlayers = value; } }

        public PokerGame(List<Player> players, int numCard)
        {
            _players = players;
            _numPlayers = _players.Count;
            _numCard = numCard;
        }

        // Determine winner players
        public List<Player> evaluate()
        {
            List<HAND> winnersHand = new List<HAND>();
            List<Player> winners = new List<Player>();

            //determine winners by only checking PokerHand
            foreach(var player in _players) {
                HAND tempHand = player.determinePokerHand();

                if(winners.Count == 0 || (winnersHand[0].pokerHand == tempHand.pokerHand)) {
                    winnersHand.Insert(0, tempHand);
                    winners.Insert(0, player);
                } else if(winnersHand[0].pokerHand < tempHand.pokerHand)
                {
                    winners.Clear();
                    winnersHand.Clear();
                    winnersHand.Add(tempHand);
                    winners.Add(player);
                }   
            }

            //Tie breaker
            switch (winnersHand[0].pokerHand) {
                case PokerHand.Flush: 
                case PokerHand.HighCard:
                    {
                        // going from highest rank card, remove players that have 
                        // lower rank in each level until remaining winner is left
                        for (int i = _numCard-1; i >= 0; i--)
                        {
                            if (winners.Count == 1) break;

                            Player winnerInitial = winners[0];
                            List<Player> playersToBeRemoved = new List<Player>();
                            foreach (var awinner in winners)
                            {
                                if (awinner.Cards[i].Rank <
                                winnerInitial.Cards[i].Rank)
                                {
                                    playersToBeRemoved.Add(awinner);
                                }

                                if (awinner.Cards[i].Rank >
                                winnerInitial.Cards[i].Rank)
                                {
                                    playersToBeRemoved.Add(winnerInitial);
                                    winnerInitial = awinner;
                                }
                            }

                            foreach (var awinner in playersToBeRemoved)
                                winners.Remove(awinner);
                        }
                    }
                    break;
                case PokerHand.ThreeOfAKind:
                    {
                        Player winner = winners[0];
                        List<Player> playersToBeRemoved2 = new List<Player>();

                        // Remove players that have less rank of the three of a kind card
                        foreach (var awinner in winners)
                        {
                            if (awinner.determinePokerHand().threeOfAKindRank <
                            winner.determinePokerHand().threeOfAKindRank)
                            {
                                playersToBeRemoved2.Add(awinner);
                            }

                            if (winner.determinePokerHand().threeOfAKindRank <
                            awinner.determinePokerHand().threeOfAKindRank)
                            {
                                playersToBeRemoved2.Add(winner);
                                winner = awinner;
                            }
                        }

                        foreach (var awinner in playersToBeRemoved2)
                            winners.Remove(awinner);

                        // going from highest rank in the two kicker cards, 
                        // remove players that have lower rank in each level 
                        // until remaining winner is left
                        for (int i = 1; i >= 0; i--)
                        {
                            if (winners.Count == 1) break;

                            Player winnerInitial = winners[0];
                            List<Player> playersToBeRemoved = new List<Player>();
                            foreach (var awinner in winners)
                            {
                                if(awinner.determinePokerHand().threeOfAKind2cards.Length != 2 ||
                                   winnerInitial.determinePokerHand().threeOfAKind2cards.Length != 2 ) {
                                    throw new Exception();
                                    }

                                if (awinner.determinePokerHand().threeOfAKind2cards[i].Rank <
                                winnerInitial.determinePokerHand().threeOfAKind2cards[i].Rank)
                                {
                                    playersToBeRemoved.Add(awinner);
                                }

                                if (awinner.determinePokerHand().threeOfAKind2cards[i].Rank >
                                winnerInitial.determinePokerHand().threeOfAKind2cards[i].Rank)
                                {
                                    playersToBeRemoved.Add(winnerInitial);
                                    winnerInitial = awinner;
                                }
                            }

                            foreach (var awinner in playersToBeRemoved)
                                winners.Remove(awinner);
                        }
                    }
                    break;
                case PokerHand.OnePair:
                    {
                        Player winner = winners[0];
                        List<Player> playersToBeRemoved = new List<Player>();
                        foreach (var awinner in winners)
                        {
                            if (awinner.determinePokerHand().onePairRank <
                            winner.determinePokerHand().onePairRank)
                            {
                                playersToBeRemoved.Add(awinner);
                            }

                            if (winner.determinePokerHand().onePairRank <
                            awinner.determinePokerHand().onePairRank)
                            {
                                playersToBeRemoved.Add(winner);
                                winner = awinner;
                            }
                        }

                        foreach (var awinner in playersToBeRemoved)
                            winners.Remove(awinner);

                        // going from highest rank in the three kicker cards, 
                        // remove players that have lower rank in each level 
                        // until remaining winner is left
                        for (int i = 2; i >= 0; i--)
                        {
                            if (winners.Count == 1) break;

                            Player winnerInitial = winners[0];
                            List<Player> playersToBeRemoved2 = new List<Player>();
                            foreach (var awinner in winners)
                            {
                                if (awinner.determinePokerHand().OnePair3cards.Length != 3 ||
                                  winnerInitial.determinePokerHand().OnePair3cards.Length != 3)
                                {
                                    throw new Exception();
                                }
                                if (awinner.determinePokerHand().OnePair3cards[i].Rank <
                                winnerInitial.determinePokerHand().OnePair3cards[i].Rank)
                                {
                                    playersToBeRemoved2.Add(awinner);
                                }

                                if (awinner.determinePokerHand().OnePair3cards[i].Rank >
                                winnerInitial.determinePokerHand().OnePair3cards[i].Rank)
                                {
                                    playersToBeRemoved2.Add(winnerInitial);
                                    winnerInitial = awinner;
                                }
                            }

                            foreach (var awinner in playersToBeRemoved2)
                                winners.Remove(awinner);
                        }
                    }
                    break;
                default:
                    break;
            }

            return winners;
        }

    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            //interactiveTest();
            //randomlyGeneratedTest();
            test1();
            test2();
            test3();
            test4();
        }

        public static void interactiveTest() {
            bool answer;
            do
            {
                int numbPlayer = 0;
                bool loop;
                do
                {
                    Console.Write("Enter Number of players:");
                    try
                    {
                        numbPlayer = Convert.ToInt32(Console.ReadLine());
                        loop = false;
                    }
                    catch
                    {
                        Console.WriteLine("Wrong input please enter number!");
                        loop = true;
                    }
                } while (loop);

                List<Player> players = new List<Player>();

                for (int i = 0; i < numbPlayer; i++)
                {
                    List<Card> cards = new List<Card>();
                    Console.Write("Enter player{0} name:", i + 1);
                    string name = Console.ReadLine();
                    string[] cardsArray;

                    do
                    {
                        Console.WriteLine("Enter {0}'s 5 cards separated by spaces(E.g. 5S 2D 3C JS AH):", name);
                        cardsArray = Console.ReadLine().Split(' ');
                        if (cardsArray.Length != 5)
                            Console.WriteLine("Wrong input please enter number!");
                    } while (cardsArray.Length != 5);

                    foreach (var card in cardsArray)
                    {
                        string suit = card[1].ToString().ToUpper();
                        string rank = "_" + card[0].ToString().ToUpper();
                        Suit aSuit;
                        Rank aRank;
                        Enum.TryParse(suit, true, out aSuit);
                        Enum.TryParse(rank, true, out aRank);
                        cards.Add(new Card(aSuit, aRank));
                    }

                    Player player = new Player(name, cards);
                    player.display();
                    players.Add(player);
                }

                PokerGame pgame = new PokerGame(players, 5);
                List<Player> winners = pgame.evaluate();
                foreach (Player winner in winners)
                    Console.WriteLine("winner={0}, hand={1}", winner.Name, winner.determinePokerHand().pokerHand);

                Console.WriteLine("Do you want to play again? (Y/N)");
                string temp = Console.ReadLine();
                answer = temp.Equals("N") || temp.Equals("n");
            } while (!answer);
        }

        public static void randomlyGeneratedTest() {
            List<Player> players = new List<Player>();
            Random random = new Random();

            for (int i = 0; i < 10000; i++)
            {
                players.Clear();
                Player player = new Player("Joe", new List<Card>(){
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14))
                                });
                player.display();
                players.Add(player);

                player = new Player("Bob", new List<Card>(){
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14))
                                });
                player.display();
                players.Add(player);

                player = new Player("Sally", new List<Card>(){
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14)),
                                new Card((Suit)random.Next(0,3), (Rank)random.Next(2,14))
                                });
                player.display();
                players.Add(player);

                PokerGame pgame = new PokerGame(players, 5);
                List<Player> winners = pgame.evaluate();
                foreach (Player winner in winners)
                    Console.WriteLine("winner={0}, hand={1}\n", winner.Name, winner.determinePokerHand().pokerHand);
            }

        }

        public static void test1() {
            //game 1
            List<Player> players = new List<Player>();
            players.Add(new Player("Joe", new List<Card>(){
                            new Card(Suit.S, Rank._8),
                            new Card(Suit.D, Rank._8),
                            new Card(Suit.D, Rank._A),
                            new Card(Suit.D, Rank._Q),
                            new Card(Suit.H, Rank._J) }));

            players.Add(new Player("Bob",
                new List<Card>(){
                            new Card(Suit.S, Rank._A),
                            new Card(Suit.S, Rank._Q),
                            new Card(Suit.S, Rank._8),
                            new Card(Suit.S, Rank._6),
                            new Card(Suit.S, Rank._4)
                    }));

            players.Add(new Player("Sally",
            new List<Card>(){
                    new Card(Suit.S, Rank._4),
                    new Card(Suit.H, Rank._4),
                    new Card(Suit.H, Rank._3),
                    new Card(Suit.C, Rank._Q),
                    new Card(Suit.C, Rank._8)
            }));

            foreach (Player pl in players) pl.display(); 

            PokerGame pgame = new PokerGame(players, 5);

            List<Player> winners = pgame.evaluate();

            foreach (Player winner in winners)
                Console.WriteLine("winner={0}, hand={1}\n", winner.Name, winner.determinePokerHand().pokerHand);

        }

        public static void test2()
        {
            ////game 2
            List<Player> players = new List<Player>();
            players.Add(new Player("Joe", new List<Card>(){
                                new Card(Suit.S, Rank._Q),
                                new Card(Suit.S, Rank._8),
                                new Card(Suit.S, Rank._K),
                                new Card(Suit.S, Rank._7),
                                new Card(Suit.S, Rank._3)
                                }));

            players.Add(new Player("Bob", new List<Card>(){
                                new Card(Suit.D, Rank._8),
                                new Card(Suit.D, Rank._8),
                                new Card(Suit.D, Rank._A),
                                new Card(Suit.D, Rank._Q),
                                new Card(Suit.D, Rank._J) 
                                }));


            players.Add(new Player("Sally", new List<Card>(){
            new Card(Suit.S, Rank._4),
            new Card(Suit.H, Rank._4),
            new Card(Suit.H, Rank._3),
            new Card(Suit.C, Rank._Q),
            new Card(Suit.C, Rank._8)
            }));

            foreach (Player pl in players) pl.display();

            PokerGame pgame = new PokerGame(players, 5);

            List<Player> winners = pgame.evaluate();

            foreach (Player winner in winners)
                Console.WriteLine("winner={0}, hand={1}\n", winner.Name, winner.determinePokerHand().pokerHand);

        }

        public static void test3()
        {
            //game 3
            List<Player> players = new List<Player>();

            players.Add(new Player("Joe", new List<Card>(){
                                new Card(Suit.H, Rank._3),
                                new Card(Suit.D, Rank._5),
                                new Card(Suit.C, Rank._9),
                                new Card(Suit.D, Rank._9),
                                new Card(Suit.H, Rank._Q)
                                }));

            players.Add(new Player("Jen", new List<Card>(){
                                new Card(Suit.C, Rank._5),
                                new Card(Suit.D, Rank._7),
                                new Card(Suit.H, Rank._9),
                                new Card(Suit.S, Rank._9),
                                new Card(Suit.S, Rank._Q)
                                }));


            players.Add(new Player("Bob", new List<Card>(){
            new Card(Suit.H, Rank._2),
            new Card(Suit.C, Rank._2),
            new Card(Suit.S, Rank._5),
            new Card(Suit.C, Rank._10),
            new Card(Suit.H, Rank._A)
            }));

            foreach (Player pl in players) pl.display();

            PokerGame pgame = new PokerGame(players, 5);

            List<Player> winners = pgame.evaluate();

            foreach (Player winner in winners)
                Console.WriteLine("winner={0}, hand={1} \n", winner.Name, winner.determinePokerHand().pokerHand);

        }

        public static void test4()
        {
            //game 4
            List<Player> players = new List<Player>();

             players.Add(new Player("Joe", new List<Card>(){
                                new Card(Suit.H, Rank._2),
                                new Card(Suit.D, Rank._3),
                                new Card(Suit.C, Rank._4),
                                new Card(Suit.D, Rank._5),
                                new Card(Suit.H, Rank._10)
                                }));

            players.Add(new Player("Jen", new List<Card>(){
                                new Card(Suit.C, Rank._5),
                                new Card(Suit.D, Rank._7),
                                new Card(Suit.H, Rank._8),
                                new Card(Suit.S, Rank._9),
                                new Card(Suit.D, Rank._Q)
                                }));


            players.Add(new Player("Bob", new List<Card>(){
            new Card(Suit.C, Rank._2),
            new Card(Suit.D, Rank._4),
            new Card(Suit.S, Rank._5),
            new Card(Suit.C, Rank._10),
            new Card(Suit.H, Rank._J)
            }));

            foreach (Player pl in players) pl.display();

            PokerGame pgame = new PokerGame(players, 5);

            List<Player> winners = pgame.evaluate();

            foreach (Player winner in winners)
                Console.WriteLine("winner={0}, hand={1}\n", winner.Name, winner.determinePokerHand().pokerHand);

        }

    }
}
