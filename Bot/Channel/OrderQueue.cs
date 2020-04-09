using Bot.Services.Orderbook;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Channel
{
    public class OrderQueue
    {
        public GenericQueue<Order> Queue { get; set; } = new GenericQueue<Order>();
        public DateTime StartTime { get; set; } = DateTime.Now;

        public OrderQueue()
        {
            this.Queue.Name = "OrderQueue";
            this.Queue.ServiceType = Service.Order;
            this.Queue.OnAddHandler += Queue_Enqueued;
        }

        private void Queue_Enqueued(object sender, QueueItemArgs e)
        {
            var CastItem = (Order)e.Item;

        }
        public void AddSubscription(OrderFlowStatistics Route)
        {
            this.Queue.OnAddHandler += Route.Queue_OnAddHandler;
        }

    }
}
