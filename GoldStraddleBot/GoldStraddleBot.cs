using System;
using cAlgo.API;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.EAfricaStandardTime, AccessRights = AccessRights.None)]
    public class GoldStraddleBot : Robot
    {

        #region Parameters

        [Parameter("Volume (Lots)", DefaultValue = 0.01)]
        public double Volume { get; set; }

        [Parameter("Stop Loss (Pips)", DefaultValue = 100)]
        public double StopLoss { get; set; }

        [Parameter("Take Profit (Pips)", DefaultValue = 400)]
        public double TakeProfit { get; set; }


        [Parameter("Trade Time 1 (HH:mm:ss)", DefaultValue = "09:30:00")]
        public string TradeTime1 { get; set; }

        [Parameter("Trade Time 2 (HH:mm:ss)", DefaultValue = "14:59:57")]
        public string TradeTime2 { get; set; }

        [Parameter("Trade Time 3 (HH:mm:ss)", DefaultValue = "21:00:00")]
        public string TradeTime3 { get; set; }


        [Parameter("Trade Monday", DefaultValue = true)]
        public bool TradeMonday { get; set; }

        [Parameter("Trade Tuesday", DefaultValue = true)]
        public bool TradeTuesday { get; set; }

        [Parameter("Trade Wednesday", DefaultValue = true)]
        public bool TradeWednesday { get; set; }

        [Parameter("Trade Thursday", DefaultValue = true)]
        public bool TradeThursday { get; set; }

        [Parameter("Trade Friday", DefaultValue = true)]
        public bool TradeFriday { get; set; }


        [Parameter("Debug Mode", DefaultValue = false)]
        public bool DebugMode { get; set; }

        #endregion


        #region Variables

        private TimeSpan _time1;
        private TimeSpan _time2;
        private TimeSpan _time3;

        private bool _executedTime1;
        private bool _executedTime2;
        private bool _executedTime3;

        private DateTime _currentDate;

        #endregion


        #region Start

        protected override void OnStart()
        {
            LogInfo("GoldStraddleBot v2.0-alpha3 Started");

            ValidateParameters();

            _currentDate = Server.Time.Date;

            Timer.Start(TimeSpan.FromMilliseconds(200));

            LogInfo($"Server Time: {Server.Time}");
        }

        #endregion


        #region Timer

        protected override void OnTimer()
        {
            ResetDailyStatus();

            if (!IsTradingDay())
                return;


            CheckTradeTime(_time1, ref _executedTime1, "Time 1");

            CheckTradeTime(_time2, ref _executedTime2, "Time 2");

            CheckTradeTime(_time3, ref _executedTime3, "Time 3");


            Debug($"Checking time: {Server.Time:HH:mm:ss}");
        }

        #endregion


        #region Scheduler

        private void CheckTradeTime(TimeSpan tradeTime, ref bool executed, string name)
        {
            if (executed)
                return;


            TimeSpan current = Server.Time.TimeOfDay;


            if (current >= tradeTime &&
                current < tradeTime.Add(TimeSpan.FromSeconds(1)))
            {
                LogInfo($"{name} triggered at {Server.Time:HH:mm:ss}");

                ExecuteStraddle();

                executed = true;
            }
        }

        #endregion


        #region Trading

        private void ExecuteStraddle()
        {
            double volumeInUnits =
                Symbol.QuantityToVolumeInUnits(Volume);


            DateTime buyStart = Server.Time;


            var buyResult = ExecuteMarketOrder(
                TradeType.Buy,
                SymbolName,
                volumeInUnits,
                "GoldStraddle_BUY",
                StopLoss,
                TakeProfit);


            DateTime buyEnd = Server.Time;


            var sellResult = ExecuteMarketOrder(
                TradeType.Sell,
                SymbolName,
                volumeInUnits,
                "GoldStraddle_SELL",
                StopLoss,
                TakeProfit);


            DateTime sellEnd = Server.Time;


            if (buyResult.IsSuccessful)
            {
                LogSuccess(
                    $"BUY executed | Price: {buyResult.Position.EntryPrice}");
            }
            else
            {
                LogError(
                    $"BUY failed | {buyResult.Error}");
            }


            if (sellResult.IsSuccessful)
            {
                LogSuccess(
                    $"SELL executed | Price: {sellResult.Position.EntryPrice}");
            }
            else
            {
                LogError(
                    $"SELL failed | {sellResult.Error}");
            }


            double buyTime =
                (buyEnd - buyStart).TotalMilliseconds;

            double sellTime =
                (sellEnd - buyEnd).TotalMilliseconds;


            LogInfo(
                $"Execution timing | BUY: {buyTime} ms | SELL: {sellTime} ms");
        }

        #endregion


        #region Validation

        private void ValidateParameters()
        {
            if (!TimeSpan.TryParse(TradeTime1, out _time1))
                LogError("Trade Time 1 invalid");

            if (!TimeSpan.TryParse(TradeTime2, out _time2))
                LogError("Trade Time 2 invalid");

            if (!TimeSpan.TryParse(TradeTime3, out _time3))
                LogError("Trade Time 3 invalid");


            if (Volume <= 0)
                LogError("Invalid lot size");

            if (StopLoss <= 0)
                LogError("Invalid Stop Loss");

            if (TakeProfit <= 0)
                LogError("Invalid Take Profit");


            LogInfo("Parameters validated");
        }

        #endregion


        #region Trading Days

        private bool IsTradingDay()
        {
            switch (Server.Time.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return TradeMonday;

                case DayOfWeek.Tuesday:
                    return TradeTuesday;

                case DayOfWeek.Wednesday:
                    return TradeWednesday;

                case DayOfWeek.Thursday:
                    return TradeThursday;

                case DayOfWeek.Friday:
                    return TradeFriday;

                default:
                    return false;
            }
        }


        private void ResetDailyStatus()
        {
            if (_currentDate != Server.Time.Date)
            {
                _executedTime1 = false;
                _executedTime2 = false;
                _executedTime3 = false;

                _currentDate = Server.Time.Date;

                LogInfo("Daily execution reset");
            }
        }

        #endregion


        #region Logging

        private void LogInfo(string message)
        {
            Print("[INFO] " + message);
        }


        private void LogSuccess(string message)
        {
            Print("[SUCCESS] " + message);
        }


        private void LogError(string message)
        {
            Print("[ERROR] " + message);
        }


        private void Debug(string message)
        {
            if (DebugMode)
                Print("[DEBUG] " + message);
        }

        #endregion

    }
}
