using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.EAT, AccessRights = AccessRights.None)]
    public class GoldStraddleBot : Robot
    {
        [Parameter("Volume (Lots)", DefaultValue = 0.01, MinValue = 0.01, Step = 0.01)]
        public double Volume { get; set; }

        private bool _executed = false;

        protected override void OnStart()
        {
            // Check current time every 500 milliseconds for precise execution
            Timer.Start(0.5);
        }

        protected override void OnTimer()
        {
            if (_executed) return;

            DateTime now = Server.Time;

            // Target time: Exactly 14:59:57 EAT
            if (now.Hour == 14 && now.Minute == 59 && now.Second == 57)
            {
                _executed = true;
                Timer.Stop();

                ExecuteStraddle();
            }
        }

        private void ExecuteStraddle()
        {
            // Convert lots to standard volume units
            double volumeInUnits = Symbol.QuantityToVolumeInUnits(Volume);

            // Calculate exact Stop Loss and Take Profit distances in Pips based on your 0.01 lot specifications:
            // $10 SL per 0.01 lot = $1,000 per 1 standard lot = 100 Gold points/pips
            // $40 TP per 0.01 lot = $4,000 per 1 standard lot = 400 Gold points/pips
            double stopLossPips = 100;
            double takeProfitPips = 400;

            // Execute Buy and Sell simultaneously
            ExecuteMarketOrder(TradeType.Buy, SymbolName, volumeInUnits, "Buy_Order", stopLossPips, takeProfitPips);
            ExecuteMarketOrder(TradeType.Sell, SymbolName, volumeInUnits, "Sell_Order", stopLossPips, takeProfitPips);
            
            Print("Simultaneous Buy and Sell executed successfully at 14:59:57 EAT.");
        }
    }
}
