// dllmain.cpp : Определяет точку входа для приложения DLL.


#define AntiDebugNativeDef 1
#define AntiDebugManagedDef 1
#define AntiDumpDef 1
#define MethodEncryptionDef 1
#define CRC32CheckDef 1

#define CRC32 3590569465
#define CodeLength 848
#define AssemblySize 9216
#define AntiDebugManagedDelay 1000000 // 100 ms

#include "stdafx.h"
#include "Dll1.h"
#include <psapi.h>
#include <tchar.h>
#include "Protect.h"
#include <fstream>
#if (CRC32CheckDef == 1)

#endif
#if (AntiDebugNativeDef == 1)
	#include <process.h>
#endif

#if (MethodEncryptionDef == 1)
	#include "aes.hpp"
#endif

#ifdef _UNICODE
#define tcout wcout
#define tcerr wcerr
#else
#define tcout cout
#define tcerr cerr
#endif

const uint8_t iv[] = { 0x49,0xbd,0x5a,0xbd,0xd6,0x26,0xdd,0x3e,0xf6,0x2b,0xdf,0x23,0x84,0xaa,0x8f,0xc0 };
const uint8_t key[] = { 0x97,0x6e,0x6b,0x4b,0xda,0x75,0xf5,0x53,0xc0,0xc3,0xbc,0x8f,0xfc,0x78,0xd9,0x86,0x5b,0x5a,0x2b,0x63,0xf0,0x25,0x09,0x23,0xf5,0x3b,0x8f,0xb1,0x03,0xee,0x65,0x1c };

using namespace std;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
	{
#if (AntiDebugNativeDef == 1)
if (IsDebuggerPresent()) {
		eraseCode();
		TerminateProcess(GetCurrentProcess(), 0);
}
		_beginthread(AntiDebugThread, 0, NULL);
#endif

#if (CRC32CheckDef == 1)
		TCHAR filename[MAX_PATH];

		GetModuleFileNameEx(GetCurrentProcess(), NULL, filename, MAX_PATH);

		int read;
		char charFilename[MAX_PATH];
		wcstombs(charFilename, filename, wcslen(filename) + 1);
		char *file = ReadAllBytes(charFilename, &read);


		unsigned long currentCRC = Crc32((unsigned char*)file, AssemblySize);
		if (currentCRC != CRC32)
		{
			eraseCode();
			TerminateProcess(GetCurrentProcess(), 0);
			return FALSE;			
		}

#endif
	}
	break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
		break;
    }
    return TRUE;
}


void AntiDebugThread(void *param) {
	while (true) {
		if (IsDebuggerPresent()) {
			eraseCode();
			TerminateProcess(GetCurrentProcess(), 0);
		}
		// CheckRemoteDebuggerPresent
		PBOOL crdp = FALSE;
		CheckRemoteDebuggerPresent(GetCurrentProcess(), crdp);
		if (crdp) {
			eraseCode();
			TerminateProcess(GetCurrentProcess(), 0);
		}

		if (GetCurrentProcess() == 0) {
			eraseCode();
			TerminateProcess(GetCurrentProcess(), 0);
		}

		Sleep(5000);
	}
}

extern "C" __declspec(dllexport) bool _stdcall Protect() {//void(__stdcall *antidump)()
	HANDLE curProc = GetCurrentProcess();
	DWORD old;

#if (AntiDebugNativeDef == 1)
	FILETIME createTime;
	FILETIME exitTime;
	FILETIME kernelTime;
	FILETIME userTime;
	SYSTEMTIME currentSystemTime;
	FILETIME currentTime;

	if (GetProcessTimes(GetCurrentProcess(),
		&createTime, &exitTime, &kernelTime, &userTime) == 1) {
		GetSystemTime(&currentSystemTime);
		SystemTimeToFileTime(&currentSystemTime, &currentTime);

		LONGLONG diffInTicks =	reinterpret_cast<LARGE_INTEGER*>(&currentTime)->QuadPart - reinterpret_cast<LARGE_INTEGER*>(&createTime)->QuadPart;

		if (diffInTicks > AntiDebugManagedDelay) {
			eraseCode();
			TerminateProcess(GetCurrentProcess(), 0);
		}
	}
	else {
		eraseCode();
		TerminateProcess(GetCurrentProcess(), 0);
	}
#endif

#if (AntiDumpDef == 1)
	BYTE *bas = (BYTE*)GetProcessBaseAddress(curProc);
	BYTE *ptr = bas + 0x3c;
	ptr = bas + *ptr;
	BYTE *ptr2 = ptr;
	ptr += 0x6;
	uint16_t sectNum = *ptr;
	ptr += 14;
	USHORT optSize = *ptr;

	void *BYTEArr[11] = { 0x0 };
	// if (module.FullyQualifiedName[0] != '<') //Mapped

	BYTE *mdDir = bas + *(ptr - 16);

	if (*(ptr - 0x78) != 0)
	{
		BYTE *importDir = bas + *(ptr - 0x78);
		BYTE *oftMod = bas + *importDir;
		BYTE *modName = bas + *(importDir + 12);
		BYTE *funcName = bas + *(oftMod + 2);

		VirtualProtect(modName, 11, 0x40, &old);

		*BYTEArr = (uint32_t*)0x6c64746e;
		*(BYTEArr + 1) = (uint32_t*)0x6c642e6c;
		*(BYTEArr + 4) = (uint16_t*)0x006c;
		*(BYTEArr + 10) = (BYTE*)0;

		for (int i = 0; i < 11; i++)
			*(modName + i) = (BYTE)*(BYTEArr + i);

		VirtualProtect(funcName, 11, 0x40, &old);

		*BYTEArr = (uint32_t*)0x6f43744e;
		*(BYTEArr + 1) = (uint32_t*)0x6c642e6c;
		*(BYTEArr + 4) = (uint16_t*)0x006c;
		*(BYTEArr + 10) = (BYTE*)0;

		for (int i = 0; i < 11; i++)
			*(funcName + i) = (BYTE)*(BYTEArr + i);
	}

	for (int i = 0; i < sectNum; i++)
	{
		VirtualProtect(ptr, 8, 0x40, &old);
		memset(ptr, 0, 8);
		ptr += 0x28;
	}

	VirtualProtect(mdDir, 0x48, 0x40, &old);

	BYTE* mdHdr = bas + *(mdDir + 8);
	*(mdDir + 1) = 0;
	*(mdDir + 2) = 0;
	*(mdDir + 3) = 0;

	VirtualProtect(mdHdr, 4, 0x40, &old);
	*mdHdr = 0;
	mdHdr += 12;
	mdHdr += *mdHdr;
	mdHdr = (BYTE*)((uint64_t)(mdHdr + 7) & ~3UL);
	mdHdr += 2;
	uint16_t numOfStream = *mdHdr;
	mdHdr += 2;

	for (int i = 0; i < numOfStream; i++)
	{
		VirtualProtect(mdHdr, 8, 0x40, &old);
		mdHdr += 4;
		mdHdr += 4;
		for (int ii = 0; ii < 8; ii++)
		{
			VirtualProtect(mdHdr, 4, 0x40, &old);
			*mdHdr = 0;
			mdHdr++;
			if (*mdHdr == 0)
			{
				mdHdr += 3;
				break;
			}

			*mdHdr = 0;
			mdHdr++;
			if (*mdHdr == 0)
			{
				mdHdr += 2;
				break;
			}

			*mdHdr = 0;
			mdHdr++;
			if (*mdHdr == 0)
			{
				mdHdr += 1;
				break;
			}

			*mdHdr = 0;
			mdHdr++;
		}
	}
#endif

#if (MethodEncryptionDef == 1)
	DWORD_PTR module_ptr = GetProcessBaseAddress(curProc);
	uint8_t BYTEs[CodeLength];
	VirtualProtect((LPVOID)(module_ptr + 8192), sizeof(BYTEs), 0x40, &old);
	ReadProcessMemory(curProc, (LPVOID)(module_ptr + 8192), &BYTEs, sizeof(BYTEs), NULL);

	AES_ctx ctx;
	AES_init_ctx_iv(&ctx, key, iv);
	AES_CBC_decrypt_buffer(&ctx, BYTEs, sizeof(BYTEs));
	memset((void*)(module_ptr + 8192), 0x0, CodeLength);
	WriteProcessMemory(curProc, (LPVOID)(module_ptr + 8192), &BYTEs, sizeof(BYTEs), NULL);
	VirtualProtect((LPVOID)(module_ptr + 8192), sizeof(BYTEs), 0x0, &old);
#endif

	return true;
}

DWORD_PTR GetProcessBaseAddress(HANDLE processHandle)
{
	DWORD_PTR   baseAddress = 0;
	HMODULE     *moduleArray;
	LPBYTE      moduleArrayBYTEs;
	DWORD       BYTEsRequired;
	if (processHandle)
	{
		if (EnumProcessModules(processHandle, NULL, 0, &BYTEsRequired))
		{
			if (BYTEsRequired)
			{
				moduleArrayBYTEs = (LPBYTE)LocalAlloc(LPTR, BYTEsRequired);
				if (moduleArrayBYTEs)
				{
					unsigned int moduleCount;
					moduleCount = BYTEsRequired / sizeof(HMODULE);
					moduleArray = (HMODULE *)moduleArrayBYTEs;
					if (EnumProcessModules(processHandle, moduleArray, BYTEsRequired, &BYTEsRequired))
					{
						baseAddress = (DWORD_PTR)moduleArray[0];
					}
					LocalFree(moduleArrayBYTEs);
				}
			}
		}
		CloseHandle(processHandle);
	}
	return baseAddress;
}

void eraseCode() {
	DWORD_PTR module_ptr = GetProcessBaseAddress(GetCurrentProcess());
	DWORD old;

	//VirtualProtect((void*)module_ptr, 4096, 0x40, &old);
	//memset((void*)(module_ptr + 8192), 0x0, 4096);
	//VirtualProtect((void*)module_ptr, 4096, 0x0, &old);
	VirtualProtect((void*)(module_ptr + 8192), CodeLength, 0x40, &old);
	memset((void*)(module_ptr + 8192), 0x0, CodeLength);
	VirtualProtect((void*)(module_ptr + 8192), CodeLength, 0x0, &old);
}

static char * ReadAllBytes(const char * filename, int * read)
{
	ifstream ifs(filename, ios::binary | ios::ate);
	ifstream::pos_type pos = ifs.tellg();
	int length = pos;
	char *pChars = new char[length];
	ifs.seekg(0, ios::beg);
	ifs.read(pChars, length);
	ifs.close();
	*read = length;
	return pChars;
}

unsigned long Crc32(unsigned char *buf, unsigned long len)
{
	unsigned long crc_table[256] = {
			0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
			0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
			0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
			0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
			0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
			0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
			0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
			0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
			0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
			0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
			0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
			0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
			0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
			0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
			0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
			0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
			0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
			0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
			0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
			0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
			0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
			0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
			0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
			0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
			0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
			0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
			0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
			0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
			0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
			0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
			0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
			0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
	};
	unsigned long crc;

	for (int i = 0; i < 256; i++)
	{
		crc = i;
		for (int j = 0; j < 8; j++)
			crc = crc & 1 ? (crc >> 1) ^ 0xEDB88320UL : crc >> 1;
		crc_table[i] = crc;
	}

	crc = 0xFFFFFFFFUL;

	while (len--)
		crc = crc_table[(crc ^ *buf++) & 0xFF] ^ (crc >> 8);

	return crc ^ 0xFFFFFFFFUL;
};