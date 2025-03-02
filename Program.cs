﻿namespace Produttore_e_consumatore
{    
    internal class Program
    {
        static Semaphore consumerSemaphore = new Semaphore(7, 7);
        
        static Producer producer = new Producer();
        
        static Consumer consumer = new Consumer();

        public static LimitedQueue<int> lQueue = new LimitedQueue<int>(6);

        static void Main(string[] args)
        {            
            Thread threadProducer = new Thread(ProduceNumber);
            Thread threadConsumer = new Thread(ConsumeNumber);

            threadProducer.Start();
            threadConsumer.Start();

            threadConsumer.Join();
            threadProducer.Join();
           

            Console.WriteLine("il totale dei numeri prodotti è: " + producer.GetCount());
            Console.WriteLine("il totale dei numeri prodotti è: " + consumer.GetCountNumberGenerator());

            Console.WriteLine("il totale dei numeri consumati è: " + consumer.GetCount());
            Console.WriteLine("il totale dei numeri consumati è: " + consumer.GetCountNumberGenerator());

            var result = lQueue.GetCount();
            const string message = "il totale dei numeri presenti nella coda è: ";
            Console.WriteLine(message + result.Count);
            Console.WriteLine("il totale della coda è: " + result.CountNumberGenerator);

            Console.ReadKey();
        }

        public static void ProduceNumber()
        {
            DateTime start = DateTime.Now;
            while (DateTime.Now < start.AddSeconds(5))
            {
                if ((lQueue.Count) < 6)
                {

                    lQueue.Enqueue(producer.GiveMeNumber());

                }
                else {
                    Console.WriteLine("il produttore sta consumando CPU");
                }
            }
            
            Console.WriteLine("ho fatto 10 secondi e finito di produrre numeri");
        }

        public static void ConsumeNumber()
        {
            DateTime start = DateTime.Now;
            while (DateTime.Now < start.AddSeconds(5))
            {
                if ((lQueue.Count) > 0)
                {
                    consumer.ConsumeNumber(lQueue.Dequeue());
                }
                else { 
                    Console.WriteLine("il consumatore sta consumando CPU");
                }
            }
            Console.WriteLine("ho fatto 10 secondi e finito di consumare dati");
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
            Console.WriteLine("ho prodotto il numero: " + i);
            _count = _count +i;
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
            Console.WriteLine("ho consumato il numero: " + number);
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
            while (this.Count != 0) {                
                var isSuccess = this.TryDequeue(out int t);
                // var t = this.Dequeue();
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

    #endregion
}
