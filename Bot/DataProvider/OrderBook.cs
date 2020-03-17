using Binance.Net.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.DataProvider
{
    public class Book
    {
        public List<SimpleBookEntry> Asks { get; set; } = new List<SimpleBookEntry>();
        public List<SimpleBookEntry> Bids { get; set; } = new List<SimpleBookEntry>();
        public decimal CumuluatedSellerOnHotRange { get; set; }
        public decimal CumuluatedBuyerOnHotRange { get; set; }
        public decimal BestLiquidAsk { get; set; }
        public decimal BestLiquidBid { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.Now;
        public Book()
        {

        }
        public Book(List<SimpleBookEntry> InAsk, List<SimpleBookEntry> InBid)
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
        public List<Book> OrderBookSnap = new List<Book>();
        public BookSnap()
        {

        }
        public void AddBook(Book DataIn)
        {
            OrderBookSnap.Add(DataIn);
        }
    }
}
