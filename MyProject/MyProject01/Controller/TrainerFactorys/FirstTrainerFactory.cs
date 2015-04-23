using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller.Jobs;

namespace MyProject01.Controller.TrainerFactorys
{
    class FirstTrainerFactory
    {
        public string TestCaseName;
        public ITradeDesisoin TradeDesionin;

        public Trainer GetTrainer()
        {
            NormalTrainer trainer = new NormalTrainer();

            TrainResultCheckSyncController mainCheckCtrl = new TrainResultCheckSyncController();
            mainCheckCtrl.Add(new CheckNetworkChangeJob());
            mainCheckCtrl.Add(new UpdataControllerJob());

            TrainResultCheckAsyncController subCheckCtrl = new TrainResultCheckAsyncController();
            subCheckCtrl.Add(new UpdateTestCaseJob() { TestName = TestCaseName, DecisionCtrl = TradeDesionin, });

            mainCheckCtrl.Add(subCheckCtrl);
            trainer.CheckCtrl = mainCheckCtrl;

            return trainer;
        }
    }
}
