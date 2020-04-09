using Bot.Services.Orderbook;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Channel
{
    public enum Service
    {
        Default,
        Order,
        Ticks,
        Orderbook,
        Analysis
    }
    public class QueueItemArgs : EventArgs
    {
        public dynamic Item { get; set; }
        public QueueItemArgs(dynamic input)
        {
            this.Item = input;

        }
    }


    public class GenericQueue<T>
    {
        public virtual string Name { get; set; } = "Generic";
        public Service ServiceType { get; set; } = Service.Default;
        private LinkedList<EventHandler<QueueItemArgs>> eventHandlers = new LinkedList<EventHandler<QueueItemArgs>>();

        public event EventHandler<QueueItemArgs> OnAddHandler
        {
            add
            {
                
                eventHandlers.AddLast(value);
            }
            remove
            {
                eventHandlers.RemoveLast();
            }

        }
        private readonly Queue<T> queue  = new Queue<T>();

        public event EventHandler Enqueued;
        protected virtual void NewQueueItem(T item)
        {
            foreach (EventHandler<QueueItemArgs> handler in eventHandlers)
            {
                Task.Run(() =>
                {
                    handler?.Invoke(this, new QueueItemArgs(item));

                });

                //handler(this, EventArgs.Empty);
            }

            //Enqueued?.Invoke(this, new QueueItemArgs(item));
        }
        public virtual void Enqueue(T item)
        {
            queue.Enqueue(item);
            NewQueueItem(item);
        }
        public virtual void Clear()
        {
            this.queue.Clear();
        }
        public int Count
        {
            get
            {
                return queue.Count;
            }
        }
        public virtual T Dequeue()
        {
            T item = queue.Dequeue();
            return item;
        }


    }
}
