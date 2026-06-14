/* Minimal stub for bindgen parsing. */
#pragma once
typedef unsigned long pthread_t;
typedef union { char data[48]; long long align; } pthread_mutex_t;
typedef union { char data[48]; long long align; } pthread_cond_t;
typedef union { char data[56]; long long align; } pthread_rwlock_t;
typedef struct { int detached; } pthread_attr_t;
typedef struct { int kind; } pthread_mutexattr_t;
typedef struct { int clock; } pthread_condattr_t;
