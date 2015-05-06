using Encog.Neural.NEAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.PopulationFactorys
{
    abstract class BasicPopulationFactory
    {
        public int PopulationNumber = 100;

        public NEATPopulation Get(int inputVectoryNumber, int outputVectoryNumber)
        {
            return Create(inputVectoryNumber, outputVectoryNumber);
        }

        abstract protected NEATPopulation Create(int inputVectoryNumber, int outputVectoryNumber);

    }
}
