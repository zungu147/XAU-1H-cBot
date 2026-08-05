using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.EAfricaStandardTime, AccessRights = AccessRights.None)]
    public class GoldStraddleBot : Robot
    {
        private const string BotVersion = "v3.0";
        private const string Slot1Label = "GoldStraddleBot_V3_Slot1";
        private const string Slot2Label = "GoldStraddleBot_V3_Slot2";

        #region Slot 1 Parameters

        [Parameter("Enable Slot 1", Group = "Slot 1 Settings", DefaultValue = true)]
        public bool Slot1Enable { get; set; }

        [Parameter("Symbol Names (comma-separated)", Group = "Slot 1 Settings", DefaultValue = "XAUUSD")]
        public string Slot1SymbolName { get; set; }

        [Parameter("Volume (Lots) (comma-separated or single)", Group = "Slot 1 Settings", DefaultValue = 0.01)]
        public string Slot1Volume { get; set; }

        [Parameter("Stop Loss (USD) (comma-separated or single)", Group = "Slot 1 Settings", DefaultValue = 100)]
        public string Slot1StopLossUSD { get; set; }

        [Parameter("Take Profit (USD) (comma-separated or single)", Group = "Slot 1 Settings", DefaultValue = 400)]
        public string Slot1TakeProfitUSD { get; set; }

        [Parameter("Trade Time 1 (HH:mm:ss) (comma-separated or single)", Group = "Slot 1 Settings", DefaultValue = "09:30:00")]
        public string Slot1TradeTime1 { get; set; }

        [Parameter("Trade Time 2 (HH:mm:ss) (comma-separated or single)", Group = "Slot 1 Settings", DefaultValue = "14:59:57")]
        public string Slot1TradeTime2 { get; set; }

        [Parameter("Trade Monday", Group = "Slot 1 Days", DefaultValue = true)]
        public bool Slot1Monday { get; set; }

        [Parameter("Trade Tuesday", Group = "Slot 1 Days", DefaultValue = true)]
        public bool Slot1Tuesday { get; set; }

        [Parameter("Trade Wednesday", Group = "Slot 1 Days", DefaultValue = true)]
        public bool Slot1Wednesday { get; set; }

        [Parameter("Trade Thursday", Group = "Slot 1 Days", DefaultValue = true)]
        public bool Slot1Thursday { get; set; }

        [Parameter("Trade Friday", Group = "Slot 1 Days", DefaultValue = true)]
        public bool Slot1Friday { get; set; }

        [Parameter("Trade Saturday", Group = "Slot 1 Days", DefaultValue = false)]
        public bool Slot1Saturday { get; set; }

        [Parameter("Trade Sunday", Group = "Slot 1 Days", DefaultValue = false)]
        public bool Slot1Sunday { get; set; }

        #endregion

        #region Slot 2 Parameters

        [Parameter("Enable Slot 2", Group = "Slot 2 Settings", DefaultValue = true)]
        public bool Slot2Enable { get; set; }

        [Parameter("Symbol Names (comma-separated)", Group = "Slot 2 Settings", DefaultValue = "US30")]
        public string Slot2SymbolName { get; set; }

        [Parameter("Volume (Lots) (comma-separated or single)", Group = "Slot 2 Settings", DefaultValue = 0.01)]
        public string Slot2Volume { get; set; }

        [Parameter("Stop Loss (USD) (comma-separated or single)", Group = "Slot 2 Settings", DefaultValue = 100)]
        public string Slot2StopLossUSD { get; set; }

        [Parameter("Take Profit (USD) (comma-separated or single)", Group = "Slot 2 Settings", DefaultValue = 400)]
        public string Slot2TakeProfitUSD { get; set; }

        [Parameter("Trade Time 1 (HH:mm:ss) (comma-separated or single)", Group = "Slot 2 Settings", DefaultValue = "09:30:00")]
        public string Slot2TradeTime1 { get; set; }

        [Parameter("Trade Time 2 (HH:mm:ss) (comma-separated or single)", Group = "Slot 2 Settings", DefaultValue = "14:59:57")]
        public string Slot2TradeTime2 { get; set; }

        [Parameter("Trade Monday", Group = "Slot 2 Days", DefaultValue = true)]
        public bool Slot2Monday { get; set; }

        [Parameter("Trade Tuesday", Group = "Slot 2 Days", DefaultValue = true)]
        public bool Slot2Tuesday { get; set; }

        [Parameter("Trade Wednesday", Group = "Slot 2 Days", DefaultValue = true)]
        public bool Slot2Wednesday { get; set; }

        [Parameter("Trade Thursday", Group = "Slot 2 Days", DefaultValue = true)]
        public bool Slot2Thursday { get; set; }

        [Parameter("Trade Friday", Group = "Slot 2 Days", DefaultValue = true)]
        public bool Slot2Friday { get; set; }

        [Parameter("Trade Saturday", Group = "Slot 2 Days", DefaultValue = false)]
        public bool Slot2Saturday { get; set; }

        [Parameter("Trade Sunday", Group = "Slot 2 Days", DefaultValue = false)]
        public bool Slot2Sunday { get; set; }

        #endregion

        #region Global Parameters

        [Parameter("Debug Mode", Group = "Global Settings", DefaultValue = true)]
        public bool DebugMode { get; set; }

        #endregion

        #region Global Variables

        private List<Symbol> _slot1Symbols = new List<Symbol>();
        private List<Symbol> _slot2Symbols = new List<Symbol>();

        private List<double> _slot1Volumes = new List<double>();
        private List<double> _slot2Volumes = new List<double>();

        private List<double> _slot1StopLossUSDList = new List<double>();
        private List<double> _slot1TakeProfitUSDList = new List<double>();

        private List<double> _slot2StopLossUSDList = new List<double>();
        private List<double> _slot2TakeProfitUSDList = new List<double>();

        private List<TimeSpan> _slot1Time1List = new List<TimeSpan>();
        private List<TimeSpan> _slot1Time2List = new List<TimeSpan>();

        private List<TimeSpan> _slot2Time1List = new List<TimeSpan>();
        private List<TimeSpan> _slot2Time2List = new List<TimeSpan>();

        private List<DateTime> _slot1LastTradeCandles = new List<DateTime>();
        private List<DateTime> _slot2LastTradeCandles = new List<DateTime>();

        #endregion

        #region Start

        protected override void OnStart()
        {
            ValidateAndParseParameters();

            // Load symbols dynamically for slot 1
            if (Slot1Enable && _slot1Symbols.Count == 0)
            {
                var names = SplitAndTrim(Slot1SymbolName);
                for (int i = 0; i < names.Count; i++)
                {
                    var s = Symbols.GetSymbol(names[i]);
                    if (s == null)
                    {
                        LogError($"Slot 1 Symbol '{names[i]}' could not be loaded!");
                    }
                    else
                    {
                        _slot1Symbols.Add(s);
                    }
                }

                // initialize last trade candles for each configured symbol
                _slot1LastTradeCandles = Enumerable.Repeat(DateTime.MinValue, _slot1Symbols.Count).ToList();
            }

            // Load symbols dynamically for slot 2
            if (Slot2Enable && _slot2Symbols.Count == 0)
            {
                var names = SplitAndTrim(Slot2SymbolName);
                for (int i = 0; i < names.Count; i++)
                {
                    var s = Symbols.GetSymbol(names[i]);
                    if (s == null)
                    {
                        LogError($"Slot 2 Symbol '{names[i]}' could not be loaded!");
                    }
                    else
                    {
                        _slot2Symbols.Add(s);
                    }
                }

                // initialize last trade candles for each configured symbol
                _slot2LastTradeCandles = Enumerable.Repeat(DateTime.MinValue, _slot2Symbols.Count).ToList();
            }

            PrintStartupReport();
            LogInfo($"{BotVersion} Started");
        }

        #endregion

        #region Tick Execution

        protected override void OnTick()
        {
            DateTime nowServer = Server.Time;

            if (Slot1Enable && _slot1Symbols != null && _slot1Symbols.Count > 0)
            {
                for (int i = 0; i < _slot1Symbols.Count; i++)
                {
                    var sym = _slot1Symbols[i];
                    double vol = _slot1Volumes.ElementAtOrDefault(i);
                    double slUsd = _slot1StopLossUSDList.ElementAtOrDefault(i);
                    double tpUsd = _slot1TakeProfitUSDList.ElementAtOrDefault(i);
                    TimeSpan t1 = _slot1Time1List.ElementAtOrDefault(i);
                    TimeSpan t2 = _slot1Time2List.ElementAtOrDefault(i);

                    ProcessSlot(
                        1,
                        sym,
                        vol,
                        slUsd,
                        tpUsd,
                        t1,
                        t2,
                        $"{Slot1Label}_{sym.Name}",
                        ref _slot1LastTradeCandles[i],
                        IsTradingDaySlot1(nowServer.DayOfWeek));
                }
            }

            if (Slot2Enable && _slot2Symbols != null && _slot2Symbols.Count > 0)
            {
                for (int i = 0; i < _slot2Symbols.Count; i++)
                {
                    var sym = _slot2Symbols[i];
                    double vol = _slot2Volumes.ElementAtOrDefault(i);
                    double slUsd = _slot2StopLossUSDList.ElementAtOrDefault(i);
                    double tpUsd = _slot2TakeProfitUSDList.ElementAtOrDefault(i);
                    TimeSpan t1 = _slot2Time1List.ElementAtOrDefault(i);
                    TimeSpan t2 = _slot2Time2List.ElementAtOrDefault(i);

                    ProcessSlot(
                        2,
                        sym,
                        vol,
                        slUsd,
                        tpUsd,
                        t1,
                        t2,
                        $"{Slot2Label}_{sym.Name}",
                        ref _slot2LastTradeCandles[i],
                        IsTradingDaySlot2(nowServer.DayOfWeek));
                }
            }
        }

        #endregion

        #region Slot Processor Logic

        private void ProcessSlot(
            int slotNumber,
            Symbol symbol,
            double volumeLots,
            double stopLossUSD,
            double takeProfitUSD,
            TimeSpan time1,
            TimeSpan time2,
            string label,
            ref DateTime lastTradeCandle,
            bool isTradingDay)
        {
            if (!isTradingDay)
                return;

            DateTime currentH1Candle = new DateTime(Server.Time.Year, Server.Time.Month, Server.Time.Day, Server.Time.Hour, 0, 0);

            // Prevent multiple executions in the same H1 candle block
            if (lastTradeCandle == currentH1Candle)
                return;

            TimeSpan currentTimeOfDay = Server.Time.TimeOfDay;

            bool isTime1Trigger = time1 != TimeSpan.Zero && currentTimeOfDay >= time1 && currentTimeOfDay <= time1.Add(TimeSpan.FromSeconds(30));
            bool isTime2Trigger = time2 != TimeSpan.Zero && currentTimeOfDay >= time2 && currentTimeOfDay <= time2.Add(TimeSpan.FromSeconds(30));

            if (isTime1Trigger || isTime2Trigger)
            {
                Debug($"Slot {slotNumber} checking {symbol.Name} - Trade time detected at {Server.Time:HH:mm:ss.fff}");

                bool success = OpenStraddle(symbol, volumeLots, stopLossUSD, takeProfitUSD, label);

                if (success)
                {
                    lastTradeCandle = currentH1Candle;
                }
            }
        }

        #endregion

        #region Straddle Execution

        private bool OpenStraddle(Symbol symbol, double volumeLots, double stopLossUSD, double takeProfitUSD, string label)
        {
            double volumeUnits = symbol.QuantityToVolumeInUnits(volumeLots);

            double stopLossPips = ConvertUsdToPips(symbol, volumeLots, stopLossUSD);
            double takeProfitPips = ConvertUsdToPips(symbol, volumeLots, takeProfitUSD);

            if (stopLossPips <= 0 || takeProfitPips <= 0)
            {
                LogError($"Invalid USD-to-Pip conversion for {symbol.Name}. Execution aborted.");
                return false;
            }

            Debug($"Executing Straddle for {symbol.Name} | Vol: {volumeLots} | SL Pips: {stopLossPips} | TP Pips: {takeProfitPips}");

            Stopwatch timer = Stopwatch.StartNew();

            // Execute Buy Order
            Debug("Opening BUY");
            var buyResult = ExecuteMarketOrder(
                TradeType.Buy,
                symbol.Name,
                volumeUnits,
                $"{label}_BUY",
                stopLossPips,
                takeProfitPips);

            double buyTime = timer.Elapsed.TotalMilliseconds;

            // Execute Sell Order
            Debug("Opening SELL");
            var sellResult = ExecuteMarketOrder(
                TradeType.Sell,
                symbol.Name,
                volumeUnits,
                $"{label}_SELL",
                stopLossPips,
                takeProfitPips);

            double sellTime = timer.Elapsed.TotalMilliseconds;
            timer.Stop();

            // Check Success
            if (buyResult.IsSuccessful)
            {
                LogSuccess($"BUY successful | {symbol.Name} | Entry: {buyResult.Position.EntryPrice}");
            }
            else
            {
                LogError($"BUY failed: {buyResult.Error}");
            }

            if (sellResult.IsSuccessful)
            {
                LogSuccess($"SELL successful | {symbol.Name} | Entry: {sellResult.Position.EntryPrice}");
            }
            else
            {
                LogError($"SELL failed: {sellResult.Error}");
            }

            Debug($"Execution gap: {sellTime - buyTime:F2} ms");

            return buyResult.IsSuccessful || sellResult.IsSuccessful;
        }

        #endregion

        #region USD to Pips System

        private double ConvertUsdToPips(Symbol symbol, double volumeLots, double usdAmount)
        {
            if (usdAmount <= 0 || volumeLots <= 0 || symbol == null)
                return 0;

            double volumeUnits = symbol.QuantityToVolumeInUnits(volumeLots);

            // Pip Value represents total monetary change per pip movement for the configured position size
            double pipValue = symbol.PipValue * volumeUnits;

            if (pipValue <= 0)
                return 0;

            double pips = usdAmount / pipValue;

            Debug($"[USD Convert] Symbol: {symbol.Name} | USD: ${usdAmount} | Volume: {volumeLots} | Calculated Pips: {pips:F1}");

            return Math.Round(pips, 1);
        }

        #endregion

        #region Trading Day Filters

        private bool IsTradingDaySlot1(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => Slot1Monday,
                DayOfWeek.Tuesday => Slot1Tuesday,
                DayOfWeek.Wednesday => Slot1Wednesday,
                DayOfWeek.Thursday => Slot1Thursday,
                DayOfWeek.Friday => Slot1Friday,
                DayOfWeek.Saturday => Slot1Saturday,
                DayOfWeek.Sunday => Slot1Sunday,
                _ => false
            };
        }

        private bool IsTradingDaySlot2(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => Slot2Monday,
                DayOfWeek.Tuesday => Slot2Tuesday,
                DayOfWeek.Wednesday => Slot2Wednesday,
                DayOfWeek.Thursday => Slot2Thursday,
                DayOfWeek.Friday => Slot2Friday,
                DayOfWeek.Saturday => Slot2Saturday,
                DayOfWeek.Sunday => Slot2Sunday,
                _ => false
            };
        }

        #endregion

        #region Validation & Setup

        private void ValidateAndParseParameters()
        {
            // Parse slot 1 lists
            var slot1Names = SplitAndTrim(Slot1SymbolName);
            var slot1Vols = ParseDoublesFromString(Slot1Volume);
            var slot1Sls = ParseDoublesFromString(Slot1StopLossUSD);
            var slot1Tps = ParseDoublesFromString(Slot1TakeProfitUSD);
            var slot1T1 = ParseTimeSpansFromString(Slot1TradeTime1);
            var slot1T2 = ParseTimeSpansFromString(Slot1TradeTime2);

            AlignListsToCount(slot1Names.Count, ref slot1Vols, ref slot1Sls, ref slot1Tps, ref slot1T1, ref slot1T2, "Slot 1");

            _slot1Volumes = slot1Vols;
            _slot1StopLossUSDList = slot1Sls;
            _slot1TakeProfitUSDList = slot1Tps;
            _slot1Time1List = slot1T1;
            _slot1Time2List = slot1T2;

            // Parse slot 2 lists
            var slot2Names = SplitAndTrim(Slot2SymbolName);
            var slot2Vols = ParseDoublesFromString(Slot2Volume);
            var slot2Sls = ParseDoublesFromString(Slot2StopLossUSD);
            var slot2Tps = ParseDoublesFromString(Slot2TakeProfitUSD);
            var slot2T1 = ParseTimeSpansFromString(Slot2TradeTime1);
            var slot2T2 = ParseTimeSpansFromString(Slot2TradeTime2);

            AlignListsToCount(slot2Names.Count, ref slot2Vols, ref slot2Sls, ref slot2Tps, ref slot2T1, ref slot2T2, "Slot 2");

            _slot2Volumes = slot2Vols;
            _slot2StopLossUSDList = slot2Sls;
            _slot2TakeProfitUSDList = slot2Tps;
            _slot2Time1List = slot2T1;
            _slot2Time2List = slot2T2;

            LogInfo("Parameters parsed and validated.");
        }

        private List<string> SplitAndTrim(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            var parts = input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            return parts;
        }

        private List<double> ParseDoublesFromString(string input)
        {
            var list = new List<double>();

            if (string.IsNullOrWhiteSpace(input))
                return list;

            // allow single number like "0.01" or comma-separated
            var parts = input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p));

            foreach (var p in parts)
            {
                if (double.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out double v))
                    list.Add(v);
                else
                    LogError($"Failed to parse number '{p}'");
            }

            return list;
        }

        private List<TimeSpan> ParseTimeSpansFromString(string input)
        {
            var list = new List<TimeSpan>();

            if (string.IsNullOrWhiteSpace(input))
                return list;

            var parts = input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p));

            foreach (var p in parts)
            {
                if (TimeSpan.TryParse(p, out TimeSpan t))
                    list.Add(t);
                else
                    LogError($"Failed to parse time '{p}'. Expected format HH:mm:ss");
            }

            return list;
        }

        private void AlignListsToCount(int count, ref List<double> vols, ref List<double> sls, ref List<double> tps, ref List<TimeSpan> t1s, ref List<TimeSpan> t2s, string slotName)
        {
            // If any list is empty, try to use a reasonable default and replicate
            if (vols.Count == 0)
                vols = Enumerable.Repeat(0.01, count).ToList();

            if (sls.Count == 0)
                sls = Enumerable.Repeat(100.0, count).ToList();

            if (tps.Count == 0)
                tps = Enumerable.Repeat(400.0, count).ToList();

            if (t1s.Count == 0)
                t1s = Enumerable.Repeat(TimeSpan.Zero, count).ToList();

            if (t2s.Count == 0)
                t2s = Enumerable.Repeat(TimeSpan.Zero, count).ToList();

            // If a list has exactly 1 entry, replicate it across all symbols
            if (vols.Count == 1 && count > 1)
                vols = Enumerable.Repeat(vols[0], count).ToList();

            if (sls.Count == 1 && count > 1)
                sls = Enumerable.Repeat(sls[0], count).ToList();

            if (tps.Count == 1 && count > 1)
                tps = Enumerable.Repeat(tps[0], count).ToList();

            if (t1s.Count == 1 && count > 1)
                t1s = Enumerable.Repeat(t1s[0], count).ToList();

            if (t2s.Count == 1 && count > 1)
                t2s = Enumerable.Repeat(t2s[0], count).ToList();

            // Final validation: all lists must be either count or greater; if mismatch, log and trim or extend with defaults
            if (vols.Count != count)
            {
                LogError($"{slotName}: Volume list length ({vols.Count}) doesn't match symbol count ({count}). Adjusting.");
                vols = ResizeDoubleList(vols, count, 0.01);
            }

            if (sls.Count != count)
            {
                LogError($"{slotName}: StopLoss list length ({sls.Count}) doesn't match symbol count ({count}). Adjusting.");
                sls = ResizeDoubleList(sls, count, 100.0);
            }

            if (tps.Count != count)
            {
                LogError($"{slotName}: TakeProfit list length ({tps.Count}) doesn't match symbol count ({count}). Adjusting.");
                tps = ResizeDoubleList(tps, count, 400.0);
            }

            if (t1s.Count != count)
            {
                LogError($"{slotName}: TradeTime1 list length ({t1s.Count}) doesn't match symbol count ({count}). Adjusting.");
                t1s = ResizeTimeSpanList(t1s, count, TimeSpan.Zero);
            }

            if (t2s.Count != count)
            {
                LogError($"{slotName}: TradeTime2 list length ({t2s.Count}) doesn't match symbol count ({count}). Adjusting.");
                t2s = ResizeTimeSpanList(t2s, count, TimeSpan.Zero);
            }
        }

        private List<double> ResizeDoubleList(List<double> list, int size, double defaultValue)
        {
            var result = new List<double>(list);
            if (result.Count > size)
                result = result.Take(size).ToList();
            while (result.Count < size)
                result.Add(defaultValue);
            return result;
        }

        private List<TimeSpan> ResizeTimeSpanList(List<TimeSpan> list, int size, TimeSpan defaultValue)
        {
            var result = new List<TimeSpan>(list);
            if (result.Count > size)
                result = result.Take(size).ToList();
            while (result.Count < size)
                result.Add(defaultValue);
            return result;
        }

        #endregion

        #region Logging & Reporting

        private void PrintStartupReport()
        {
            Print("========================================");
            Print($"GoldStraddleBot {BotVersion} Started");
            Print($"Slot 1 Enabled: {Slot1Enable} | Symbols: {Slot1SymbolName}");
            Print($"Slot 1 Volumes: {Slot1Volume} | SLs: {Slot1StopLossUSD} | TPs: {Slot1TakeProfitUSD}");
            Print($"Slot 1 Times: {Slot1TradeTime1}, {Slot1TradeTime2}");
            Print("----------------------------------------");
            Print($"Slot 2 Enabled: {Slot2Enable} | Symbols: {Slot2SymbolName}");
            Print($"Slot 2 Volumes: {Slot2Volume} | SLs: {Slot2StopLossUSD} | TPs: {Slot2TakeProfitUSD}");
            Print($"Slot 2 Times: {Slot2TradeTime1}, {Slot2TradeTime2}");
            Print("========================================");
        }

        private void LogInfo(string msg) => Print($"[INFO] {msg}");
        private void LogSuccess(string msg) => Print($"[SUCCESS] {msg}");
        private void LogError(string msg) => Print($"[ERROR] {msg}");
        private void Debug(string msg)
        {
            if (DebugMode)
                Print($"[DEBUG] {msg}");
        }

        #endregion
    }
}
