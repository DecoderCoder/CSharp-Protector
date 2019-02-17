#pragma once

#include "windows.h"
#include <iostream>

extern "C" __declspec(dllexport) bool _stdcall Protect64();//void(__stdcall *callbackfunc)()
DWORD_PTR GetProcessBaseAddress(HANDLE processHandle);
unsigned long Crc32(unsigned char *buf, unsigned long len);
static char * ReadAllBytes(const char * filename, int * read);
void AntiDebugThread(void *param);
void eraseCode();
//void write_to_file(uint8_t *ptr, size_t len, char *name);
//void AntiDump();