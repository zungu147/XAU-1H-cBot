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

        private DateTime _lastResetDate;

        #endregion


        #region Start

        protected override void OnStart()
        {
            LogInfo("GoldStraddleBot v2.0-alpha2 Started");

            ValidateParameters();

            _lastResetDate = Server.Time.Date;

            Timer.Start(TimeSpan.FromMilliseconds(500));
        }

        #endregion


        #region Timer

        protected override void OnTimer()
        {
            ResetDailyFlags();

            if (!IsTradingDay())
            {
                Debug("Trading day disabled");
                return;
            }


            CheckSchedule(_time1, ref _executedTime1, "Time 1");

            CheckSchedule(_time2, ref _executedTime2, "Time 2");

            CheckSchedule(_time3, ref _executedTime3, "Time 3");
        }

        #endregion


        #region Scheduler

        private void CheckSchedule(TimeSpan tradeTime, ref bool executed, string name)
        {
            if (executed)
                return;


            TimeSpan currentTime = Server.Time.TimeOfDay;


            if (currentTime >= tradeTime &&
                currentTime < tradeTime.Add(TimeSpan.FromSeconds(1)))
            {
                executed = true;

                LogSuccess(name + " triggered at " + Server.Time);

                // Trading will be added in v2.0-alpha3
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


        #region Trading Day

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

        private void ResetDailyFlags()
        {
            if (_lastResetDate != Server.Time.Date)
            {
                _executedTime1 = false;
                _executedTime2 = false;
                _executedTime3 = false;

                _lastResetDate = Server.Time.Date;

                LogInfo("Daily flags reset");
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
