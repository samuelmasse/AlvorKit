/* Minimal stub for bindgen parsing. */
#pragma once
#include <stddef.h>
#include <stdarg.h>
typedef struct FILE FILE;
int printf(const char* fmt, ...);
int fprintf(FILE* f, const char* fmt, ...);
int snprintf(char* s, size_t n, const char* fmt, ...);
int vsnprintf(char* s, size_t n, const char* fmt, va_list args);
