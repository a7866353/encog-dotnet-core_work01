include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "stdafx.h"
#include <math.h>
#include "DllTools.h"

EXTERN_C
{
	struct NEATLink
	{
		int toNeuron;
		int fromNeuron;
		double weight;
	};

	struct NEATNetworkParm
	{
		int linkCount;
		NEATLink *link;

		int neuronCount;
		int outputIndex;
		double *preActivation;
		double *postActivation;

		int inputCount;
		double *input;

		int outputCount;
		double *output;

		int activationCycles;
	};

	static inline double ActivationSigmoid(double v)
	{
		return 1.0 / (1.0 + exp(-4.9 * v));
	}

	static void InternalCompute(NEATNetworkParm *parm)
	{
		NEATLink *pLink;

		// update neuron input sum
		for (int i = 0; i < parm->linkCount; i++)
		{
			pLink = &parm->link[i];
			parm->preActivation[pLink->toNeuron] += parm->postActivation[pLink->fromNeuron] * pLink->weight;
		}

		// update neuron output
		for (int i = parm->outputIndex; i < parm->neuronCount; i++)
		{
			parm->postActivation[i] = parm->preActivation[i];
			parm->postActivation[i] = ActivationSigmoid(parm->postActivation[i]);
			parm->preActivation[i] = 0.0;
		}
	}

	static void Compute(NEATNetworkParm *parm)
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
			parm->postActivation[i + 1] = parm->input[i];
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

	DLLTOOLS_API void __stdcall DllTools_NEATNetwork(NEATNetworkParm param)
	{
		NEATNetworkParm internalParam = param;
		Compute(&internalParam);
	}

}
