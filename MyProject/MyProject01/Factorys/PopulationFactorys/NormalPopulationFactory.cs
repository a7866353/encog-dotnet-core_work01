﻿using Encog.Neural.NEAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Factorys.PopulationFactorys
{
    class NormalPopulationFactory : BasicPopulationFactory
    {
        protected override NEATPopulation Create(int inputVectoryNumber, int outputVectoryNumber)
        {
            NEATPopulation population = new NEATPopulation(inputVectoryNumber, outputVectoryNumber, PopulationNumber);
            population.InitialConnectionDensity = 1.0;
            population.WeightRange = 0.1;
            population.Reset();

            return population;
        }
    }
}
