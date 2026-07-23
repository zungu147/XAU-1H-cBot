using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    // Changed TimeZone to the official standard "E. Africa Standard Time"
    [Robot(TimeZone = "E. Africa Standard Time", AccessRights = AccessRights.None)]
    public class GoldStraddleBot : Robot
    {
        [Parameter("Volume (Lots)", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double Volume { get; set; }

        private bool _executed = false;

        protected override void OnStart()
        {
            Timer.Start(0.5);
        }

        protected override void OnTimer()
        {
            if (_executed) return;

            DateTime now = Server.Time;

            if (now.Hour == 14 && now.Minute == 59 && now.Second == 57)
            {
                _executed = true;
                Timer.Stop();

                ExecuteStraddle();
            }
        }

        private void ExecuteStraddle()
        {
            double volumeInUnits = Symbol.QuantityToVolumeInUnits(Volume);

            double stopLossPips = 100;
            double takeProfitPips = 400;

            ExecuteMarketOrder(TradeType.Buy, SymbolName, volumeInUnits, "Buy_Order", stopLossPips, takeProfitPips);
            ExecuteMarketOrder(TradeType.Sell, SymbolName, volumeInUnits, "Sell_Order", stopLossPips, takeProfitPips);
            
            Print("Simultaneous Buy and Sell executed successfully at 14:59:57 EAT.");
        }
    }
}
