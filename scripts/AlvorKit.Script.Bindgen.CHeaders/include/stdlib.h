/* Minimal stub for bindgen parsing. */
#pragma once
#include <stddef.h>
void* malloc(size_t size);
void* calloc(size_t count, size_t size);
void* realloc(void* p, size_t size);
void free(void* p);
void exit(int code);
char* getenv(const char* name);
