/* Minimal stub for bindgen parsing. */
#pragma once
typedef long long jmp_buf[32];
int setjmp(jmp_buf env);
void longjmp(jmp_buf env, int value);
