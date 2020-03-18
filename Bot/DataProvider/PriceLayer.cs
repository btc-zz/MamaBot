using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.DataProvider
{
    public class PriceMap
    {

        public List<PriceLayer> Map { get; set; } = new List<PriceLayer>();
        public DateTime StartDate { get; set; } = DateTime.Now;
        public PriceMap()
        {

        }
        public void AddPrice(PriceLayer PriceToAdd)
        {
            var PriceFound = this.Map.Count > 0 ? this.Map.Find(y => y.Price == PriceToAdd.Price) : null;
            if (PriceFound != null)
            {
                if (PriceToAdd.Direction == PriceDirection.Ask) { PriceFound.AskQuantity += PriceToAdd.AskQuantity; }
                else {
                    PriceFound.BidQuantity += PriceToAdd.BidQuantity; }
                this.Map.Remove(PriceFound);
                Console.WriteLine("Price : {0}", PriceFound.Price);
                Console.WriteLine("AskQ : {0}", PriceFound.AskQuantity);
                Console.WriteLine("BidQ : {0}", PriceFound.BidQuantity);
                this.Map.Add(PriceFound);
            }
            else
            {
                this.Map.Add(PriceToAdd);
            }


        }
    }
    public enum PriceDirection
    {
        Ask, Bid
    }


    public class PriceLayer
    {
        public decimal Price { get; set; }
        public decimal AskQuantity { get; set; }
        public decimal BidQuantity { get; set; }
        public decimal Quantity =>  AskQuantity + BidQuantity;
        public PriceDirection Direction { get; set; }
        public PriceLayer(decimal price, decimal quantity, PriceDirection dir)
        {
            Price = price;
            Direction = dir;

            if (dir == PriceDirection.Ask) { this.AskQuantity = quantity; }
            else { this.BidQuantity = quantity; }
        }

    }
}
