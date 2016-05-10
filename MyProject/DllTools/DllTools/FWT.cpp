#include <stdio.h>  
#include <stdlib.h>  
#include "stdafx.h"
#include "DllTools.h"

EXTERN_C
{
#define LENGTH 512//信号长度  
	/******************************************************************
	*　一维卷积函数
	*
	*　说明: 循环卷积,卷积结果的长度与输入信号的长度相同
	*
	*　输入参数: data[],输入信号; core[],卷积核; cov[],卷积结果;
	*            n,输入信号长度; m,卷积核长度.
	*
	*　李承宇, lichengyu2345@126.com
	*
	*  2010-08-18
	******************************************************************/
#if 0
	void Covlution(double data[], double core[], double cov[], int n, int m)
	{
		int i = 0;
		int j = 0;
		int k = 0;

		//将cov[]清零  
		for (i = 0; i < n + m - 1; i++)
		{
			cov[i] = 0;
		}

		//前m/2+1行  
		i = 0;
		for (j = 0; j < m / 2; j++, i++)
		{
			for (k = m / 2 - j; k < m; k++)
			{
				cov[i] += data[k - (m / 2 - j)] * core[k];//k针对core[k]  
			}
#if 1
			for (k = n - m / 2 + j; k < n; k++)
			{
				cov[i] += data[k] * core[k - (n - m / 2 + j)];//k针对data[k]  
			}
#endif
		}

		//中间的n-m行  
		for (i = m / 2; i <= (n - m) + m / 2; i++)
		{
			for (j = 0; j < m; j++)
			{
				cov[i] += data[i - m / 2 + j] * core[j];
			}
		}

		//最后m/2-1行  
		i = (n - m) + m / 2 + 1;
		for (j = 1; j < m / 2; j++, i++)
		{
			for (k = 0; k < j; k++)
			{
				cov[i] += data[k] * core[m - j - k];//k针对data[k]  
			}
#if 1
			for (k = 0; k < m - j; k++)
			{
				cov[i] += core[k] * data[n - (m - j) + k];//k针对core[k]  
			}
#endif
		}

	}
#else
	void Covlution(double data[], double core[], double cov[], int n, int m)
	{
		int i = 0;
		int j = 0;
		int k = 0;
		double tmp;

		int len = n + m - 1;
		//将cov[]清零  
		for (i = 0; i < len; i++)
		{
			cov[i] = 0;
		}

		for (i = 0; i < len; i++)
		{
			tmp = 0;
			for (j = max(0, i + 1 - m); j <= min(i, n - 1); j++)
			{
				tmp += core[i-j] * data[j];
			}

			cov[i] = tmp;
		}

		
	}

#endif


	/******************************************************************
	*　一维小波变换函数
	*
	*　说明: 一维小波变换,只变换一次
	*
	*　输入参数: input[],输入信号; output[],小波变换结果，包括尺度系数和
	*　小波系数两部分; temp[],存放中间结果;h[],Daubechies小波基低通滤波器系数;
	*　g[],Daubechies小波基高通滤波器系数;n,输入信号长度; m,Daubechies小波基紧支集长度.
	*
	*　李承宇, lichengyu2345@126.com
	*
	*  2010-08-19
	******************************************************************/
	void DWT1D(double input[], double output[], double temp[], double h[],
		double g[], int n, int m)
	{
		//  double temp[LENGTH] = {0};//?????????????  

		int i = 0;
		/*
		//尺度系数和小波系数放在一起
		Covlution(input, h, temp, n, m);

		for(i = 0; i < n; i += 2)
		{
		output[i] = temp[i];
		}

		Covlution(input, g, temp, n, m);

		for(i = 1; i < n; i += 2)
		{
		output[i] = temp[i];
		}
		*/

		//尺度系数和小波系数分开  
		Covlution(input, h, temp, n, m);

		for (i = 0; i < n; i += 2)
		{
			output[i / 2] = temp[i];//尺度系数  
		}

		Covlution(input, g, temp, n, m);

		for (i = 1; i < n; i += 2)
		{
			output[n / 2 + i / 2] = temp[i];//小波系数  
		}

	}
	void main()
	{

		double data[LENGTH];//输入信号  
		double temp[LENGTH];//中间结果  
		double data_output[LENGTH];//一维小波变换后的结果  
		int n = 0;//输入信号长度  
		int m = 6;//Daubechies正交小波基长度  
		int i = 0;
		char s[32];//从txt文件中读取一行数据  

		static double h[] = { .332670552950, .806891509311, .459877502118, -.135011020010,
			-.085441273882, .035226291882 };
		static double g[] = { .035226291882, .085441273882, -.135011020010, -.459877502118,
			.806891509311, -.332670552950 };
		/*
		//读取输入信号  
		FILE *fp;
		
		fp = fopen("data.txt", "r");
		if (fp == NULL) //如果读取失败  
		{
			printf("错误！找不到要读取的文件/"data.txt / "/n");
			exit(1);//中止程序  
		}
		
		while (fgets(s, 32, fp) != NULL)//读取长度n要设置得长一点，要保证读到回车符，这样指针才会定位到下一行？回车符返回的是零值？是，非数字字符经过atoi变换都应该返回零值  
		{
			//  fscanf(fp,"%d", &data[count]);//一定要有"&"啊！！！最后读了个回车符！适应能力不如atoi啊  
			data[n] = atof(s);
			n++;
		}
		*/
		//一维小波变换  
		DWT1D(data, data_output, temp, h, g, n, m);

		/*
		//一维小波变换后的结果写入txt文件  
		fp = fopen("data_output.txt", "w");

		//打印一维小波变换后的结果  
		for (i = 0; i < n; i++)
		{
			printf("%f/n", data_output[i]);
			fprintf(fp, "%f/n", data_output[i]);
		}

		//关闭文件  
		fclose(fp);
		*/
	}

}