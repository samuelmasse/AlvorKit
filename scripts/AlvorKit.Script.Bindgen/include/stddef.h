/* Minimal stub for bindgen parsing; clang's builtin macros adapt per target. */
#pragma once
typedef __SIZE_TYPE__ size_t;
typedef __PTRDIFF_TYPE__ ptrdiff_t;
typedef __WCHAR_TYPE__ wchar_t;
#define NULL ((void*)0)
#define offsetof(t, m) __builtin_offsetof(t, m)
