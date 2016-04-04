// DllTools.cpp : 定义 DLL 应用程序的导出函数。
//
#include <stdio.h>  
#include <stdlib.h>  
#include "stdafx.h"
#include "math.h"
#include "DllTools.h"
#include <malloc.h>


// 这是导出变量的一个示例
DLLTOOLS_API int nDllTools=0;

// 这是导出函数的一个示例。
DLLTOOLS_API int fnDllTools(void)
{
	return 42;
}

// 这是已导出类的构造函数。
// 有关类定义的信息，请参阅 DllTools.h
CDllTools::CDllTools()
{
	return;
}

EXTERN_C
{
	extern void DWT1D(double input[], double output[], double temp[], double h[], double g[], int n, int m);
	struct FWTParam
	{
		double *input;
		double *output;
		double *temp;
		int inputLength;

		double *h;
		double *g;
		int filterLength;
	};
#if 1 
	DLLTOOLS_API void __stdcall DllTools_DWT1D(FWTParam param)
	{
		/*
		memcpy(param.output, param.input, sizeof(double)*param.inputLength);
		for (int i = 0; i < param.inputLength; i++)
		{
			param.temp[i] = i;
		}
		*/
		int level = 1;
		while (true)
		{
			if (pow((double)2, level) > param.inputLength)
			{
				break;
			}
			level++;
		}
		level--;
		int len = param.inputLength;
		for (int i = 0; i < level; i++)
		{
			DWT1D(param.input, param.output, param.temp, param.h, param.g, len, param.filterLength);

			len /= 2;
			
			if (len <= 8)
				break;
			
			memcpy(param.input, param.output, len*sizeof(double));
		}
	}
#else
	extern int OneFWT(float *signal, unsigned int slength, float **output);
	DLLTOOLS_API void __stdcall DllTools_DWT1D(FWTParam param)
	{
		
		OneFWT((float*)param.input, param.inputLength, (float**)param.output);
	}
#endif

	DLLTOOLS_API void __stdcall DllTools_DWT1D_V2(FWTParam param)
	{
#define             INV_SQRT_2      0.70710678118654752440f;
		int i;
		double dataA, dataB;
		int n = param.inputLength;
		int level = n;
		double *dataArr = param.input;
		double *resultArr = param.output;

		while (level > 1)
		{
			level = level / 2;
			for (i = 0; i < level; i++)
			{
				dataA = dataArr[2 * i];
				dataB = dataArr[2 * i + 1];
				resultArr[i] = (dataA + dataB) * INV_SQRT_2;
				resultArr[level + i] = (dataA - dataB) * INV_SQRT_2;
			}

			for (i = 0; i < level; i++)
			{
				dataArr[i] = resultArr[i];
			}
		}

	}

	DLLTOOLS_API void __stdcall DllTools_DWT1D_V3(FWTParam param)
	{
		int level = param.inputLength;
		double *in = param.input;
		double *out = param.output;
		double *temp = (double *)malloc(param.inputLength*sizeof(double));
		while (1)
		{
			if (level == 1)
				break;
			DWT1D(in, out, temp, param.h, param.g, level, param.filterLength);

			in = out;
			out += level;
			level /= 2;
		}

		free(temp);
	}

}