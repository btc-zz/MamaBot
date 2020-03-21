using Binance.Net;
using Binance.Net.Objects;
using Bot.DataProvider;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using MamaBot.GlobalShared;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace MaMa.HFT.Console.GlobalShared
{
    public class Instance
    {
        public BinanceClient client { get; set; }
        public BinanceSocketClient socketClient = new BinanceSocketClient();
        private CancellationToken token;
        decimal CurrentCumulativeDelta = 0;
        public BookSnap BookSnapshot = new BookSnap();
        public PriceMapSnap MapHistory = new PriceMapSnap();
        public PriceMap Map = new PriceMap();
        protected readonly Logger Logger;
        public string PairLink { get; set; }
        public List<BinanceStreamTrade> BuyerMatcher = new List<BinanceStreamTrade>();

        public List<BinanceStreamTrade> SellerMatcher = new List<BinanceStreamTrade>();

        public string ListenerKey { get; set; }
        public Instance(string pair,string api,string apisec)
        {
            Logger = LogManager.GetCurrentClassLogger();

            PairLink = pair;


            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(api, apisec),
                LogVerbosity = LogVerbosity.Debug
            });

            BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
            {
                ApiCredentials = new ApiCredentials(api, apisec),
                LogVerbosity = LogVerbosity.Debug
            });
            client = new BinanceClient();
            var accountInfo = client.GetAccountInfo();
            ListenerKey = client.StartUserStream().Data;
            OrderDataStream();
        }

        public void OrderDataStream()
        {
            socketClient.SubscribeToUserDataUpdates(ListenerKey, null, OrderStreamUpdate, OrderStream, BalanceStream, null);

        }

        public void StartMaker()
        {


        }

        public void ReceiveOrderBook()
        {

        }


        public void PlaceOrder(string PairLink, OrderSide side , OrderType type, decimal quantity, decimal price)
        {

            var orderResult = client.PlaceOrder(PairLink, side, type, quantity, null, null, price, TimeInForce.GoodTillCancel, null, null, null, null, token);
            if(orderResult.Error != null && Vars.ShowOrderErrors)
            {
                Logger.Error(string.Format("Order not executed due to : {0}", orderResult.Error.Message));

            }
            else
            {

            }


        }

        public void ObSub()
        {
            //socketClient.SubscribeToBookTickerUpdates(PairLink, HandleBookOffer);

            Task.Run(() =>
            {
                socketClient.SubscribeToKlineUpdates(PairLink, KlineInterval.OneMinute, KL1Min);

            });
            Task.Run(() =>
            {
                socketClient.SubscribeToTradeUpdates(PairLink, TT7);

            });
            Task.Run(() =>
            {
                socketClient.SubscribeToSymbolTickerUpdates(PairLink, TT5);

            });
            Task.Run(() =>
            {
                socketClient.SubscribeToPartialOrderBookUpdates(PairLink, 5, 100, OrderBookHandler);

            });




            //socketClient.SubscribeToTradeUpdates(PairLink, TT7);
            //socketClient.SubscribeToSymbolTickerUpdates(PairLink, TT5);
            //socketClient.SubscribeToPartialOrderBookUpdates(PairLink, 5, 100, OrderBookHandler);

        }

        private void TT7(BinanceStreamTrade obj)
        {
            try
            {
                if (obj.BuyerIsMaker)
                {
                    BuyerMatcher.Add(obj);
                    var GrouperBuyer = BuyerMatcher.GroupBy(y => y.BuyerIsMaker = true);
                    var LastBuyer = GrouperBuyer.Last();
                    var FilledBuyerQuantity = LastBuyer.Sum(y => y.Quantity);
                    var FilledBuyerPrice = LastBuyer.Last().Price;
                    Logger.Info(string.Format("FilledBuyerQuantity : {0}", FilledBuyerQuantity));
                    Logger.Info(string.Format("FilledBuyerPrice : {0}", FilledBuyerPrice));

                }
                else
                {
                    SellerMatcher.Add(obj);
                    var GrouperSeller = SellerMatcher.GroupBy(y => y.BuyerIsMaker = false);
                    var LastSeller = GrouperSeller.Last();
                    var FilledSellerQuantity = LastSeller.Sum(y => y.Quantity);
                    var FilledSellerPrice = LastSeller.Last().Price;
                    Logger.Info(string.Format("FilledSellerQuantity : {0}", FilledSellerQuantity));
                    //Logger.Info(string.Format("FilledSellerPrice : {0}", FilledSellerPrice));

                }





            }
            catch
            {

            }
        }

        private void OrderBookHandler(BinanceOrderBook obj)
        {
            var BestAsk = (List<BinanceOrderBookEntry>)obj.Asks;
            var BestBid = (List<BinanceOrderBookEntry>)obj.Bids;
            var Capture = BestAsk.ConvertToBook(BestBid);
            Capture.Compute();
            BookSnapshot.AddBook(Capture);

            var RounderAsk = BestAsk.Select(y => Math.Round(y.Price, 0)).ToList().ToList();
            var RounderAskVol = BestAsk.Select(y => Math.Round(y.Quantity, 5)).ToList().ToList();

            var RounderBid = BestBid.Select(y => Math.Round(y.Price, 0)).ToList().ToList();
            var RounderBidVol = BestBid.Select(y => Math.Round(y.Quantity, 5)).ToList().ToList();

            for(int i = 0;i < RounderAsk.Count; i++)
            {
                Map.AddPrice(new PriceLayer(RounderAsk[i], RounderAskVol[i], PriceDirection.Ask));
            }
            for (int i = 0; i < RounderBid.Count; i++)
            {
                Map.AddPrice(new PriceLayer(RounderBid[i], RounderBidVol[i], PriceDirection.Bid));
            }
            Logger.Info(string.Format("Price : {0}", Map.Map.Last().Price));
            Logger.Info(string.Format("Ask Quantity Price : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Ask).Last().PriceLayerRef));
            Logger.Info(string.Format("Ask Quantity Before : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Ask).Last().QuantityBefore));
            Logger.Info(string.Format("Ask Quantity After : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Ask).Last().QuantityAfter));

            Logger.Info(string.Format("Bid Quantity Price : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Bid).Last().PriceLayerRef));
            Logger.Info(string.Format("Bid Quantity Before : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Bid).Last().QuantityBefore));
            Logger.Info(string.Format("Bid Quantity After : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Bid).Last().QuantityAfter));

            //Map.AddPrice(new PriceLayer(RounderAsk.First(), RounderAskVol.First(), decimal.MinusOne,false,true));
            //Map.AddPrice(new PriceLayer(RounderAsk.First(), RounderAskVol.First(), null, true, false));

            CurrentCumulativeDelta += RounderAskVol.First();
            CurrentCumulativeDelta -= RounderBidVol.First();
            
            //Logger.Info(string.Format("CVD : {0}", CurrentCumulativeDelta));

            //Logger.Info(string.Format("RounderAskVol : {0}", RounderAskVol.First()));
            //Logger.Info(string.Format("RounderBidVol : {0}", RounderBidVol.First()));


            //Extract Average volume Per Price;




            //BookEntry Entry = new BookEntry(BestAsk[0].Price, BestBid[0].Price, obj.LastUpdateId);


            //Logger.Info(string.Format("Spread : {0}", Entry.PriceSpread));
            //Logger.Info(string.Format("MediumPrice : {0}", Entry.MediumPrice));
            //Logger.Info(string.Format("Ask : {0}", Entry.Ask));
            //Logger.Info(string.Format("CumuluatedBuyerOnHotRange : {0}", CumuluatedBuyerOnHotRange));
            //Logger.Info(string.Format("BestLiquidAsk : {0}", BestLiquidAsk));

            //Logger.Info(string.Format("Bid : {0}", Entry.Bid));
            //Logger.Info(string.Format("CumuluatedSellerOnHotRange : {0}", CumuluatedSellerOnHotRange));
            //Logger.Info(string.Format("BestLiquidBid : {0}", BestLiquidBid));



            //if (WANTTOBUY)
            //{
            //    Task.Run(() =>
            //    {
            //        this.RemoveAllDirectionOrder(Binance.Net.Objects.OrderSide.Buy);
            //        //this.RemoveAllDirectionOrder(Binance.Net.Objects.OrderSide.Sell);

            //        this.PlaceOrder("BTCUSDT", Binance.Net.Objects.OrderSide.Buy, Binance.Net.Objects.OrderType.Limit, decimal.Round(.0022m, 4), decimal.Round(Entry.Bid, 3));
            //        //this.PlaceOrder("BTCUSDT", Binance.Net.Objects.OrderSide.Sell, Binance.Net.Objects.OrderType.Limit, decimal.Round(.0022m, 4), decimal.Round(Entry.Ask, 2));

            //    });
            //}

        }

        private void TT5(BinanceStreamTick obj)
        {
            var LastPrice = obj.LastPrice;
            var BidPrice = obj.BidPrice;
            var AskPrice = obj.BidPrice;

            //TBD
            //Logger.Info(string.Format("LastPrice : {0}", LastPrice));
            //Logger.Info(string.Format("BidPrice : {0}", BidPrice));
            //Logger.Info(string.Format("AskPrice : {0}", AskPrice));

            //Logger.Info(string.Format("WAP : {0}", obj.WeightedAveragePrice));

            CurrentCumulativeDelta -= obj.AskQuantity;

            CurrentCumulativeDelta += obj.BidQuantity;

        }

    private void KL1Min(BinanceStreamKlineData obj)
        {
            //Periodic reset (Temporary)


            //this.IsAllowedIntoRange = obj.Data.Open > obj.Data.Close;
            //CurrentCumulativeDelta = (obj.Data.Volume - obj.Data.TakerBuyQuoteAssetVolume);
            //Logger.Info(string.Format("CVD : {0}", CurrentCumulativeDelta));
            //Logger.Info(string.Format("VOL : {0}", obj.Data.Volume));
            if (obj.Data.Final) { CurrentCumulativeDelta = 0; MapHistory.AddMap(Map); ; Map.Clear(); SellerMatcher.Clear();BuyerMatcher.Clear();
            }
        }

        public void RemoveAllDirectionOrder(OrderSide direction)
        {

            try
            {
                var CurrentOrder = client.GetOpenOrders(PairLink);
                foreach (var order in CurrentOrder.Data)
                {
                    if (order.Side == direction && order.Symbol == PairLink)
                    {
                        client.CancelOrder(PairLink, order.OrderId);
                    }
                }

            }
            catch
            {

            }


        }

        /// <summary>
        /// Here goes equity curve calculation
        /// </summary>
        /// <param name="obj"></param>
        private void BalanceStream(IEnumerable<BinanceStreamBalance> obj)
        {
            foreach(var value in obj)
            {

                //Logger.Info(string.Format("Account update received for : {0}",value.Asset));
                //Logger.Info(string.Format("Account update received for : {0}", value.Total));
                //Logger.Info(string.Format("Account update received for : {0}", value.Free));

            }
        }

        private void OrderStream(BinanceStreamOrderList obj)
        {


                //Logger.Info(string.Format("TransactionTime received for : {0}", obj.TransactionTime));
                //Logger.Info(string.Format("ListOrderStatus received for : {0}", obj.ListOrderStatus));


        }

        private void OrderStreamUpdate(BinanceStreamOrderUpdate obj)
        {
            switch (obj.Status)
            {
                case OrderStatus.Filled:
                    //if (obj.Side == OrderSide.Buy)
                    //{
                    //    this.RemoveAllDirectionOrder(Binance.Net.Objects.OrderSide.Sell);

                    //}
                    //if (obj.Side == OrderSide.Sell)
                    //{
                    //    this.RemoveAllDirectionOrder(Binance.Net.Objects.OrderSide.Buy);

                    //}
                    break;
                    Logger.Info(string.Format("TransactionTime received for : {0}", obj.Status));
                    Logger.Info(string.Format("ListOrderStatus received for : {0}", obj.Quantity));
            }
        }
    }
}
