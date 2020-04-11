using Binance.Net;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Spot.MarketData;
using Binance.Net.Objects.Spot.MarketStream;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trady.Analysis;
using Trady.Analysis.Extension;
using Trady.Core;

namespace MaMa.HFT.Console
{
    public class HistoGram
    {
        public List<AnalyzableTick<decimal?>> Data { get; set; }
        public AnalyzableTick<decimal?> Last { get; set; }
        public KlineInterval Interval { get; set; }
        public HistoGram(List<Candle> Source, KlineInterval period)
        {
            this.Last = Source.MacdHist(12, 26, 9)[Source.Count() - 1];
            this.Data = Source.MacdHist(12, 26, 9).ToList();
            this.Interval = period;
        }
    }

    public class CandleService
    {
        public BinanceClient client { get; set; } = new BinanceClient();
        public List<Candle> Candles { get; set; } = new List<Candle>();
        public Dictionary<KlineInterval, HistoGram> HistoGramData = new Dictionary<KlineInterval, HistoGram>();
        public string KeyPair { get; set; } = string.Empty;
        public Dictionary<KlineInterval, List<Candle>> RawCandles = new Dictionary<KlineInterval, List<Candle>>();
        public DateTime StartTime { get; set; } = DateTime.Now;

        public CandleService()
        {

        }

        public CandleService(string keypair)
        {
            this.KeyPair = keypair;
            LoadKLineData();
        }
        internal void LoadKLineData()
        {
            this.RawCandles.Clear();

            this.RawCandles.Add(KlineInterval.OneHour, TransformCandle(client.GetKlines(KeyPair, KlineInterval.OneHour, startTime: DateTime.UtcNow.AddHours(-24), endTime: DateTime.UtcNow, limit: 1000).Data));
            this.RawCandles.Add(KlineInterval.FourHour, TransformCandle(client.GetKlines(KeyPair, KlineInterval.FourHour, startTime: DateTime.UtcNow.AddHours(-24), endTime: DateTime.UtcNow, limit: 1000).Data));
            this.RawCandles.Add(KlineInterval.ThirtyMinutes, TransformCandle(client.GetKlines(KeyPair, KlineInterval.ThirtyMinutes, startTime: DateTime.UtcNow.AddHours(-24), endTime: DateTime.UtcNow, limit: 1000).Data));

            //Interpret

            try
            {
                Parallel.ForEach(this.RawCandles, (source) =>
                {
                    this.HistoGramData.Add(source.Key, new HistoGram(source.Value, source.Key));
                });

            }
            catch
            {

            }
        }
        internal List<Candle> TransformCandle(IEnumerable<BinanceKline> data)
        {
            List<Candle> OutputList = new List<Candle>();
            Parallel.ForEach(data, (source) =>
            {
                OutputList.Add(new Candle(source.CloseTime, source.Open, source.High, source.Low, source.Close, source.Volume));
            });
            return OutputList;
        }

        public void Queue_OnAddHandler(object sender, Bot.Channel.QueueItemArgs e)
        {
            var CastItem = (BinanceStreamTick)e.Item;
            this.Candles.Add(new Candle(CastItem.CloseTime, CastItem.OpenPrice, CastItem.HighPrice, CastItem.LowPrice, CastItem.LastPrice, CastItem.TotalTradedQuoteAssetVolume));
            var HistoGram = this.Candles.MacdHist(12, 26, 9)[this.Candles.Count() - 1];
            var Sma = this.Candles.Sma(7);

            var HasHistoTick = HistoGram.Tick != null;
            var hasSma7 = Sma.Last().Tick != null;

            if (HasHistoTick)
            {
                Debug.WriteLine($"HistoGram Value : {HistoGram.Tick.Value}");

            }
            if (hasSma7)
            {
                Debug.WriteLine($"Sma Value : {Sma.Last().Tick.Value}");

            }
        }

    }
}
