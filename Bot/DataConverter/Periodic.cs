using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Data
{
    public static class PeriodicExtension
    {
        public static bool ListHadEnoughPeriod(this List<decimal> Data,int period)
        {
            return Data.Count > period;
        }
        public static decimal GetValueBefore(this List<decimal> Data)
        {
            return Data[Data.Count - 1];
        }
        public static bool ValueIsGreaterThenLast(this List<decimal> Data)
        {
            return (Data[Data.Count - 1] > Data[Data.Count - 2]);
        }
        public static bool ValueIsLowerThenLast(this List<decimal> Data)
        {
            return (Data[Data.Count - 1] < Data[Data.Count - 2]);
        }


    }
}
