// ���� ifdef ���Ǵ���ʹ�� DLL �������򵥵�
// ��ı�׼�������� DLL �е������ļ��������������϶���� DLLTOOLS_EXPORTS
// ���ű���ġ���ʹ�ô� DLL ��
// �κ�������Ŀ�ϲ�Ӧ����˷��š�������Դ�ļ��а������ļ����κ�������Ŀ���Ὣ
// DLLTOOLS_API ������Ϊ�Ǵ� DLL ����ģ����� DLL ���ô˺궨���
// ������Ϊ�Ǳ������ġ�
#ifdef DLLTOOLS_EXPORTS
#define DLLTOOLS_API __declspec(dllexport)
#else
#define DLLTOOLS_API __declspec(dllimport)
#endif

// �����Ǵ� DllTools.dll ������
class DLLTOOLS_API CDllTools {
public:
	CDllTools(void);
	// TODO:  �ڴ�������ķ�����
};

extern DLLTOOLS_API int nDllTools;

DLLTOOLS_API int fnDllTools(void);


