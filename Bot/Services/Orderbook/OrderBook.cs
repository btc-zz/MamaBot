using Binance.Net.Objects;
using Bot.Services.Orderbook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.DataProvider
{
    public enum BookOperation
    {
        Add,
        Reduce
    }

    public enum Flow
    {
        Layer0,
        Layer1,
        Layer2,
        Layer3,
        Layer4,
        Layer5
    }

    public class OrderBook
    {
        public List<SimpleBookEntry> Asks { get; set; } = new List<SimpleBookEntry>();
        public List<SimpleBookEntry> Bids { get; set; } = new List<SimpleBookEntry>();
        public decimal CumuluatedSellerOnHotRange { get; set; }
        public decimal CumuluatedBuyerOnHotRange { get; set; }
        public decimal BestLiquidAsk { get; set; }
        public decimal BestLiquidBid { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public OrderBook()
        {

        }
        public OrderBook(List<SimpleBookEntry> InAsk, List<SimpleBookEntry> InBid)
        {
            this.Asks = InAsk;
            this.Bids = InBid;
        }
        public void Compute()
        {
            CumuluatedSellerOnHotRange = this.Asks.Sum(a => a.Quantity);
            CumuluatedBuyerOnHotRange = this.Bids.Sum(y => y.Quantity);

            BestLiquidAsk = this.Asks.OrderBy(a => a.Quantity).First().Price;
            BestLiquidBid = this.Bids.OrderBy(a => a.Quantity).First().Price;

        }
    }

    public class BookSnap
    {
        public List<OrderBook> OrderBookSnap = new List<OrderBook>();
        public OrderFlowStatistics Stats = new OrderFlowStatistics();
        public BookSnap()
        {

        }


        public void AddBook(OrderBook DataIn)
        {
            OrderBookSnap.Add(DataIn);

        }
    }
}
