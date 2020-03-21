using Bot.DataProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Services.Orderbook
{

    public class OrderbookChange : EventArgs
    {
        public DateTime Update { get; set; } = DateTime.Now;
        public BookLevel Level {get;set;}
        public OrderbookChange()
        {

        }
    }
    public class OrderbookStatistic
    {
        public decimal BearishVolume { get; set; } = 0;
        public decimal BullishVolume { get; set; } = 0;
        public decimal VolumeIntensity { get; set; } = 0;
        public decimal VolumePerTrade { get; set; } = 0;
      
        public event EventHandler<OrderbookChange> BookUpdate;

        /// <summary>
        /// Default handler of OrderbookChangeEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OrderbookStaticHandler(object sender, OrderbookChange e)
        {
            //Generic Implementation
        }
        public OrderbookStatistic()
        {
            this.BookUpdate += OrderbookStaticHandler;

        }

        /// <summary>
        /// Route OrderbookChange event outside class for another operation
        /// </summary>
        /// <param name="SentTo"></param>
        public void AddCustomHandler(EventHandler<OrderbookChange> SentTo)
        {
            this.BookUpdate += SentTo;
        }
    }
}
