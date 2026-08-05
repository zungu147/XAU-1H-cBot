using System;
using System.Linq;
using System.Diagnostics;
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

        [Parameter("Symbol Name", Group = "Slot 1 Settings", DefaultValue = "XAUUSD")]
        public string Slot1SymbolName { get; set; }

        [Parameter("Volume (Lots)", Group = "Slot 1 Settings", DefaultValue = 0.01)]
        public double Slot1Volume { get; set; }

        [Parameter("Stop Loss (USD)", Group = "Slot 1 Settings", DefaultValue = 100)]
        public double Slot1StopLossUSD { get; set; }

        [Parameter("Take Profit (USD)", Group = "Slot 1 Settings", DefaultValue = 400)]
        public double Slot1TakeProfitUSD { get; set; }

        [Parameter("Trade Time 1 (HH:mm:ss)", Group = "Slot 1 Settings", DefaultValue = "09:30:00")]
        public string Slot1TradeTime1 { get; set; }

        [Parameter("Trade Time 2 (HH:mm:ss)", Group = "Slot 1 Settings", DefaultValue = "14:59:57")]
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

        [Parameter("Symbol Name", Group = "Slot 2 Settings", DefaultValue = "US30")]
        public string Slot2SymbolName { get; set; }

        [Parameter("Volume (Lots)", Group = "Slot 2 Settings", DefaultValue = 0.01)]
        public double Slot2Volume { get; set; }

        [Parameter("Stop Loss (USD)", Group = "Slot 2 Settings", DefaultValue = 100)]
        public double Slot2StopLossUSD { get; set; }

        [Parameter("Take Profit (USD)", Group = "Slot 2 Settings", DefaultValue = 400)]
        public double Slot2TakeProfitUSD { get; set; }

        [Parameter("Trade Time 1 (HH:mm:ss)", Group = "Slot 2 Settings", DefaultValue = "09:30:00")]
        public string Slot2TradeTime1 { get; set; }

        [Parameter("Trade Time 2 (HH:mm:ss)", Group = "Slot 2 Settings", DefaultValue = "14:59:57")]
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

        private Symbol _slot1Symbol;
        private Symbol _slot2Symbol;

        private TimeSpan _slot1Time1;
        private TimeSpan _slot1Time2;
        private TimeSpan _slot2Time1;
        private TimeSpan _slot2Time2;

        private DateTime _slot1LastTradeCandle;
        private DateTime _slot2LastTradeCandle;

        #endregion

        #region Start

        protected override void OnStart()
        {
            ValidateAndParseParameters();

            // Load symbols dynamically
            if (Slot1Enable)
            {
                _slot1Symbol = Symbols.GetSymbol(Slot1SymbolName);
                if (_slot1Symbol == null)
                {
                    LogError($"Slot 1 Symbol '{Slot1SymbolName}' could not be loaded!");
                    Slot1Enable = false;
                }
            }

            if (Slot2Enable)
            {
                _slot2Symbol = Symbols.GetSymbol(Slot2SymbolName);
                if (_slot2Symbol == null)
                {
                    LogError($"Slot 2 Symbol '{Slot2SymbolName}' could not be loaded!");
                    Slot2Enable = false;
                }
            }

            PrintStartupReport();
            LogInfo($"{BotVersion} Started");
        }

        #endregion

        #region Tick Execution

        protected override void OnTick()
        {
            DateTime nowServer = Server.Time;

            if (Slot1Enable && _slot1Symbol != null)
            {
                ProcessSlot(
                    1,
                    _slot1Symbol,
                    Slot1Volume,
                    Slot1StopLossUSD,
                    Slot1TakeProfitUSD,
                    _slot1Time1,
                    _slot1Time2,
                    Slot1Label,
                    ref _slot1LastTradeCandle,
                    IsTradingDaySlot1(nowServer.DayOfWeek));
            }

            if (Slot2Enable && _slot2Symbol != null)
            {
                ProcessSlot(
                    2,
                    _slot2Symbol,
                    Slot2Volume,
                    Slot2StopLossUSD,
                    Slot2TakeProfitUSD,
                    _slot2Time1,
                    _slot2Time2,
                    Slot2Label,
                    ref _slot2LastTradeCandle,
                    IsTradingDaySlot2(nowServer.DayOfWeek));
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

            bool isTime1Trigger = currentTimeOfDay >= time1 && currentTimeOfDay <= time1.Add(TimeSpan.FromSeconds(30));
            bool isTime2Trigger = currentTimeOfDay >= time2 && currentTimeOfDay <= time2.Add(TimeSpan.FromSeconds(30));

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
            if (!TimeSpan.TryParse(Slot1TradeTime1, out _slot1Time1))
                LogError("Slot 1 Trade Time 1 format is invalid.");

            if (!TimeSpan.TryParse(Slot1TradeTime2, out _slot1Time2))
                LogError("Slot 1 Trade Time 2 format is invalid.");

            if (!TimeSpan.TryParse(Slot2TradeTime1, out _slot2Time1))
                LogError("Slot 2 Trade Time 1 format is invalid.");

            if (!TimeSpan.TryParse(Slot2TradeTime2, out _slot2Time2))
                LogError("Slot 2 Trade Time 2 format is invalid.");

            LogInfo("Parameters parsed and validated.");
        }

        #endregion

        #region Logging & Reporting

        private void PrintStartupReport()
        {
            Print("========================================");
            Print($"GoldStraddleBot {BotVersion} Started");
            Print($"Slot 1 Enabled: {Slot1Enable} | Symbol: {Slot1SymbolName} | Times: {Slot1TradeTime1}, {Slot1TradeTime2}");
            Print($"Slot 2 Enabled: {Slot2Enable} | Symbol: {Slot2SymbolName} | Times: {Slot2TradeTime1}, {Slot2TradeTime2}");
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
