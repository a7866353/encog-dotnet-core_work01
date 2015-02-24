#include <stdio.h>  
#include <stdlib.h>  
#include "stdafx.h"
#include "DllTools.h"

EXTERN_C
{
#define LENGTH 512//�źų���  
	/******************************************************************
	*��һά�������
	*
	*��˵��: ѭ�����,�������ĳ����������źŵĳ�����ͬ
	*
	*���������: data[],�����ź�; core[],�����; cov[],������;
	*            n,�����źų���; m,����˳���.
	*
	*�������, lichengyu2345@126.com
	*
	*  2010-08-18
	******************************************************************/
	void Covlution(double data[], double core[], double cov[], int n, int m)
	{
		int i = 0;
		int j = 0;
		int k = 0;

		//��cov[]����  
		for (i = 0; i < n; i++)
		{
			cov[i] = 0;
		}

		//ǰm/2+1��  
		i = 0;
		for (j = 0; j < m / 2; j++, i++)
		{
			for (k = m / 2 - j; k < m; k++)
			{
				cov[i] += data[k - (m / 2 - j)] * core[k];//k���core[k]  
			}

			for (k = n - m / 2 + j; k < n; k++)
			{
				cov[i] += data[k] * core[k - (n - m / 2 + j)];//k���data[k]  
			}
		}

		//�м��n-m��  
		for (i = m / 2; i <= (n - m) + m / 2; i++)
		{
			for (j = 0; j < m; j++)
			{
				cov[i] += data[i - m / 2 + j] * core[j];
			}
		}

		//���m/2-1��  
		i = (n - m) + m / 2 + 1;
		for (j = 1; j < m / 2; j++, i++)
		{
			for (k = 0; k < j; k++)
			{
				cov[i] += data[k] * core[m - j - k];//k���data[k]  
			}

			for (k = 0; k < m - j; k++)
			{
				cov[i] += core[k] * data[n - (m - j) + k];//k���core[k]  
			}
		}

	}

	/******************************************************************
	*��һάС���任����
	*
	*��˵��: һάС���任,ֻ�任һ��
	*
	*���������: input[],�����ź�; output[],С���任����������߶�ϵ����
	*��С��ϵ��������; temp[],����м���;h[],DaubechiesС������ͨ�˲���ϵ��;
	*��g[],DaubechiesС������ͨ�˲���ϵ��;n,�����źų���; m,DaubechiesС������֧������.
	*
	*�������, lichengyu2345@126.com
	*
	*  2010-08-19
	******************************************************************/
	void DWT1D(double input[], double output[], double temp[], double h[],
		double g[], int n, int m)
	{
		//  double temp[LENGTH] = {0};//?????????????  

		int i = 0;
		/*
		//�߶�ϵ����С��ϵ������һ��
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

		//�߶�ϵ����С��ϵ���ֿ�  
		Covlution(input, h, temp, n, m);

		for (i = 0; i < n; i += 2)
		{
			output[i / 2] = temp[i];//�߶�ϵ��  
		}

		Covlution(input, g, temp, n, m);

		for (i = 1; i < n; i += 2)
		{
			output[n / 2 + i / 2] = temp[i];//С��ϵ��  
		}

	}

	void main()
	{

		double data[LENGTH];//�����ź�  
		double temp[LENGTH];//�м���  
		double data_output[LENGTH];//һάС���任��Ľ��  
		int n = 0;//�����źų���  
		int m = 6;//Daubechies����С��������  
		int i = 0;
		char s[32];//��txt�ļ��ж�ȡһ������  

		static double h[] = { .332670552950, .806891509311, .459877502118, -.135011020010,
			-.085441273882, .035226291882 };
		static double g[] = { .035226291882, .085441273882, -.135011020010, -.459877502118,
			.806891509311, -.332670552950 };
		/*
		//��ȡ�����ź�  
		FILE *fp;
		
		fp = fopen("data.txt", "r");
		if (fp == NULL) //�����ȡʧ��  
		{
			printf("�����Ҳ���Ҫ��ȡ���ļ�/"data.txt / "/n");
			exit(1);//��ֹ����  
		}
		
		while (fgets(s, 32, fp) != NULL)//��ȡ����nҪ���õó�һ�㣬Ҫ��֤�����س���������ָ��Żᶨλ����һ�У��س������ص�����ֵ���ǣ��������ַ�����atoi�任��Ӧ�÷�����ֵ  
		{
			//  fscanf(fp,"%d", &data[count]);//һ��Ҫ��"&"�������������˸��س�������Ӧ��������atoi��  
			data[n] = atof(s);
			n++;
		}
		*/
		//һάС���任  
		DWT1D(data, data_output, temp, h, g, n, m);

		/*
		//һάС���任��Ľ��д��txt�ļ�  
		fp = fopen("data_output.txt", "w");

		//��ӡһάС���任��Ľ��  
		for (i = 0; i < n; i++)
		{
			printf("%f/n", data_output[i]);
			fprintf(fp, "%f/n", data_output[i]);
		}

		//�ر��ļ�  
		fclose(fp);
		*/
	}

}