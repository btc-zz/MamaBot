using Bot.Data;
using Bot.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Bot.Services.Orderbook
{

    public class OrderFlowChange : EventArgs
    {
        public DateTime Update { get; set; } = DateTime.Now;
        public Flow Level {get;set;}
        public OrderFlowChange()
        {

        }
    }

    public enum OrderDirection
    {
        Buy,
        Sell
    }

    public class Order {
        public long OrderId { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public OrderDirection Direction {get;set;}
        public DateTime TradeTime { get; set; }
        public Order()
        {

        }
        public Order(long orderId, decimal price, decimal quantity, OrderDirection direction, DateTime tradeTime)
        {
            this.OrderId = orderId;
            this.Price = price;
            this.Quantity = quantity;
            this.Direction = direction;
            this.TradeTime = tradeTime;

        }
    }

    public class OrderFlowStatistics
    {

        public decimal BearishVolume { get; set; } = 0;
        public decimal BullishVolume { get; set; } = 0;
        public decimal VolumeIntensity { get; set; } = 0;
        public decimal VolumePerTrade { get; set; } = 0;
        public List<Order> Orders { get; set; } = new List<Order>();
        public decimal RevRSScore { get; set; } = 0;
        public List<decimal> X { get; set; } = new List<decimal>();
        public List<decimal> T { get; set; } = new List<decimal>();
        public List<decimal> V { get; set; } = new List<decimal>();
        public int PeriodicAnalysisLong = 70;
        public int PeriodicAnalysisShort = 30;

        public event EventHandler<OrderFlowChange> StatisticReady;
        public event EventHandler<Channel.QueueItemArgs> OrderChannelData;

        public OrderFlowStatistics()
        {
            this.StatisticReady += OrderFlowStatsComputed;
            this.OrderChannelData += Queue_OnAddHandler;
        }

        public void Queue_OnAddHandler(object sender, Channel.QueueItemArgs e)
        {
            var CastItem = (Order)e.Item;
            AddOrder(CastItem);
        }
        /// <summary>
        /// Todo : Add validation here
        /// </summary>
        /// <param name="NewOrder"></param>
        public async void AddOrder(Order NewOrder) { this.Orders.Add(NewOrder);
            if(this.Orders.Count > PeriodicAnalysisLong)
            {
                var selectPrice = this.Orders.Select(y => y.Price).ToList();
                //Start computation here
                var test = TestCMPT(selectPrice, 7, 30);
                var test2 = TestCMPT(selectPrice, 7, 70);
                MamaBot.GlobalShared.Vars.Logger.LogDebug("Call sent to CMPT");
                decimal TestCMPT(List<decimal> xList, decimal t, decimal v)
                {
                    decimal output = 0;

                    Parallel.ForEach(xList, x =>
                    {
                        var indexofP = xList.ToList().IndexOf(x) - 1;
                        decimal er = 2 * t - 1;
                        decimal u = 2 / (er + 1);
                        decimal e = 0;
                        decimal ax = x > xList.GetValueBefore() ? u * (xList.GetValueBefore() - x) + (1 - u) : -1;
                        decimal z = (t - 1) * (ax * v / 100 - v) - e;
                        decimal o = z >= 0 ? x + z : x + z * (100 - v) / v;
                        output = o;

                    });
                    return output;

                }

            }

        }


        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="x"></param>
        /// <param name="t"></param>
        /// <param name="v"></param>
        internal void RevScoreCompute(decimal x,decimal t,decimal v)
        {
            X.ListHadEnoughPeriod(70);

        }


        /// <summary>
        /// Default handler of OrderbookChangeEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OrderFlowStatsComputed(object sender, OrderFlowChange e)
        {


        }



        /// <summary>
        /// Route OrderbookChange event outside class for another operation
        /// </summary>
        /// <param name="SentTo"></param>
        public void AddCustomHandler(EventHandler<OrderFlowChange> SentTo)
        {
            this.StatisticReady += SentTo;
        }
    }
}
