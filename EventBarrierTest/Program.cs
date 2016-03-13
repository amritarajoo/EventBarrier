using System;
using System.Threading;

namespace EventBarrierTest
{
    class EventBarrier
    {
        static ManualResetEvent consumerWaitHandle = new ManualResetEvent(false);
        static ManualResetEvent producerWaitHandle = new ManualResetEvent(false);

        object locker = new object();
        int NoOfConsumers = 0;

        // called by consumer to enter the gate
        // if gate is closed, consumer will wait
        // if gate is open, consumer will enter and do work
        public void Arrive()
        {
            lock (locker)
            {
                if (NoOfConsumers == 0)
                {
                    producerWaitHandle.Reset();
                }
                ++NoOfConsumers;
                consumerWaitHandle.WaitOne(); // wait if closed gate, this immediately returs if gate is open
            }
        }

        // called by producer to open gate
        public void Raise()
        {
            consumerWaitHandle.Set(); // open gate for consumer
            producerWaitHandle.WaitOne(); // producer should wait till all consumers are done
        }

        // called by consumer when work is done
        public void Complete()
        {
            lock (locker)
            {
                --NoOfConsumers;
                if (NoOfConsumers == 0) // check if last consumer is leaving
                {
                    consumerWaitHandle.Reset(); // close the open gate
                    producerWaitHandle.Set(); // let producer proceed and exit when last consumer is done
                }
            }
        }
    }

    class Program
    {
        static EventBarrier eb = new EventBarrier();
        static void Main(string[] args)
        {
            for (int i = 0; i < 3; i++)
            {
                Thread consumer = new Thread(ConsumerWork);
                consumer.Name = i.ToString();
                consumer.Start();
            }
            Thread producer = new Thread(ProducerWork);
            producer.Name = "p1";
            producer.Start();

            Thread.Sleep(1000);
            for (int i = 0; i < 2; i++)
            {
                Thread consumer = new Thread(ConsumerWork);
                consumer.Name = i.ToString();
                consumer.Start();
            }
            Thread producer2 = new Thread(ProducerWork);
            producer2.Name = "p2";
            producer2.Start();
            Console.ReadKey();
        }

        public static void ConsumerWork()
        {
            Console.WriteLine("Consumer {0} arrived", Thread.CurrentThread.Name);
            eb.Arrive();
            Console.WriteLine("Consumer {0} doing work", Thread.CurrentThread.Name);
            for (int i = 0; i < 10000; i++)
            {
                for (int j = 0; j < 10000; j++)
                {
                }
            }
            eb.Complete();
            Console.WriteLine("Consumer {0} exiting", Thread.CurrentThread.Name);
        }

        public static void ProducerWork()
        {
            Console.WriteLine("Producer {0} is raising gate", Thread.CurrentThread.Name);
            eb.Raise();

            Console.WriteLine("Producer {0} ending", Thread.CurrentThread.Name);
        }
    }
}
