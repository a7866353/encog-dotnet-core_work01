using Encog.ML;
using Encog.ML.EA.Train;
using Encog.Neural.NEAT;
using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Controller.Jobs
{
    class TrainerContex
    {
        public long Epoch;

        public NormalTrainer Trainer;
        public bool IsEnd;
        public bool IsChanged;

        // Test Data
        public BasicDataBlock _testDataBlock;
        public BasicDataBlock _trainDataBlock;

        // TestCase
        public TrainEA train;

        public NEATNetwork BestNetwork;

        public void SetDataLength(BasicDataBlock dataBlock, int trainLength)
        {
            _testDataBlock = dataBlock;

            // Update test data
            _trainDataBlock = _testDataBlock.GetNewBlock(0, trainLength);

        }



        public TrainerContex Clone()
        {
            return (TrainerContex)MemberwiseClone();
        }
    }
}
