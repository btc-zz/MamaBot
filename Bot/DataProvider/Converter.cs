using Binance.Net.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.DataProvider
{
    public static class Converter
    {
        public static Book ConvertToBook(this List<BinanceOrderBookEntry> Asks, List<BinanceOrderBookEntry> Bids)
        {
            Book test = new Book();
            for (int IndexOfEntry = 0; IndexOfEntry < Asks.Count; IndexOfEntry++)
            {
                test.Asks.Add(new SimpleBookEntry(Asks[IndexOfEntry].Price, Asks[IndexOfEntry].Quantity));
                test.Bids.Add(new SimpleBookEntry(Bids[IndexOfEntry].Price, Bids[IndexOfEntry].Quantity));
            }
            return test;
        }

    }
}
