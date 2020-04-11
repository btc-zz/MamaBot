using Binance.Net.Objects.Spot.MarketStream;
using Bot.Services.Orderbook;
using MaMa.HFT.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Channel
{
    public class TickQueue
    {
        public GenericQueue<BinanceStreamTick> Queue { get; set; } = new GenericQueue<BinanceStreamTick>();
        public DateTime StartTime { get; set; } = DateTime.Now;
        public Dictionary<int, int> TickPeriod { get; set; } = new Dictionary<int, int>();
        
        /// <summary>
        /// Class responsible to distribute stats of current tick every informations (period of tick..) and TA
        /// </summary>
        public TickQueue()
        {
            this.Queue.Name = "TickQueue";
            this.Queue.ServiceType = Service.Ticks;
            this.Queue.OnAddHandler += Queue_Enqueued;
            this.TickPeriod.Add(0, 9);
            this.TickPeriod.Add(10, 19);
            this.TickPeriod.Add(20, 29);
            this.TickPeriod.Add(30, 39);
            this.TickPeriod.Add(40, 49);
            this.TickPeriod.Add(50, 59);
        }

        /// <summary>
        /// Return Index of Ticks into candle to keep their sizing reconstructible into 1min candle
        /// </summary>
        /// <returns></returns>
        public int GetCurrentTimeTicksIndex()
        {
            int Second = DateTime.Now.Second;

            int IndexOfCandle = -0;
            foreach (KeyValuePair<int, int> PeriodValue in this.TickPeriod)
            {
                if (Second >= PeriodValue.Key && Second <= PeriodValue.Value)
                {
                    IndexOfCandle = this.TickPeriod.Values.ToList().IndexOf(PeriodValue.Value);
                }
            }
            return IndexOfCandle;
        }
        private void Queue_Enqueued(object sender, QueueItemArgs e)
        {
            var CastItem = (BinanceStreamTick)e.Item;

        }
        public void AddSubscription(CandleService Route)
        {
            this.Queue.OnAddHandler += Route.Queue_OnAddHandler;
        }

    }
}
