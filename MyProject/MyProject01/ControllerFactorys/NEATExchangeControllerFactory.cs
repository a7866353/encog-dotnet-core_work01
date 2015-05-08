using MyProject01.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.ControllerFactorys
{
    class NEATStateKeepControllerFactory : BasicControllerFactory
    {
        public NEATStateKeepControllerFactory()
        {
            Name = GetDesc();
        }
        protected override NetworkController Create()
        {
            NetworkController controller;

            TradeDecisionController decisionCtrl = new TradeDecisionController();
            decisionCtrl._inputFormater = new FWTFormater(_inputLength);
            decisionCtrl._outputConvertor = new TradeStateKeepConvertor();
            decisionCtrl.BestNetwork = null;

            controller = NetworkController.Create(Name, decisionCtrl);
            return controller;           
        }

        public override string GetDesc()
        {
            return "FWT_StateKeep";
        }
    }

    class NEATStateSwitchControllerFactory : BasicControllerFactory
    {
        public NEATStateSwitchControllerFactory()
        {
            Name = GetDesc();
        }
        protected override NetworkController Create()
        {
            NetworkController controller;

            TradeDecisionController decisionCtrl = new TradeDecisionController();
            decisionCtrl._inputFormater = new FWTFormater(_inputLength);
            decisionCtrl._outputConvertor = new TradeStateSwitchConvertor();
            decisionCtrl.BestNetwork = null;

            controller = NetworkController.Create(Name, decisionCtrl);
            return controller;
        }

        public override string GetDesc()
        {
            return "FWT_StateSwitch";
        }
    }
}
