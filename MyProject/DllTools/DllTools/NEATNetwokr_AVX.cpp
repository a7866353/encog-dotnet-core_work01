#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "stdafx.h"
#include <math.h>
#include "DllTools.h"
#include <immintrin.h>

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

		int outBufferCtrlCount;
		OutputBufferCtrl *outBufferCtrl;

		int inputCount;
		double *input;

		int outputCount;
		double *output;

		int activationCycles;
	};

	struct OutputBufferCtrl
	{
		float *pBuf;
		float *pBufStart;
		int length;
	};

	static inline double ActivationSigmoid(double v)
	{
		return 1.0 / (1.0 + exp(-4.9 * v));
	}

#define BLOCK_LENGTH (8)
	static float Sumfloat_AVX_4loop(const float *inArr, int cntbuf)
	{
		float s = 0;
		int i;
		int nBlockWidth = 8 * 4;
		int cntBlock = cntbuf / nBlockWidth;
		int cntRem = cntbuf % nBlockWidth;

		__m256 yfsSum = _mm256_setzero_ps();
		__m256 yfsSum1 = _mm256_setzero_ps();
		__m256 yfsSum2 = _mm256_setzero_ps();
		__m256 yfsSum3 = _mm256_setzero_ps();

		__m256 yfsLoad;
		__m256 yfsLoad1;
		__m256 yfsLoad2;
		__m256 yfsLoad3;

		const float *p = inArr;
		const float *q;

		for (i = 0; i < cntBlock; i++)
		{
			yfsLoad = _mm256_loadu_ps(p);
			yfsLoad1 = _mm256_loadu_ps(p + 8);
			yfsLoad2 = _mm256_loadu_ps(p + 16);
			yfsLoad3 = _mm256_loadu_ps(p + 24);

			yfsSum = _mm256_add_ps(yfsSum, yfsLoad);
			yfsSum1 = _mm256_add_ps(yfsSum1, yfsLoad1);
			yfsSum2 = _mm256_add_ps(yfsSum2, yfsLoad2);
			yfsSum3 = _mm256_add_ps(yfsSum3, yfsLoad3);

			p += nBlockWidth;
		}

		yfsSum = _mm256_add_ps(yfsSum, yfsSum1);
		yfsSum2 = _mm256_add_ps(yfsSum2, yfsSum3);
		yfsSum = _mm256_add_ps(yfsSum, yfsSum2);
		q = (const float*)&yfsSum;
		s = q[0] + q[1] + q[2] + q[3] + q[4] + q[5] + q[6] + q[7];

		for (i = 0; i < cntRem; i++)
		{
			s += p[i];
			p++;
		}

		return s;
	}


	static void InternalCompute(NEATNetworkParm *parm)
	{
		NEATLink *pLink, *pLinkBlockStart;

		float *outputArr;
		int outputArrStride = parm->neuronCount - parm->outputIndex;
		float **outputPtrArr;
		
		OutputBufferCtrl *pOutBufferCtrl;

		float postActArr[BLOCK_LENGTH];
		float weightArr[BLOCK_LENGTH];
		float preArr[BLOCK_LENGTH];

		// Init output buffer control
		for (int i = 0; i < parm->outBufferCtrlCount; i++)
		{
			OutputBufferCtrl *pCtrl = &parm->outBufferCtrl[i];
			pCtrl->pBuf = pCtrl->pBufStart;
			pCtrl->length = 0;
		}

		pLinkBlockStart = pLink = &parm->link[0];
		int cntRem = parm->linkCount;
		int cntBlockLen;

		// update neuron input sum
		while (cntRem>0)
		{
			cntBlockLen = parm->linkCount>BLOCK_LENGTH ? BLOCK_LENGTH : parm->linkCount;
			pLink = pLinkBlockStart;
			for (int j = 0; j < cntBlockLen; j++)
			{
				postActArr[j] = parm->postActivation[pLink->fromNeuron];
				weightArr[j] = parm->postActivation[pLink->fromNeuron] * pLink->weight;
				pLink++;
			}
			__m256 post = _mm256_load_ps(postActArr);
			__m256 weight = _mm256_load_ps(weightArr);
			__m256 pre = _mm256_mul_ps(post, weight);
			_mm256_storeu_ps(preArr, pre);

			pLink = pLinkBlockStart;
			for (int j = 0; j < cntBlockLen; j++)
			{
				OutputBufferCtrl *pCtrl = &parm->outBufferCtrl[pLink->toNeuron];
				*(pCtrl->pBuf) = preArr[j];
				pCtrl->pBuf++;
				pCtrl->length++;
			}
			pLinkBlockStart += cntBlockLen;

			cntRem -= cntBlockLen;
		}

		// Calculate output sum
		for (int i = 0; i < parm->outBufferCtrlCount; i++)
		{
			OutputBufferCtrl *pCtrl = &parm->outBufferCtrl[i];
			float result = Sumfloat_AVX_4loop(pCtrl->pBufStart, pCtrl->length);
			parm->preActivation[parm->outputIndex + i] = result;
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

	DLLTOOLS_API void __stdcall DllTools_NEATNetwork_AVX(NEATNetworkParm param)
	{
		Compute(&param);
	}

}
