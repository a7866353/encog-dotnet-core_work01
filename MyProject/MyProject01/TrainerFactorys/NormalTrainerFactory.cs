using MyProject01.Controller;
using MyProject01.Controller.Jobs;
using MyProject01.Controller.TrainerFactorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.TrainerFactorys
{
    class NormalTrainerFactory : BasicTrainerFactory
    {
        public string TestCaseName;
        public ITradeDesisoin TradeDesionin;
        protected override Controller.Trainer Create()
        {
            /*
            NormalTrainer trainer = new NormalTrainer();

            TrainResultCheckSyncController mainCheckCtrl = new TrainResultCheckSyncController();
            mainCheckCtrl.Add(new CheckNetworkChangeJob());
            mainCheckCtrl.Add(new UpdataControllerJob());

            TrainResultCheckAsyncController subCheckCtrl = new TrainResultCheckAsyncController();
            subCheckCtrl.Add(new UpdateTestCaseJob() { TestName = TestCaseName, DecisionCtrl = TradeDesionin, });

            mainCheckCtrl.Add(subCheckCtrl);
            trainer.CheckCtrl = mainCheckCtrl;
            
            return trainer;
            */

            return null;

        }
    }
}
