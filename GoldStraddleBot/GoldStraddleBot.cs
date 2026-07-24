using System;
using cAlgo.API;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.EAfricaStandardTime, AccessRights = AccessRights.None)]
    public class GoldStraddleBot : Robot
    {
        [Parameter("Volume (Lots)", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double Volume { get; set; }

        [Parameter("Stop Loss (Pips)", DefaultValue = 100)]
        public double StopLoss { get; set; }

        [Parameter("Take Profit (Pips)", DefaultValue = 400)]
        public double TakeProfit { get; set; }

        [Parameter("Execution Hour", DefaultValue = 14)]
        public int ExecutionHour { get; set; }

        [Parameter("Execution Minute", DefaultValue = 59)]
        public int ExecutionMinute { get; set; }

        [Parameter("Execution Second", DefaultValue = 57)]
        public int ExecutionSecond { get; set; }

        private bool _executed;

        protected override void OnStart()
        {
            Timer.Start(TimeSpan.FromMilliseconds(500));
            Print("GoldStraddleBot started.");
        }

        protected override void OnTimer()
        {
            if (_executed)
                return;

            DateTime now = Server.Time;

            if (now.Hour == ExecutionHour &&
                now.Minute == ExecutionMinute &&
                now.Second >= ExecutionSecond)
            {
                ExecuteOrders();
                _executed = true;
                Timer.Stop();
            }
        }

        private void ExecuteOrders()
        {
            double volumeInUnits = Symbol.QuantityToVolumeInUnits(Volume);

            ExecuteMarketOrder(
                TradeType.Buy,
                SymbolName,
                volumeInUnits,
                "GoldStraddle_Buy",
                StopLoss,
                TakeProfit);

            ExecuteMarketOrder(
                TradeType.Sell,
                SymbolName,
                volumeInUnits,
                "GoldStraddle_Sell",
                StopLoss,
                TakeProfit);

            Print($"Orders executed successfully at {Server.Time:HH:mm:ss}");
        }
    }
}
