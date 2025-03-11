using System.Diagnostics;
using Spectre.Console;

namespace Produttore_e_consumatore
{
    internal class Program
    {
        public static readonly long StartTimeStamp = Stopwatch.GetTimestamp();

        public static SemaphoreSlim mySemaphore = new SemaphoreSlim(0);

        public static List<LogEntry> logs = new List<LogEntry>();

        static int countThreadWaiting = 0;

        static int queueSize = 6;

        public static int nSeconds = 1;

        public static Producer producer = new Producer();

        public static Consumer consumer = new Consumer();

        public static LimitedQueue<int> lQueue = new LimitedQueue<int>(queueSize);

        public static void AddLog(string role, string message, int threadCount, int queueElements, string color)
        {
            lock (logs)  // Protezione contro race condition
            {
                logs.Add(new LogEntry(role, message, threadCount, queueElements, color));
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Thread threadProducer = new Thread(ProduceNumber);
                Thread threadConsumer = new Thread(ConsumeNumber);

                threadProducer.Start();
                threadConsumer.Start();

                threadConsumer.Join();
                threadProducer.Join();

                Console.WriteLine(" ");

                PrintLogsTable();

                Console.WriteLine("produttore: il totale dei numeri prodotti è: " + producer.GetCount());
                Console.WriteLine("produttore: sono stati prodotti : " + producer.GetCountNumberGenerator() + " numeri");

                Console.WriteLine("consumatore: il totale dei numeri consumati è: " + consumer.GetCount());
                Console.WriteLine("consumatore: sono stati consumati : " + consumer.GetCountNumberGenerator() + " numeri");

                var remainResultQueue = lQueue.GetCount();
                Console.WriteLine("coda: il totale dei numeri rimasti nella coda è: " + remainResultQueue.Count);
                Console.WriteLine("coda: sono rimasti : " + remainResultQueue.CountNumberGenerator + " numeri");


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                PrintLogsTable();
            }

            Console.ReadKey();

        }

        public static void PrintLogsTable()
        {
            Console.WriteLine(" ");

            var table = new Table();

            table.Border = TableBorder.Rounded;
            table.AddColumn(new TableColumn("[bold]Produttore/Consumatore[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Messaggio[/]").Centered());
            table.AddColumn(new TableColumn("[bold]N° Elementi[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Thread Addormentati[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Time[/]").Centered());

            foreach (var log in logs)
            {
                table.AddRow(
                    $"[{log.Color}]{log.Role}[/]",
                    log.Message,
                    log.QueueElements.ToString() + " / 6",
                    log.ThreadCount.ToString(),
                    log.Ticks.ToString()  
                );
            }

            AnsiConsole.Write(table);
        }

        public static void ProduceNumber()
        {
            DateTime start = DateTime.Now;
            int countLoop = 0;
            while (DateTime.Now < start.AddSeconds(nSeconds))
            {
                Console.Write("p");
                AddLog("Produttore", $"Sto producendo numeri (loop {countLoop})", countThreadWaiting, lQueue.Count, "green");
                countLoop++;
                try
                {
                    if ((lQueue.Count) >= queueSize)
                    {
                        AddLog("Produttore", $"Mi addormento", countThreadWaiting, lQueue.Count, "green");
                        countThreadWaiting++;
                        mySemaphore.Wait();
                        AddLog("Produttore", $"Mi risveglio", countThreadWaiting, lQueue.Count, "green");
                    }
                    else
                    {
                        AddLog("Produttore", $"Ci sono spazi nella coda per me", countThreadWaiting, lQueue.Count, "green");
                    }

                    var numberProduct = producer.GiveMeNumber();
                    AddLog("Produttore", $"Ho prodotto il numero: {numberProduct}", Program.countThreadWaiting, Program.lQueue.Count, "green");
                    lQueue.Enqueue(numberProduct);

                    if (countThreadWaiting > 0)
                    {
                        AddLog("Produttore", "Sto svegliando il consumatore", countThreadWaiting, lQueue.Count, "green");
                        countThreadWaiting--;
                        mySemaphore.Release();
                    }

                    //Thread.Sleep(150);
                }
                catch (Exception e)
                {
                    AddLog("Produttore", $"Errore: {e.Message}", countThreadWaiting, lQueue.Count, "red");
                    Program.PrintLogsTable();
                    Debugger.Break();
                }
            }

            AddLog("Produttore", $"Ho fatto {nSeconds} secondi e finito di produrre numeri", countThreadWaiting, lQueue.Count, "green");
        }

        public static void ConsumeNumber()
        {
            DateTime start = DateTime.Now;
            var countLoop = 0;
            while (DateTime.Now < start.AddSeconds(nSeconds))
            {
                AddLog("Consumatore", $"Sto consumando dati (loop {countLoop})", countThreadWaiting, lQueue.Count, "yellow");
                countLoop++;
                try
                {
                    Console.Write("c");
                    if ((lQueue.Count) == 0)
                    {
                        AddLog("Consumatore", $"Mi addormento", countThreadWaiting, lQueue.Count, "yellow");
                        countThreadWaiting++;
                        mySemaphore.Wait();
                        AddLog("Consumatore", $"Mi risveglio", countThreadWaiting, lQueue.Count, "yellow");
                    }
                    else
                    {
                        AddLog("Consumatore", $"Ci sono elementi nella coda per me", countThreadWaiting, lQueue.Count, "yellow");
                    }

                    var number = lQueue.Dequeue();
                    Program.AddLog("Consumatore", $"Ho consumato il numero: {number}", Program.countThreadWaiting, Program.lQueue.Count, "yellow");
                    consumer.ConsumeNumber(number);

                    if (countThreadWaiting > 0)
                    {
                        AddLog("Consumatore", "Sto svegliando il produttore", countThreadWaiting, lQueue.Count, "yellow");
                        countThreadWaiting--;
                        mySemaphore.Release();
                    }

                    //Thread.Sleep(150);
                }
                catch (Exception e)
                {
                    AddLog("Consumatore", $"Errore: {e.Message}", countThreadWaiting, lQueue.Count, "red");
                    Program.PrintLogsTable();
                    Debugger.Break();
                }
            }

            AddLog("Consumatore", $"Ho fatto {nSeconds} secondi e finito di consumare dati", countThreadWaiting, lQueue.Count, "yellow");
        }

    }

    #region ProducerConsumer

    class Producer
    {
        private Random _random = new Random(772);
        int _count = 0;
        int _countNumberGenerator = 0;
        public int GiveMeNumber()
        {
            int i = _random.Next(100);            
            _count = _count + i;
            _countNumberGenerator++;
            return i;
        }
        public int GetCount()
        {
            return _count;
        }
        public int GetCountNumberGenerator()
        {
            return _countNumberGenerator;
        }
    }

    class Consumer
    {
        private List<int> _numbers = new List<int>();
        int _countConsumer = 0;
        int _countNumberConsumer = 0;
        public void ConsumeNumber(int number)
        {
            _numbers.Add(number);            
            _countConsumer += number;
            _countNumberConsumer++;
        }
        public int GetCount()
        {
            return _countConsumer;
        }
        public int GetCountNumberGenerator()
        {
            return _countNumberConsumer;
        }
    }
    class LimitedQueueCountResult
    {
        public int Count { get; set; }
        public int CountNumberGenerator { get; set; }

    }
    class LimitedQueue<T> : Queue<int>
    {
        private int _maxSize;

        public LimitedQueueCountResult GetCount()
        {
            LimitedQueueCountResult result = new LimitedQueueCountResult();
            while (this.Count != 0)
            {
                var isSuccess = this.TryDequeue(out int t);
                result.CountNumberGenerator++;
                result.Count += t;
            }
            return result;
        }
        public LimitedQueue(int maxSize)
        {
            _maxSize = maxSize;
        }

        public new void Enqueue(int item)
        {
            if (this.Count >= _maxSize)
            {
                throw new InvalidOperationException("Queue is full");
            }
            base.Enqueue(item);
        }
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; } // Data e ora del log
        public string Role { get; set; } // "Produttore" o "Consumatore"
        public string Message { get; set; } // Il messaggio da mostrare
        public int ThreadCount { get; set; } // Numero di thread addormentati
        public int QueueElements { get; set; } // Elementi nella coda
        public string Color { get; set; } // Colore del messaggio
        public long Ticks { get; set; }

        public LogEntry(string role, string message, int threadCount, int queueElements, string color)
        {
            Ticks = Stopwatch.GetTimestamp() - Program.StartTimeStamp; // Tempo relativo in tick
            Timestamp = DateTime.UtcNow;  // Usa UTC per coerenza
            Role = role;
            Message = message;
            ThreadCount = threadCount;
            QueueElements = queueElements;
            Color = color;
        }
    }

    #endregion
}
