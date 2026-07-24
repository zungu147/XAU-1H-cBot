using System;
using System.Diagnostics;
using cAlgo.API;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.EAfricaStandardTime, AccessRights = AccessRights.None)]
    public class GoldStraddleBot : Robot
    {

        private const string BotVersion = "v2.0-beta1";


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


        private double _totalExecutionGap;
        private double _fastestGap = double.MaxValue;
        private double _slowestGap;


        #endregion



        #region Start

        protected override void OnStart()
        {
            _currentDate = Server.Time.Date;


            ValidateParameters();


            Timer.Start(TimeSpan.FromMilliseconds(200));


            LogInfo($"{BotVersion} Started");
            LogInfo($"Symbol: {SymbolName}");
            LogInfo($"Volume: {Volume}");
            LogInfo($"SL: {StopLoss}");
            LogInfo($"TP: {TakeProfit}");
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


            Debug($"Time {Server.Time:HH:mm:ss.fff}");
        }

        #endregion



        #region Scheduler

        private void CheckTradeTime(TimeSpan tradeTime, ref bool executed, string name)
        {
            if (executed)
                return;


            TimeSpan now = Server.Time.TimeOfDay;


            TimeSpan windowEnd =
                tradeTime.Add(TimeSpan.FromSeconds(30));


            if (now >= tradeTime && now <= windowEnd)
            {
                executed = true;


                LogInfo($"{name} triggered");


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


            double volumeUnits =
                Symbol.QuantityToVolumeInUnits(Volume);


            Stopwatch timer = Stopwatch.StartNew();


            var buy =
                ExecuteMarketOrder(
                    TradeType.Buy,
                    SymbolName,
                    volumeUnits,
                    $"{BotVersion}_BUY",
                    StopLoss,
                    TakeProfit);


            double buyMilliseconds = timer.Elapsed.TotalMilliseconds;


            var sell =
                ExecuteMarketOrder(
                    TradeType.Sell,
                    SymbolName,
                    volumeUnits,
                    $"{BotVersion}_SELL",
                    StopLoss,
                    TakeProfit);


            double sellMilliseconds = timer.Elapsed.TotalMilliseconds;


            timer.Stop();


            double executionGap =
                sellMilliseconds - buyMilliseconds;


            _totalExecutions++;


            if (buy.IsSuccessful)
                _successfulBuys++;
            else
            {
                _failedOrders++;
                LogError("BUY failed: " + buy.Error);
            }


            if (sell.IsSuccessful)
                _successfulSells++;
            else
            {
                _failedOrders++;
                LogError("SELL failed: " + sell.Error);
            }


            _totalExecutionGap += executionGap;


            if (executionGap < _fastestGap)
                _fastestGap = executionGap;


            if (executionGap > _slowestGap)
                _slowestGap = executionGap;



            if (buy.IsSuccessful && sell.IsSuccessful)
            {
                LogSuccess(
                    $"{trigger} completed | " +
                    $"BUY OK | SELL OK | " +
                    $"Gap {executionGap:F2} ms");
            }
        }

        #endregion



        #region Validation

        private void ValidateParameters()
        {
            TimeSpan.TryParse(TradeTime1, out _time1);
            TimeSpan.TryParse(TradeTime2, out _time2);
            TimeSpan.TryParse(TradeTime3, out _time3);


            LogInfo("Parameters validated");
        }

        #endregion



        #region Trading Days

        private bool IsTradingDay()
        {
            return Server.Time.DayOfWeek switch
            {
                DayOfWeek.Monday => TradeMonday,
                DayOfWeek.Tuesday => TradeTuesday,
                DayOfWeek.Wednesday => TradeWednesday,
                DayOfWeek.Thursday => TradeThursday,
                DayOfWeek.Friday => TradeFriday,
                _ => false
            };
        }

        #endregion



        #region Reset

        private void ResetDaily()
        {
            if (_currentDate == Server.Time.Date)
                return;


            PrintDailySummary();


            _currentDate = Server.Time.Date;


            _executedTime1 = false;
            _executedTime2 = false;
            _executedTime3 = false;


            _totalExecutions = 0;
            _successfulBuys = 0;
            _successfulSells = 0;
            _failedOrders = 0;


            _totalExecutionGap = 0;
            _fastestGap = double.MaxValue;
            _slowestGap = 0;
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
            double average =
                _totalExecutions > 0
                ? _totalExecutionGap / _totalExecutions
                : 0;


            Print("====================");
            Print($"{BotVersion} DAILY SUMMARY");
            Print($"Executions: {_totalExecutions}");
            Print($"BUY Success: {_successfulBuys}");
            Print($"SELL Success: {_successfulSells}");
            Print($"Errors: {_failedOrders}");
            Print($"Average Gap: {average:F2} ms");
            Print($"Fastest Gap: {_fastestGap:F2} ms");
            Print($"Slowest Gap: {_slowestGap:F2} ms");
            Print("====================");
        }

        #endregion
    }
}
