﻿using Binance.Net;
using Binance.Net.Objects;
using Bot.DataProvider;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using MamaBot.GlobalShared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Bot.Services.Orderbook;
using Bot;
using Microsoft.Extensions.Logging;

namespace BotApp
{
    public class BotIstance
    {
        private readonly BotConfig _botConfig;
        private readonly ILogger _logger;

        public BinanceClient _client { get; set; }
        public BinanceSocketClient _socketClient = new BinanceSocketClient();
        decimal CurrentCumulativeDelta = 0;
        public BookSnap BookSnapshot = new BookSnap();
        public PriceMapSnap MapHistory = new PriceMapSnap();
        public PriceMap Map = new PriceMap();
        public OrderFlowStatistics TT2 = new OrderFlowStatistics();
        public List<BinanceStreamTrade> BuyerMatcher = new List<BinanceStreamTrade>();
        public List<BinanceStreamTrade> SellerMatcher = new List<BinanceStreamTrade>();
        public event EventHandler<OrderFlowChange> BookUpdate;

        public string ListenerKey { get; set; }

        public BotIstance(BotConfig botConfig, ILogger<BotIstance> logger)
        {
            _botConfig = botConfig;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client = new BinanceClient(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(_botConfig.ApiKey, _botConfig.ApiSecret),
                LogVerbosity = LogVerbosity.Debug
            });
            //var accountInfo = await client.GetAccountInfoAsync(ct: cancellationToken);
            var streamResult = await _client.StartUserStreamAsync(cancellationToken);

            ListenerKey = streamResult.Data;

            await OrderDataStreamAsync(cancellationToken);

            await SubscribeSocketsAsync(cancellationToken);

            TT2.StatisticReady += BookUpdate;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task OrderDataStreamAsync(CancellationToken cancellationToken)
        {
            await _socketClient.SubscribeToUserDataUpdatesAsync(ListenerKey, null, OrderStreamUpdate, OrderStream, BalanceStream, null);
        }

        public void StartMaker()
        {


        }

        public void ReceiveOrderBook()
        {

        }

        //public void PlaceOrder(string PairLink, OrderSide side , OrderType type, decimal quantity, decimal price)
        //{

        //    var orderResult = _client.PlaceOrder(PairLink, side, type, quantity, null, null, price, TimeInForce.GoodTillCancel, null, null, null, null, token);

        //    if(orderResult.Error != null && Vars.ShowOrderErrors)
        //    {
        //        _logger.LogError("Order not executed due to : {0}" + orderResult.Error.Message);

        //    }
        //    else
        //    {

        //    }
        //}

        public async Task SubscribeSocketsAsync(CancellationToken cancellationToken)
        {
            //socketClient.SubscribeToBookTickerUpdates(_botConfig.Pair, HandleBookOffer);

            await _socketClient.SubscribeToKlineUpdatesAsync(_botConfig.Pair, KlineInterval.OneMinute, KL1Min);
            await _socketClient.SubscribeToTradeUpdatesAsync(_botConfig.Pair, OrderSocketHandler);
            await _socketClient.SubscribeToSymbolTickerUpdatesAsync(_botConfig.Pair, TT5);
            await _socketClient.SubscribeToPartialOrderBookUpdatesAsync(_botConfig.Pair, 5, 100, OrderBookHandler);

            //socketClient.SubscribeToTradeUpdates(_botConfig.Pair, TT7);
            //socketClient.SubscribeToSymbolTickerUpdates(_botConfig.Pair, TT5);
            //socketClient.SubscribeToPartialOrderBookUpdates(_botConfig.Pair, 5, 100, OrderBookHandler);
        }

        /// <summary>
        /// Method used to receive Order socked
        /// </summary>
        /// <param name="Trade"></param>
        private void OrderSocketHandler(BinanceStreamTrade Trade)
        {
            try
            {

                if (Trade.BuyerIsMaker)
                {
                    BuyerMatcher.Add(Trade);
                    TT2.AddOrder(new Order(Trade.OrderId, Trade.Price, Trade.Quantity, OrderDirection.Buy, Trade.TradeTime));
                    var GrouperBuyer = BuyerMatcher.GroupBy(y => y.BuyerIsMaker = true);
                    var LastBuyer = GrouperBuyer.Last();
                    var FilledBuyerQuantity = LastBuyer.Sum(y => y.Quantity);
                    var FilledBuyerPrice = LastBuyer.Last().Price;
                    _logger.LogInformation($"FilledBuyerQuantity : {FilledBuyerQuantity}");
                    _logger.LogInformation($"FilledBuyerPrice : {FilledBuyerPrice}");

                }
                else
                {
                    SellerMatcher.Add(Trade);
                    TT2.AddOrder(new Order(Trade.OrderId, Trade.Price, Trade.Quantity, OrderDirection.Sell, Trade.TradeTime));
                    var GrouperSeller = SellerMatcher.GroupBy(y => y.BuyerIsMaker = false);
                    var LastSeller = GrouperSeller.Last();
                    var FilledSellerQuantity = LastSeller.Sum(y => y.Quantity);
                    var FilledSellerPrice = LastSeller.Last().Price;
                    _logger.LogInformation($"FilledSellerQuantity : {FilledSellerQuantity}");
                    _logger.LogInformation($"FilledSellerPrice : {FilledSellerPrice}");

                }
            }
            catch(Exception ex)
            {
                MamaBot.GlobalShared.Vars.Logger.LogError("Exception occured on the OrderSocketHandler : " + ex.Message);
            }
        }

        /// <summary>
        /// Receive Orderbook Update coming for the subscription
        /// </summary>
        /// <param name="OrderbookLevel"></param>
        private void OrderBookHandler(BinanceOrderBook OrderbookLevel)
        {
            try
            {
                var BestAsk = (List<BinanceOrderBookEntry>)OrderbookLevel.Asks;
                var BestBid = (List<BinanceOrderBookEntry>)OrderbookLevel.Bids;
                var Capture = BestAsk.ConvertToBook(BestBid);
                Capture.Compute();
                BookSnapshot.AddBook(Capture);

                var RounderAsk = BestAsk.Select(y => Math.Round(y.Price, 0)).ToList().ToList();
                var RounderAskVol = BestAsk.Select(y => Math.Round(y.Quantity, 5)).ToList().ToList();

                var RounderBid = BestBid.Select(y => Math.Round(y.Price, 0)).ToList().ToList();
                var RounderBidVol = BestBid.Select(y => Math.Round(y.Quantity, 5)).ToList().ToList();

                for (int i = 0; i < RounderAsk.Count; i++)
                {
                    Map.AddPrice(new PriceLayer(RounderAsk[i], RounderAskVol[i], PriceDirection.Ask));
                }
                for (int i = 0; i < RounderBid.Count; i++)
                {
                    Map.AddPrice(new PriceLayer(RounderBid[i], RounderBidVol[i], PriceDirection.Bid));
                }
                //Logger.Info(string.Format("Price : {0}", Map.Map.Last().Price));
                //Logger.Info(string.Format("Ask Quantity Price : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Ask).Last().PriceLayerRef));
                //Logger.Info(string.Format("Ask Quantity Before : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Ask).Last().QuantityBefore));
                //Logger.Info(string.Format("Ask Quantity After : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Ask).Last().QuantityAfter));
                //Logger.Info(string.Format("Bid Quantity Price : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Bid).Last().PriceLayerRef));
                //Logger.Info(string.Format("Bid Quantity Before : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Bid).Last().QuantityBefore));
                //Logger.Info(string.Format("Bid Quantity After : {0}", Map.Update.Where(y => y.TheChoice == PriceDirection.Bid).Last().QuantityAfter));
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

            }

            catch (Exception ex)
            {
                MamaBot.GlobalShared.Vars.Logger.LogError("Exception occured in the OrderBookHandler : " + ex.Message);

            }



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
                var CurrentOrder = _client.GetOpenOrders(_botConfig.Pair);
                foreach (var order in CurrentOrder.Data)
                {
                    if (order.Side == direction && order.Symbol == _botConfig.Pair)
                    {
                        _client.CancelOrder(_botConfig.Pair, order.OrderId);
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
                    _logger.LogInformation($"TransactionTime received for : {obj.Status}");
                    _logger.LogInformation($"ListOrderStatus received for : {obj.Quantity}");
            }
        }
    }
}
