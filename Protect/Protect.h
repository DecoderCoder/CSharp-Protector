#pragma once

#include "windows.h"
#include <iostream>

extern "C" __declspec(dllexport) bool _stdcall Protect();//void(__stdcall *callbackfunc)()
DWORD_PTR GetProcessBaseAddress(HANDLE processHandle);
unsigned long Crc32(unsigned char *buf, unsigned long len);
static char * ReadAllBytes(const char * filename, int * read);
void AntiDebugThread(void *param);
void eraseCode();


//#pragma pack(push)
//#pragma pack(1)
//
//template <class T>
//struct LIST_ENTRY_T
//{
//	T Flink;
//	T Blink;
//};
//
//template <class T>
//struct UNICODE_STRING_T
//{
//	union
//	{
//		struct
//		{
//			WORD Length;
//			WORD MaximumLength;
//		};
//		T dummy;
//	};
//	T _Buffer;
//};
//
//template <class T, class NGF, int A>
//struct _PEB_T
//{
//	union
//	{
//		struct
//		{
//			BYTE InheritedAddressSpace;
//			BYTE ReadImageFileExecOptions;
//			BYTE BeingDebugged;
//			BYTE _SYSTEM_DEPENDENT_01;
//		};
//		T dummy01;
//	};
//	T Mutant;
//	T ImageBaseAddress;
//	T Ldr;
//	T ProcessParameters;
//	T SubSystemData;
//	T ProcessHeap;
//	T FastPebLock;
//	T _SYSTEM_DEPENDENT_02;
//	T _SYSTEM_DEPENDENT_03;
//	T _SYSTEM_DEPENDENT_04;
//	union
//	{
//		T KernelCallbackTable;
//		T UserSharedInfoPtr;
//	};
//	DWORD SystemReserved;
//	DWORD _SYSTEM_DEPENDENT_05;
//	T _SYSTEM_DEPENDENT_06;
//	T TlsExpansionCounter;
//	T TlsBitmap;
//	DWORD TlsBitmapBits[2];
//	T ReadOnlySharedMemoryBase;
//	T _SYSTEM_DEPENDENT_07;
//	T ReadOnlyStaticServerData;
//	T AnsiCodePageData;
//	T OemCodePageData;
//	T UnicodeCaseTableData;
//	DWORD NumberOfProcessors;
//	union
//	{
//		DWORD NtGlobalFlag;
//		NGF dummy02;
//	};
//	LARGE_INTEGER CriticalSectionTimeout;
//	T HeapSegmentReserve;
//	T HeapSegmentCommit;
//	T HeapDeCommitTotalFreeThreshold;
//	T HeapDeCommitFreeBlockThreshold;
//	DWORD NumberOfHeaps;
//	DWORD MaximumNumberOfHeaps;
//	T ProcessHeaps;
//	T GdiSharedHandleTable;
//	T ProcessStarterHelper;
//	T GdiDCAttributeList;
//	T LoaderLock;
//	DWORD OSMajorVersion;
//	DWORD OSMinorVersion;
//	WORD OSBuildNumber;
//	WORD OSCSDVersion;
//	DWORD OSPlatformId;
//	DWORD ImageSubsystem;
//	DWORD ImageSubsystemMajorVersion;
//	T ImageSubsystemMinorVersion;
//	union
//	{
//		T ImageProcessAffinityMask;
//		T ActiveProcessAffinityMask;
//	};
//	T GdiHandleBuffer[A];
//	T PostProcessInitRoutine;
//	T TlsExpansionBitmap;
//	DWORD TlsExpansionBitmapBits[32];
//	T SessionId;
//	ULARGE_INTEGER AppCompatFlags;
//	ULARGE_INTEGER AppCompatFlagsUser;
//	T pShimData;
//	T AppCompatInfo;
//	UNICODE_STRING_T<T> CSDVersion;
//	T ActivationContextData;
//	T ProcessAssemblyStorageMap;
//	T SystemDefaultActivationContextData;
//	T SystemAssemblyStorageMap;
//	T MinimumStackCommit;
//};
//
//typedef _PEB_T<DWORD, DWORD64, 34> PEB32;
//typedef _PEB_T<DWORD64, DWORD, 30> PEB64;
//
//#pragma pack(pop)

//void write_to_file(uint8_t *ptr, size_t len, char *name);
//void AntiDump();