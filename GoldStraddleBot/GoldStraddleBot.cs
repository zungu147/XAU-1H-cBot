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

        private int _totalExecutions;
        private int _successfulBuys;
        private int _successfulSells;
        private int _failedOrders;

        #endregion


        #region Start

        protected override void OnStart()
        {
            _currentDate = Server.Time.Date;

            ValidateParameters();

            Timer.Start(TimeSpan.FromMilliseconds(200));

            LogInfo("GoldStraddleBot v2.0-alpha3 Started");
            LogInfo($"Symbol: {SymbolName}");
            LogInfo($"Volume: {Volume}");
            LogInfo($"SL: {StopLoss} TP: {TakeProfit}");
        }

        #endregion


        #region Timer

        protected override void OnTimer()
        {
            ResetDaily();

            if (!IsTradingDay())
                return;


            CheckTradeTime(_time1, ref _executedTime1, "Time 1");

            CheckTradeTime(_time2, ref _executedTime2, "Time 2");

            CheckTradeTime(_time3, ref _executedTime3, "Time 3");


            Debug($"Current Time: {Server.Time:HH:mm:ss}");
        }

        #endregion


        #region Scheduler

        private void CheckTradeTime(TimeSpan tradeTime, ref bool executed, string name)
        {
            if (executed)
                return;


            if (Server.Time.TimeOfDay >= tradeTime &&
                Server.Time.TimeOfDay < tradeTime.Add(TimeSpan.FromSeconds(1)))
            {
                executed = true;

                LogInfo($"{name} Triggered");

                ExecuteStraddle(name);
            }
        }

        #endregion


        #region Trading

        private void ExecuteStraddle(string trigger)
        {
            if (Volume <= 0 || StopLoss <= 0 || TakeProfit <= 0)
            {
                LogError("Invalid trading parameters");
                return;
            }


            double volumeUnits = Symbol.QuantityToVolumeInUnits(Volume);


            DateTime start = Server.Time;


            var buy = ExecuteMarketOrder(
                TradeType.Buy,
                SymbolName,
                volumeUnits,
                "GoldStraddle_BUY",
                StopLoss,
                TakeProfit);


            DateTime buyTime = Server.Time;


            var sell = ExecuteMarketOrder(
                TradeType.Sell,
                SymbolName,
                volumeUnits,
                "GoldStraddle_SELL",
                StopLoss,
                TakeProfit);


            DateTime sellTime = Server.Time;


            _totalExecutions++;


            if (buy.IsSuccessful)
                _successfulBuys++;
            else
                _failedOrders++;


            if (sell.IsSuccessful)
                _successfulSells++;
            else
                _failedOrders++;


            double delay =
                (sellTime - buyTime).TotalMilliseconds;


            if (buy.IsSuccessful && sell.IsSuccessful)
            {
                LogSuccess(
                    $"{trigger} executed | " +
                    $"BUY OK | SELL OK | " +
                    $"Delay {delay} ms");
            }
            else
            {
                LogError(
                    $"{trigger} execution problem");
            }
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

        #endregion


        #region Daily Reset

        private void ResetDaily()
        {
            if (_currentDate == Server.Time.Date)
                return;


            _currentDate = Server.Time.Date;

            _executedTime1 = false;
            _executedTime2 = false;
            _executedTime3 = false;


            PrintDailySummary();
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


        private void PrintDailySummary()
        {
            Print("====================");
            Print("DAILY SUMMARY");
            Print($"Executions: {_totalExecutions}");
            Print($"BUY Success: {_successfulBuys}");
            Print($"SELL Success: {_successfulSells}");
            Print($"Errors: {_failedOrders}");
            Print("====================");
        }

        #endregion

    }
}
