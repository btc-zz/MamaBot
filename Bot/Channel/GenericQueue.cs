using System;
using System.Collections.Generic;
using System.Text;

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
        public string Name { get; set; } = "Generic";
        public Service ServiceType { get; set; } = Service.Default;
        public event EventHandler<QueueItemArgs> OnAddHandler;
        private readonly Queue<T> queue  = new Queue<T>();

        public event EventHandler Enqueued;
        protected virtual void NewQueueItem(T item)
        {
            Enqueued?.Invoke(this, new QueueItemArgs(item));
        }
        public virtual void Enqueue(T item)
        {
            queue.Enqueue(item);
            NewQueueItem(item);
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
