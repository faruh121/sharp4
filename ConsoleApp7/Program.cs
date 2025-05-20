using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CombinedGameSolutions
{
    // ===== Код для задания 7 (Автомобильные гонки) =====
    public delegate void RaceStartDelegate();
    public delegate void RaceProgressDelegate(int position);
    public delegate void RaceFinishDelegate(string carName);

    public abstract class Car
    {
        public string Name { get; protected set; }
        public int Speed { get; protected set; }
        public int Position { get; protected set; }
        public Random Random { get; } = new Random();

        public event RaceStartDelegate OnRaceStart;
        public event RaceProgressDelegate OnRaceProgress;
        public event RaceFinishDelegate OnRaceFinish;

        protected Car(string name)
        {
            Name = name;
            Position = 0;
        }

        public virtual void PrepareForRace()
        {
            Console.WriteLine($"{Name} готовится к гонке");
            Speed = Random.Next(5, 20);
        }

        public void StartRace()
        {
            OnRaceStart?.Invoke();

            while (Position < 100)
            {
                Thread.Sleep(200);
                Position += Speed + Random.Next(-3, 4);
                if (Position > 100) Position = 100;

                OnRaceProgress?.Invoke(Position);
            }

            OnRaceFinish?.Invoke(Name);
        }
    }

    public class SportsCar : Car
    {
        public SportsCar(string name) : base(name) { }

        public override void PrepareForRace()
        {
            base.PrepareForRace();
            Console.WriteLine($"{Name} настраивает турбину");
            Speed += 5;
        }
    }

    public class PassengerCar : Car
    {
        public PassengerCar(string name) : base(name) { }
    }

    public class Truck : Car
    {
        public Truck(string name) : base(name) { }

        public override void PrepareForRace()
        {
            base.PrepareForRace();
            Console.WriteLine($"{Name} проверяет груз");
            Speed -= 3;
        }
    }

    public class Bus : Car
    {
        public Bus(string name) : base(name) { }

        public override void PrepareForRace()
        {
            base.PrepareForRace();
            Console.WriteLine($"{Name} закрывает двери");
            Speed -= 2;
        }
    }

    public class RaceGame
    {
        private List<Car> _cars = new List<Car>();
        private bool _raceFinished;

        public void AddCar(Car car)
        {
            car.OnRaceStart += () => Console.WriteLine($"{car.Name} стартовал!");
            car.OnRaceProgress += (pos) => Console.WriteLine($"{car.Name} на позиции {pos}");
            car.OnRaceFinish += (winner) =>
            {
                if (!_raceFinished)
                {
                    Console.WriteLine($"\n{winner} ПЕРВЫЙ НА ФИНИШЕ!");
                    _raceFinished = true;
                }
            };
            _cars.Add(car);
        }

        public void StartRace()
        {
            Console.WriteLine("=== ПОДГОТОВКА К ГОНКЕ ===");
            foreach (var car in _cars)
            {
                car.PrepareForRace();
            }

            Console.WriteLine("\n=== ГОНКА НАЧАЛАСЬ! ===");
            var threads = new List<Thread>();
            foreach (var car in _cars)
            {
                var thread = new Thread(car.StartRace);
                threads.Add(thread);
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
    }

    // ===== Код для задания 8 (Карточная игра) =====
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Six = 6, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }

        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }

        public override string ToString() => $"{Rank} of {Suit}";
    }

    public class Player
    {
        public string Name { get; }
        public Queue<Card> Cards { get; } = new Queue<Card>();

        public Player(string name)
        {
            Name = name;
        }

        public void ShowCards()
        {
            Console.WriteLine($"{Name}'s cards:");
            foreach (var card in Cards)
                Console.WriteLine(card);
        }
    }

    public class CardGame
    {
        private List<Player> _players = new List<Player>();
        private List<Card> _deck = new List<Card>();

        public CardGame(params string[] playerNames)
        {
            if (playerNames.Length < 2)
                throw new ArgumentException("Нужно минимум 2 игрока");

            foreach (var name in playerNames)
                _players.Add(new Player(name));

            InitializeDeck();
            ShuffleDeck();
            DealCards();
        }

        private void InitializeDeck()
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    _deck.Add(new Card(suit, rank));
        }

        private void ShuffleDeck()
        {
            var rnd = new Random();
            _deck = _deck.OrderBy(x => rnd.Next()).ToList();
        }

        private void DealCards()
        {
            int cardsPerPlayer = _deck.Count / _players.Count;

            for (int i = 0; i < _players.Count; i++)
            {
                var playerCards = _deck.Skip(i * cardsPerPlayer).Take(cardsPerPlayer);
                foreach (var card in playerCards)
                    _players[i].Cards.Enqueue(card);
            }
        }

        public void Play()
        {
            Console.WriteLine("=== ИГРА НАЧАЛАСЬ ===");

            while (_players.All(p => p.Cards.Count > 0))
            {
                var playedCards = new List<(Card card, Player player)>();

                foreach (var player in _players)
                {
                    if (player.Cards.Count > 0)
                    {
                        var card = player.Cards.Dequeue();
                        playedCards.Add((card, player));
                        Console.WriteLine($"{player.Name} играет {card}");
                    }
                }

                var winnerCard = playedCards.OrderByDescending(x => x.card.Rank).First();
                Console.WriteLine($"{winnerCard.player.Name} выигрывает раунд!");

                foreach (var (card, _) in playedCards)
                    winnerCard.player.Cards.Enqueue(card);

                var winner = _players.FirstOrDefault(p => p.Cards.Count == _deck.Count);
                if (winner != null)
                {
                    Console.WriteLine($"\n=== {winner.Name} ПОБЕДИЛ В ИГРЕ! ===");
                    return;
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Выберите игру:");
            Console.WriteLine("1. Автомобильные гонки");
            Console.WriteLine("2. Карточная игра");
            Console.Write("Ваш выбор: ");

            var choice = Console.ReadLine();

            if (choice == "1")
            {
                RaceGame game = new RaceGame();
                game.AddCar(new SportsCar("Ferrari"));
                game.AddCar(new PassengerCar("Toyota"));
                game.AddCar(new Truck("Volvo"));
                game.AddCar(new Bus("Ikarus"));
                game.StartRace();
            }
            else if (choice == "2")
            {
                Console.WriteLine("\nВведите имена игроков через запятую (минимум 2):");
                var players = Console.ReadLine()?.Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToArray();

                if (players == null || players.Length < 2)
                {
                    Console.WriteLine("Недостаточно игроков!");
                    return;
                }

                var game = new CardGame(players);
                game.Play();
            }
            else
            {
                Console.WriteLine("Неверный выбор!");
            }
        }
    }
}