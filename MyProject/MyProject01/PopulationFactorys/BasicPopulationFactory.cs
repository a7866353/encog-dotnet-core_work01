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
        public int InputVectoryNumber;
        public int OutputVectoryNumber;
        public NEATPopulation Get()
        {
            return Create();
        }

        abstract protected NEATPopulation Create();

    }
}
