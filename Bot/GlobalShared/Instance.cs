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

        protected readonly Logger Logger;
        public string PairLink { get; set; }
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
            //socketClient.SubscribeToKlineUpdates(PairLink, KlineInterval.OneMinute, KL1Min);
            //socketClient.SubscribeToSymbolTickerUpdates(PairLink, TT5);
            socketClient.SubscribeToPartialOrderBookUpdates(PairLink,5, 100, OrderBookHandler);

        }

        private void OrderBookHandler(BinanceOrderBook obj)
        {
            var BestAsk = (List<BinanceOrderBookEntry>)obj.Asks;
            var BestBid = (List<BinanceOrderBookEntry>)obj.Bids;
            var Capture = BestAsk.ConvertToBook(BestBid);
            Capture.Compute();
            BookSnapshot.AddBook(Capture);

            var RounderAsk = BestAsk.Select(y => Math.Round(y.Price, 2)).ToList().ToList();
            var RounderAskVol = BestAsk.Select(y => Math.Round(y.Quantity, 2)).ToList().ToList();

            var RounderBid = BestBid.Select(y => Math.Round(y.Price, 2)).ToList().ToList();
            var RounderBidVol = BestBid.Select(y => Math.Round(y.Quantity, 2)).ToList().ToList();

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

            //TBD
            Logger.Info(string.Format("CVD : {0}", CurrentCumulativeDelta));

            Logger.Info(string.Format("WAP : {0}", obj.WeightedAveragePrice));

            CurrentCumulativeDelta -= obj.AskQuantity;

            CurrentCumulativeDelta += obj.BidQuantity;

        }

    private void KL1Min(BinanceStreamKlineData obj)
        {
            //this.IsAllowedIntoRange = obj.Data.Open > obj.Data.Close;
            //CurrentCumulativeDelta = (obj.Data.Volume - obj.Data.TakerBuyQuoteAssetVolume);
            //Logger.Info(string.Format("CVD : {0}", CurrentCumulativeDelta));
            //Logger.Info(string.Format("VOL : {0}", obj.Data.Volume));
            if (obj.Data.Final) { CurrentCumulativeDelta = 0; }
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
