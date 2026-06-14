/* Minimal stub for bindgen parsing; clang's builtin macros adapt per target. */
#pragma once
#define CHAR_BIT 8
#define SCHAR_MIN (-128)
#define SCHAR_MAX 127
#define UCHAR_MAX 255
#define CHAR_MIN SCHAR_MIN
#define CHAR_MAX SCHAR_MAX
#define SHRT_MIN (-32768)
#define SHRT_MAX 32767
#define USHRT_MAX 65535
#define INT_MIN (-INT_MAX - 1)
#define INT_MAX __INT_MAX__
#define UINT_MAX (INT_MAX * 2u + 1u)
#define LONG_MIN (-LONG_MAX - 1l)
#define LONG_MAX __LONG_MAX__
#define ULONG_MAX (LONG_MAX * 2ul + 1ul)
#define LLONG_MIN (-LLONG_MAX - 1ll)
#define LLONG_MAX __LONG_LONG_MAX__
#define ULLONG_MAX (LLONG_MAX * 2ull + 1ull)
