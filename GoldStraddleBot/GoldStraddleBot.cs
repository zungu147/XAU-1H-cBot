using System;
using System.Diagnostics;
using cAlgo.API;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.EAfricaStandardTime, AccessRights = AccessRights.None)]
    public class GoldStraddleBot : Robot
    {
        private const string BotVersion = "v2.0-beta2";


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


        [Parameter("Maximum Open Positions", DefaultValue = 6)]
        public int MaximumOpenPositions { get; set; }


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


        #endregion



        #region Start

        protected override void OnStart()
        {
            _currentDate = Server.Time.Date;


            ValidateParameters();


            Timer.Start(TimeSpan.FromMilliseconds(200));


            PrintStartupReport();


            LogInfo($"{BotVersion} Started");
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


            Debug($"Time: {Server.Time:HH:mm:ss.fff}");
        }

        #endregion



        #region Scheduler

        private void CheckTradeTime(TimeSpan tradeTime, ref bool executed, string name)
        {
            if (executed)
                return;


            TimeSpan now = Server.Time.TimeOfDay;


            if (now >= tradeTime &&
                now <= tradeTime.Add(TimeSpan.FromSeconds(30)))
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
            if (Positions.Count >= MaximumOpenPositions)
            {
                LogError("Maximum open positions reached. Trade skipped.");
                return;
            }


            double volumeUnits =
                Symbol.QuantityToVolumeInUnits(Volume);


            Stopwatch stopwatch = Stopwatch.StartNew();


            var buy =
                ExecuteMarketOrder(
                    TradeType.Buy,
                    SymbolName,
                    volumeUnits,
                    $"{BotVersion}_BUY",
                    StopLoss,
                    TakeProfit);


            double buyTime = stopwatch.Elapsed.TotalMilliseconds;


            var sell =
                ExecuteMarketOrder(
                    TradeType.Sell,
                    SymbolName,
                    volumeUnits,
                    $"{BotVersion}_SELL",
                    StopLoss,
                    TakeProfit);


            double sellTime = stopwatch.Elapsed.TotalMilliseconds;


            stopwatch.Stop();


            double gap = sellTime - buyTime;


            _totalExecutions++;


            if (buy.IsSuccessful)
            {
                _successfulBuys++;

                LogSuccess(
                    $"BUY opened | ID {buy.Position.Id} | Price {buy.Position.EntryPrice}");
            }
            else
            {
                _failedOrders++;

                LogError(
                    $"BUY failed: {buy.Error}");
            }



            if (sell.IsSuccessful)
            {
                _successfulSells++;

                LogSuccess(
                    $"SELL opened | ID {sell.Position.Id} | Price {sell.Position.EntryPrice}");
            }
            else
            {
                _failedOrders++;

                LogError(
                    $"SELL failed: {sell.Error}");
            }


            _totalExecutionGap += gap;


            LogInfo(
                $"{trigger} completed | Execution gap {gap:F2} ms");
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
        }

        #endregion



        #region Reporting

        private void PrintStartupReport()
        {
            Print("========================");
            Print(BotVersion);
            Print($"Symbol: {SymbolName}");
            Print($"Volume: {Volume}");
            Print($"SL: {StopLoss}");
            Print($"TP: {TakeProfit}");
            Print($"Existing Positions: {Positions.Count}");
            Print("========================");
        }


        private void PrintDailySummary()
        {
            Print("========================");
            Print("DAILY SUMMARY");
            Print($"Executions: {_totalExecutions}");
            Print($"BUY: {_successfulBuys}");
            Print($"SELL: {_successfulSells}");
            Print($"Errors: {_failedOrders}");
            Print("========================");
        }

        #endregion



        #region Logging

        private void LogInfo(string msg)
        {
            Print("[INFO] " + msg);
        }


        private void LogSuccess(string msg)
        {
            Print("[SUCCESS] " + msg);
        }


        private void LogError(string msg)
        {
            Print("[ERROR] " + msg);
        }


        private void Debug(string msg)
        {
            if (DebugMode)
                Print("[DEBUG] " + msg);
        }

        #endregion
    }
}
