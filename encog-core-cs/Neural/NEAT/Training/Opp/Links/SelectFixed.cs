//
// Encog(tm) Core v3.2 - .Net Version
// http://www.heatonresearch.com/encog/
//
// Copyright 2008-2014 Heaton Research, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//   
// For more information on Heaton Research copyrights, licenses 
// and trademarks visit:
// http://www.heatonresearch.com/copyright
//
using System;
using System.Collections.Generic;
using System.Text;
using Encog.ML.EA.Train;
using Encog.MathUtil.Randomize;

namespace Encog.Neural.NEAT.Training.Opp.Links
{
    /// <summary>
    /// Select a fixed number of link genes. If the genome does not have enough links
    /// to select the specified count, then all genes will be returned.
    /// 
    /// -----------------------------------------------------------------------------
    /// http://www.cs.ucf.edu/~kstanley/ Encog's NEAT implementation was drawn from
    /// the following three Journal Articles. For more complete BibTeX sources, see
    /// NEATNetwork.java.
    /// 
    /// Evolving Neural Networks Through Augmenting Topologies
    /// 
    /// Generating Large-Scale Neural Networks Through Discovering Geometric
    /// Regularities
    /// 
    /// Automatic feature selection in neuroevolution
    /// </summary>
    public class SelectFixed : ISelectLinks
    {
        /// <summary>
        /// The number of links to choose.
        /// </summary>
        private readonly int _linkCount;

        /// <summary>
        /// The trainer.
        /// </summary>
        private IEvolutionaryAlgorithm _trainer;

        /// <summary>
        /// Construct a fixed count link selector.
        /// </summary>
        /// <param name="theLinkCount">The number of links to select.</param>
        public SelectFixed(int theLinkCount)
        {
            _linkCount = theLinkCount;
        }

        /// <summary>
        /// The trainer.
        /// </summary>
        public IEvolutionaryAlgorithm Trainer
        {
            get
            {
                return _trainer;
            }
        }

        /// <inheritdoc/>
        public void Init(IEvolutionaryAlgorithm theTrainer)
        {
            _trainer = theTrainer;
        }

        /// <inheritdoc/>
        public IList<NEATLinkGene> SelectLinks(EncogRandom rnd,
                NEATGenome genome)
        {
            IList<NEATLinkGene> result = new List<NEATLinkGene>();
            int cnt = Math.Min(_linkCount, genome.LinksChromosome.Count);

            while (result.Count < cnt)
            {
                int idx = rnd.Next(genome.LinksChromosome.Count);
                NEATLinkGene link = genome.LinksChromosome[idx];
                if (!result.Contains(link))
                {
                    result.Add(link);
                }
            }
            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append("[");
            result.Append(GetType().Name);
            result.Append(":linkCount=");
            result.Append(_linkCount);
            result.Append("]");
            return result.ToString();
        }
    }
}
