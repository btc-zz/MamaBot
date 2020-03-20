using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.DataProvider
{
    public enum OrderKind
    {
        ToIgnore,
        Important,
        Critical
    }

    public class PriceLayerChange
    {
        public decimal PriceLayerRef { get; set; }
        public decimal QuantityBefore { get; set; }
        public decimal QuantityAfter { get; set; }
        public int Deviation { get; set; }
        public PriceDirection TheChoice { get; set; }
        public PriceLayerChange()
        {

        }
        public PriceLayerChange(decimal price,decimal oldquantity,decimal newquantity,PriceDirection dir)
        {
            this.PriceLayerRef = price;
            this.QuantityBefore = oldquantity;
            this.QuantityAfter = newquantity;
            this.TheChoice = dir;

        }

    }
    public class PriceMap
    {

        public List<PriceLayer> Map { get; set; } = new List<PriceLayer>();
        public List<PriceLayerChange> Update { get; set; } = new List<PriceLayerChange>();
        public DateTime StartDate { get; set; } = DateTime.Now;
        public PriceMap()
        {

        }
        public void Clear() { this.Map.Clear(); }
        public void AddPrice(PriceLayer PriceToAdd)
        {
            //Implement level change Class
            

            var PriceFound = this.Map.Count > 0 ? this.Map.Find(y => y.Price == PriceToAdd.Price) : null;
            if (PriceFound != null)
            {
                //Operation Add
                if(PriceToAdd.Direction == PriceDirection.Ask)
                {
                    this.Update.Add(new PriceLayerChange(PriceFound.Price, PriceFound.AskQuantity, PriceToAdd.AskQuantity, PriceDirection.Ask));
                    PriceFound.AskQuantity += PriceToAdd.AskQuantity;
                }
                if (PriceToAdd.Direction == PriceDirection.Bid)
                {
                    this.Update.Add(new PriceLayerChange(PriceFound.Price, PriceFound.BidQuantity, PriceToAdd.BidQuantity, PriceDirection.Bid));
                    PriceFound.BidQuantity += PriceToAdd.BidQuantity;
                }

                this.Map.Remove(PriceFound);
                this.Map.Add(PriceFound);
            }
            else
            {
                this.Map.Add(PriceToAdd);
            }
            



        }
    }
    public class PriceMapSnap
    {

        public List<PriceMap> SnapMap { get; set; } = new List<PriceMap>();
        public PriceMapSnap()
        {

        }
        public void AddMap(PriceMap PriceToAdd)
        {

            this.SnapMap.Add(PriceToAdd);

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
