using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(
        TimeZone = TimeZones.EAfricaStandardTime,
        AccessRights = AccessRights.None)]
    public class GoldStraddleBotV3 : Robot
    {
        #region Slot 1 Parameters
        
        [Parameter("Slot 1 - Enable", Group = "Slot 1", DefaultValue = true)]
        public bool Slot1Enable { get; set; }
        
        [Parameter("Slot 1 - Symbol", Group = "Slot 1", DefaultValue = "XAUUSD")]
        public string Slot1SymbolName { get; set; }
        
        [Parameter("Slot 1 - Volume (Lots)", Group = "Slot 1", DefaultValue = 0.01, MinValue = 0.01)]
        public double Slot1Volume { get; set; }
        
        [Parameter("Slot 1 - Stop Loss (USD)", Group = "Slot 1", DefaultValue = 100)]
        public double Slot1StopLossUSD { get; set; }
        
        [Parameter("Slot 1 - Take Profit (USD)", Group = "Slot 1", DefaultValue = 400)]
        public double Slot1TakeProfitUSD { get; set; }
        
        [Parameter("Slot 1 - Trade Time 1", Group = "Slot 1", DefaultValue = "09:30:00")]
        public string Slot1TradeTime1 { get; set; }
        
        [Parameter("Slot 1 - Trade Time 2", Group = "Slot 1", DefaultValue = "")]
        public string Slot1TradeTime2 { get; set; }
        
        [Parameter("Slot 1 - Monday", Group = "Slot 1 - Days", DefaultValue = true)]
        public bool Slot1Monday { get; set; }
        
        [Parameter("Slot 1 - Tuesday", Group = "Slot 1 - Days", DefaultValue = true)]
        public bool Slot1Tuesday { get; set; }
        
        [Parameter("Slot 1 - Wednesday", Group = "Slot 1 - Days", DefaultValue = true)]
        public bool Slot1Wednesday { get; set; }
        
        [Parameter("Slot 1 - Thursday", Group = "Slot 1 - Days", DefaultValue = true)]
        public bool Slot1Thursday { get; set; }
        
        [Parameter("Slot 1 - Friday", Group = "Slot 1 - Days", DefaultValue = true)]
        public bool Slot1Friday { get; set; }
        
        [Parameter("Slot 1 - Saturday", Group = "Slot 1 - Days", DefaultValue = false)]
        public bool Slot1Saturday { get; set; }
        
        [Parameter("Slot 1 - Sunday", Group = "Slot 1 - Days", DefaultValue = false)]
        public bool Slot1Sunday { get; set; }
        
        #endregion
        
        #region Slot 2 Parameters
        
        [Parameter("Slot 2 - Enable", Group = "Slot 2", DefaultValue = true)]
        public bool Slot2Enable { get; set; }
        
        [Parameter("Slot 2 - Symbol", Group = "Slot 2", DefaultValue = "US30")]
        public string Slot2SymbolName { get; set; }
        
        [Parameter("Slot 2 - Volume (Lots)", Group = "Slot 2", DefaultValue = 0.01, MinValue = 0.01)]
        public double Slot2Volume { get; set; }
        
        [Parameter("Slot 2 - Stop Loss (USD)", Group = "Slot 2", DefaultValue = 100)]
        public double Slot2StopLossUSD { get; set; }
        
        [Parameter("Slot 2 - Take Profit (USD)", Group = "Slot 2", DefaultValue = 400)]
        public double Slot2TakeProfitUSD { get; set; }
        
        [Parameter("Slot 2 - Trade Time 1", Group = "Slot 2", DefaultValue = "14:59:57")]
        public string Slot2TradeTime1 { get; set; }
        
        [Parameter("Slot 2 - Trade Time 2", Group = "Slot 2", DefaultValue = "")]
        public string Slot2TradeTime2 { get; set; }
        
        [Parameter("Slot 2 - Monday", Group = "Slot 2 - Days", DefaultValue = true)]
        public bool Slot2Monday { get; set; }
        
        [Parameter("Slot 2 - Tuesday", Group = "Slot 2 - Days", DefaultValue = true)]
        public bool Slot2Tuesday { get; set; }
        
        [Parameter("Slot 2 - Wednesday", Group = "Slot 2 - Days", DefaultValue = true)]
        public bool Slot2Wednesday { get; set; }
        
        [Parameter("Slot 2 - Thursday", Group = "Slot 2 - Days", DefaultValue = true)]
        public bool Slot2Thursday { get; set; }
        
        [Parameter("Slot 2 - Friday", Group = "Slot 2 - Days", DefaultValue = true)]
        public bool Slot2Friday { get; set; }
        
        [Parameter("Slot 2 - Saturday", Group = "Slot 2 - Days", DefaultValue = false)]
        public bool Slot2Saturday { get; set; }
        
        [Parameter("Slot 2 - Sunday", Group = "Slot 2 - Days", DefaultValue = false)]
        public bool Slot2Sunday { get; set; }
        
        #endregion
        
        #region Global Parameters
        
        [Parameter("Debug Mode", Group = "Global", DefaultValue = true)]
        public bool DebugMode { get; set; }
        
        #endregion
        
        #region Private Constants
        
        private const string Slot1Label = "GoldStraddleBot_V3_Slot1";
        private const string Slot2Label = "GoldStraddleBot_V3_Slot2";
        
        #endregion
        
        #region Private Variables
        
        private DateTime _slot1LastTradeCandle;
        private DateTime _slot2LastTradeCandle;
        private Symbol _slot1Symbol;
        private Symbol _slot2Symbol;
        private TimeSpan _slot1Time1;
        private TimeSpan _slot1Time2;
        private TimeSpan _slot2Time1;
        private TimeSpan _slot2Time2;
        
        #endregion
        
        #region Lifecycle Methods
        
        protected override void OnStart()
        {
            // Initialize symbols
            _slot1Symbol = Symbols.GetSymbol(Slot1SymbolName);
            _slot2Symbol = Symbols.GetSymbol(Slot2SymbolName);
            
            // Parse trade times
            _slot1Time1 = ParseTime(Slot1TradeTime1);
            _slot1Time2 = ParseTime(Slot1TradeTime2);
            _slot2Time1 = ParseTime(Slot2TradeTime1);
            _slot2Time2 = ParseTime(Slot2TradeTime2);
            
            // Log startup message
            LogDebug("GoldStraddleBot V3 Started");
            LogDebug($"Slot1: {Slot1SymbolName} (Enabled: {Slot1Enable})");
            LogDebug($"Slot2: {Slot2SymbolName} (Enabled: {Slot2Enable})");
            
            // Validate symbols
            if (Slot1Enable && _slot1Symbol == null)
            {
                Print($"ERROR: Slot 1 symbol '{Slot1SymbolName}' not found!");
                Slot1Enable = false;
            }
            
            if (Slot2Enable && _slot2Symbol == null)
            {
                Print($"ERROR: Slot 2 symbol '{Slot2SymbolName}' not found!");
                Slot2Enable = false;
            }
        }
        
        protected override void OnTick()
        {
            if (Slot1Enable && _slot1Symbol != null)
            {
                CheckSlot(Slot1Label, _slot1Symbol, Slot1Volume, Slot1StopLossUSD, Slot1TakeProfitUSD, 
                         ref _slot1LastTradeCandle, _slot1Time1, _slot1Time2,
                         Slot1Monday, Slot1Tuesday, Slot1Wednesday, Slot1Thursday, 
                         Slot1Friday, Slot1Saturday, Slot1Sunday);
            }
            
            if (Slot2Enable && _slot2Symbol != null)
            {
                CheckSlot(Slot2Label, _slot2Symbol, Slot2Volume, Slot2StopLossUSD, Slot2TakeProfitUSD,
                         ref _slot2LastTradeCandle, _slot2Time1, _slot2Time2,
                         Slot2Monday, Slot2Tuesday, Slot2Wednesday, Slot2Thursday,
                         Slot2Friday, Slot2Saturday, Slot2Sunday);
            }
        }
        
        #endregion
        
        #region Core Logic
        
        private void CheckSlot(string label, Symbol symbol, double volume, double stopLossUSD, double takeProfitUSD,
                              ref DateTime lastTradeCandle, TimeSpan time1, TimeSpan time2,
                              bool monday, bool tuesday, bool wednesday, bool thursday, 
                              bool friday, bool saturday, bool sunday)
        {
            try
            {
                // Check if today is a valid trading day
                if (!IsValidTradingDay(monday, tuesday, wednesday, thursday, friday, saturday, sunday))
                {
                    LogDebug($"{label} - Skipping: Not a valid trading day");
                    return;
                }
                
                // Check for pending orders
                if (HasPendingOrders(label))
                {
                    LogDebug($"{label} - Skipping: Already has pending orders");
                    return;
                }
                
                // Get current time
                DateTime currentTime = Server.Time;
                TimeSpan currentTimeOfDay = currentTime.TimeOfDay;
                DateTime currentCandle = GetCurrentCandleStart(symbol);
                
                // Check if we already traded in this candle
                if (lastTradeCandle == currentCandle)
                {
                    LogDebug($"{label} - Skipping: Already traded in current candle");
                    return;
                }
                
                // Check time1
                if (time1 != TimeSpan.Zero && IsTimeMatch(currentTimeOfDay, time1))
                {
                    LogDebug($"{label} - Trade Time 1 detected at {time1}");
                    OpenStraddle(symbol, volume, stopLossUSD, takeProfitUSD, label);
                    lastTradeCandle = currentCandle;
                    return;
                }
                
                // Check time2
                if (time2 != TimeSpan.Zero && IsTimeMatch(currentTimeOfDay, time2))
                {
                    LogDebug($"{label} - Trade Time 2 detected at {time2}");
                    OpenStraddle(symbol, volume, stopLossUSD, takeProfitUSD, label);
                    lastTradeCandle = currentCandle;
                    return;
                }
            }
            catch (Exception ex)
            {
                Print($"ERROR in {label}: {ex.Message}");
            }
        }
        
        private void OpenStraddle(Symbol symbol, double volumeLots, double stopLossUSD, double takeProfitUSD, string label)
        {
            try
            {
                // Convert USD to pips
                double stopLossPips = ConvertUsdToPips(symbol, volumeLots, stopLossUSD);
                double takeProfitPips = ConvertUsdToPips(symbol, volumeLots, takeProfitUSD);
                
                LogDebug($"{label} - Opening Straddle on {symbol.Name}");
                LogDebug($"{label} - Volume: {volumeLots} lots");
                LogDebug($"{label} - SL: ${stopLossUSD} → {stopLossPips} pips");
                LogDebug($"{label} - TP: ${takeProfitUSD} → {takeProfitPips} pips");
                
                // Open BUY order
                TradeResult buyResult = ExecuteMarketOrder(
                    TradeType.Buy,
                    symbol.Name,
                    volumeLots,
                    $"{label}_BUY",
                    stopLossPips,
                    takeProfitPips);
                
                if (buyResult.IsSuccessful)
                {
                    LogDebug($"{label} - BUY order successful. ID: {buyResult.Position.Id}");
                }
                else
                {
                    Print($"ERROR: {label} - BUY order failed: {buyResult.Error}");
                }
                
                // Open SELL order
                TradeResult sellResult = ExecuteMarketOrder(
                    TradeType.Sell,
                    symbol.Name,
                    volumeLots,
                    $"{label}_SELL",
                    stopLossPips,
                    takeProfitPips);
                
                if (sellResult.IsSuccessful)
                {
                    LogDebug($"{label} - SELL order successful. ID: {sellResult.Position.Id}");
                }
                else
                {
                    Print($"ERROR: {label} - SELL order failed: {sellResult.Error}");
                }
            }
            catch (Exception ex)
            {
                Print($"ERROR: {label} - Failed to open straddle: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private double ConvertUsdToPips(Symbol symbol, double volumeLots, double usdAmount)
        {
            try
            {
                // Get pip value in account currency
                double pipValue = symbol.PipValue * volumeLots;
                
                if (pipValue == 0)
                {
                    Print($"WARNING: Pip value is zero for {symbol.Name}, using default value");
                    pipValue = 1.0;
                }
                
                // Calculate pips needed to achieve the USD amount
                double pips = usdAmount / pipValue;
                
                // Ensure minimum pip distance
                double minPips = symbol.PipSize * 10; // 10 pips minimum
                
                return Math.Max(pips, minPips);
            }
            catch (Exception ex)
            {
                Print($"ERROR: Failed to convert USD to pips: {ex.Message}");
                return 100; // Default fallback
            }
        }
        
        private bool IsValidTradingDay(bool monday, bool tuesday, bool wednesday, bool thursday, 
                                       bool friday, bool saturday, bool sunday)
        {
            DayOfWeek currentDay = Server.Time.DayOfWeek;
            
            switch (currentDay)
            {
                case DayOfWeek.Monday: return monday;
                case DayOfWeek.Tuesday: return tuesday;
                case DayOfWeek.Wednesday: return wednesday;
                case DayOfWeek.Thursday: return thursday;
                case DayOfWeek.Friday: return friday;
                case DayOfWeek.Saturday: return saturday;
                case DayOfWeek.Sunday: return sunday;
                default: return false;
            }
        }
        
        private bool IsTimeMatch(TimeSpan currentTime, TimeSpan targetTime)
        {
            // Allow a small window (5 seconds) for tick precision
            TimeSpan difference = currentTime - targetTime;
            return Math.Abs(difference.TotalSeconds) < 5;
        }
        
        private TimeSpan ParseTime(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return TimeSpan.Zero;
                
            if (TimeSpan.TryParse(timeString, out TimeSpan result))
                return result;
                
            Print($"WARNING: Invalid time format '{timeString}'. Use HH:mm:ss");
            return TimeSpan.Zero;
        }
        
        private DateTime GetCurrentCandleStart(Symbol symbol)
        {
            // Get the start of the current 1-hour candle
            DateTime now = Server.Time;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        }
        
        private bool HasPendingOrders(string label)
        {
            // Check if there are any open positions with this label
            return Positions.Any(p => p.Label.StartsWith(label) && p.State == TradeState.Active);
        }
        
        private void LogDebug(string message)
        {
            if (DebugMode)
            {
                Print($"[DEBUG] {message}");
            }
        }
        
        #endregion
    }
}
