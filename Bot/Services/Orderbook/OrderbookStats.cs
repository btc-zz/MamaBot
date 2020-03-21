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
        public OrderbookStatistic()
        {

        }
    }
}
