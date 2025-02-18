namespace Produttore_e_consumatore
{    
    internal class Program
    {
        static Semaphore consumerSemaphore = new Semaphore(7, 7);
        
        static Producer producer = new Producer();
        
        static Consumer consumer = new Consumer();

        public static LimitedQueue<int> lQueue = new LimitedQueue<int>(6);

        static void Main(string[] args)
        {            
            Thread producer = new Thread(ProduceNumber);
            Thread consumer = new Thread(ConsumeNumber);

            producer.Start();
            consumer.Start();

            producer.Join();
            consumer.Join();

            Console.ReadKey();
        }

        public static void ProduceNumber()
        {
            DateTime start = DateTime.Now;
            while (DateTime.Now < start.AddSeconds(10))
            {
                if ((lQueue.Count) < 6)
                {
                    
                    lQueue.Enqueue(producer.GiveMeNumber());
                    
                }
            }
            Console.WriteLine("ho fatto 10 secondi e finito di produrre numeri");
        }

        public static void ConsumeNumber()
        {
            DateTime start = DateTime.Now;
            while (DateTime.Now < start.AddSeconds(10))
            {
                if ((lQueue.Count) > 0)
                {
                    consumer.ConsumeNumber(lQueue.Dequeue());
                }
            }
            Console.WriteLine("ho fatto 10 secondi e finito di consumare dati");
        }
    }

    #region ProducerConsumer

    class Producer
    {
        private Random _random = new Random(772);
        public int GiveMeNumber()
        {
            int i = _random.Next(100);
            Console.WriteLine("ho prodotto il numero: " + i);
            return i;
        }
    }

    class Consumer
    {
        private List<int> _numbers = new List<int>();
        public void ConsumeNumber(int number)
        {
            _numbers.Add(number);
            Console.WriteLine("ho consumato il numero: " + number);
        }
    }

    class LimitedQueue<T> : Queue<T>
    {
        private int _maxSize;

        public LimitedQueue(int maxSize)
        {
            _maxSize = maxSize;
        }

        public new void Enqueue(T item)
        {
            if (this.Count >= _maxSize)
            {
                throw new InvalidOperationException("Queue is full");
            }
            base.Enqueue(item);
        }
    }

    #endregion
}
