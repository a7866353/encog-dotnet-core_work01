#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "stdafx.h"
#include <math.h>
#include "DllTools.h"

EXTERN_C
{
	struct NEATLinkFloat
	{
		int toNeuron;
		int fromNeuron;
		float weight;
	};

	struct NEATNetworkFloatParm
	{
		int linkCount;
		NEATLinkFloat *link;

		int neuronCount;
		int outputIndex;
		float *preActivation;
		float *postActivation;

		int inputCount;
		double *input;

		int outputCount;
		double *output;

		int activationCycles;
	};

	static inline float ActivationSigmoid(float v)
	{
		return 1.0f / (1.0f + exp(-4.9f * v));
	}

	static void InternalCompute(NEATNetworkFloatParm *parm)
	{
		NEATLinkFloat *pLink;

		int blockSize = 4;
		int cntBlock = parm->linkCount / blockSize;
		int cntRem = parm->linkCount % blockSize;

		pLink = &parm->link[0];
		// update neuron input sum
		for (int i = 0; i < cntBlock; i++)
		{
			parm->preActivation[pLink->toNeuron] += parm->postActivation[pLink->fromNeuron] * pLink->weight;
			pLink++;

			parm->preActivation[pLink->toNeuron] += parm->postActivation[pLink->fromNeuron] * pLink->weight;
			pLink++;

			parm->preActivation[pLink->toNeuron] += parm->postActivation[pLink->fromNeuron] * pLink->weight;
			pLink++;

			parm->preActivation[pLink->toNeuron] += parm->postActivation[pLink->fromNeuron] * pLink->weight;
			pLink++;
		}
		for (int i = 0; i < cntRem; i++)
		{
			parm->preActivation[pLink->toNeuron] += parm->postActivation[pLink->fromNeuron] * pLink->weight;
			pLink++;
		}

		// update neuron output
		for (int i = parm->outputIndex; i < parm->neuronCount; i++)
		{
			parm->postActivation[i] = parm->preActivation[i];
			parm->postActivation[i] = ActivationSigmoid(parm->postActivation[i]);
			parm->preActivation[i] = 0.0;
		}
	}

	static void Compute(NEATNetworkFloatParm *parm)
	{
		// clear
		for (int i = 0; i < parm->neuronCount; i++)
		{
			parm->preActivation[i] = 0.0;
			parm->postActivation[i] = 0.0;
		}
		parm->postActivation[0] = 1.0;

		// copy input
		for (int i = 0; i < parm->inputCount; i++)
		{
			parm->postActivation[i + 1] = (float)parm->input[i];
		}

		// iterate through the network activationCycles times
		for (int i = 0; i < parm->activationCycles; i++)
		{
			InternalCompute(parm);
		}

		// copy output
		for (int i = 0; i < parm->outputCount; i++)
		{
			parm->output[i] = parm->postActivation[parm->outputIndex + i];
		}
	}

	DLLTOOLS_API void __stdcall DllTools_NEATNetworkFloat(NEATNetworkFloatParm param)
	{
		Compute(&param);
	}

}
