using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.DataProvider
{
   public class SimpleBookEntry
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }

        public SimpleBookEntry()
        {

        }
        public SimpleBookEntry(decimal price, decimal quantity)
        {
            Price = price;
            Quantity = quantity;
        }
    }
}
