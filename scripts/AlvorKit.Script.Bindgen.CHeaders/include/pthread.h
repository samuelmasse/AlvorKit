/* glibc and Darwin public pthread storage ABIs, verified against native headers in CI. */
#pragma once
#if defined(__APPLE__)
typedef void* pthread_t;
typedef struct { long __sig; char __opaque[56]; } pthread_mutex_t;
typedef struct { long __sig; char __opaque[40]; } pthread_cond_t;
#elif defined(__linux__)
typedef unsigned long pthread_t;
#if defined(__aarch64__)
typedef union { char __size[48]; long __align; } pthread_mutex_t;
#elif defined(__x86_64__)
typedef union { char __size[40]; long __align; } pthread_mutex_t;
#elif defined(__arm__)
typedef union { char __size[24]; long __align; } pthread_mutex_t;
#else
#error Unsupported glibc pthread ABI
#endif
typedef union { char __size[48]; long long __align; } pthread_cond_t;
#else
#error Unsupported pthread ABI
#endif
typedef union { char data[56]; long long align; } pthread_rwlock_t;
typedef struct { int detached; } pthread_attr_t;
typedef struct { int kind; } pthread_mutexattr_t;
typedef struct { int clock; } pthread_condattr_t;
