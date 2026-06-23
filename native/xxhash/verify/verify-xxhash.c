#include <errno.h>
#include <inttypes.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#if defined(_WIN32)
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#else
#include <dlfcn.h>
#endif

typedef enum XXH_errorcode {
    XXH_OK = 0,
    XXH_ERROR = 1
} XXH_errorcode;

typedef struct XXH128_hash_t {
    uint64_t low64;
    uint64_t high64;
} XXH128_hash_t;

typedef struct XXH32_canonical_t {
    unsigned char digest[4];
} XXH32_canonical_t;

typedef struct XXH64_canonical_t {
    unsigned char digest[8];
} XXH64_canonical_t;

typedef struct XXH128_canonical_t {
    unsigned char digest[16];
} XXH128_canonical_t;

typedef void XXH32_state_t;
typedef void XXH64_state_t;
typedef void XXH3_state_t;

typedef uint32_t (*XXH_versionNumber_fn)(void);
typedef uint32_t (*XXH32_fn)(const void *, size_t, uint32_t);
typedef XXH32_state_t *(*XXH32_createState_fn)(void);
typedef XXH_errorcode (*XXH32_freeState_fn)(XXH32_state_t *);
typedef void (*XXH32_copyState_fn)(XXH32_state_t *, const XXH32_state_t *);
typedef XXH_errorcode (*XXH32_reset_fn)(XXH32_state_t *, uint32_t);
typedef XXH_errorcode (*XXH32_update_fn)(XXH32_state_t *, const void *, size_t);
typedef uint32_t (*XXH32_digest_fn)(const XXH32_state_t *);
typedef void (*XXH32_canonicalFromHash_fn)(XXH32_canonical_t *, uint32_t);
typedef uint32_t (*XXH32_hashFromCanonical_fn)(const XXH32_canonical_t *);

typedef uint64_t (*XXH64_fn)(const void *, size_t, uint64_t);
typedef XXH64_state_t *(*XXH64_createState_fn)(void);
typedef XXH_errorcode (*XXH64_freeState_fn)(XXH64_state_t *);
typedef void (*XXH64_copyState_fn)(XXH64_state_t *, const XXH64_state_t *);
typedef XXH_errorcode (*XXH64_reset_fn)(XXH64_state_t *, uint64_t);
typedef XXH_errorcode (*XXH64_update_fn)(XXH64_state_t *, const void *, size_t);
typedef uint64_t (*XXH64_digest_fn)(const XXH64_state_t *);
typedef void (*XXH64_canonicalFromHash_fn)(XXH64_canonical_t *, uint64_t);
typedef uint64_t (*XXH64_hashFromCanonical_fn)(const XXH64_canonical_t *);

typedef uint64_t (*XXH3_64bits_fn)(const void *, size_t);
typedef uint64_t (*XXH3_64bits_withSeed_fn)(const void *, size_t, uint64_t);
typedef uint64_t (*XXH3_64bits_withSecret_fn)(const void *, size_t, const void *, size_t);
typedef uint64_t (*XXH3_64bits_withSecretandSeed_fn)(const void *, size_t, const void *, size_t, uint64_t);
typedef XXH3_state_t *(*XXH3_createState_fn)(void);
typedef XXH_errorcode (*XXH3_freeState_fn)(XXH3_state_t *);
typedef void (*XXH3_copyState_fn)(XXH3_state_t *, const XXH3_state_t *);
typedef XXH_errorcode (*XXH3_64bits_reset_fn)(XXH3_state_t *);
typedef XXH_errorcode (*XXH3_64bits_reset_withSeed_fn)(XXH3_state_t *, uint64_t);
typedef XXH_errorcode (*XXH3_64bits_reset_withSecret_fn)(XXH3_state_t *, const void *, size_t);
typedef XXH_errorcode (*XXH3_64bits_reset_withSecretandSeed_fn)(XXH3_state_t *, const void *, size_t, uint64_t);
typedef XXH_errorcode (*XXH3_64bits_update_fn)(XXH3_state_t *, const void *, size_t);
typedef uint64_t (*XXH3_64bits_digest_fn)(const XXH3_state_t *);

typedef XXH128_hash_t (*XXH3_128bits_fn)(const void *, size_t);
typedef XXH128_hash_t (*XXH3_128bits_withSeed_fn)(const void *, size_t, uint64_t);
typedef XXH128_hash_t (*XXH3_128bits_withSecret_fn)(const void *, size_t, const void *, size_t);
typedef XXH128_hash_t (*XXH3_128bits_withSecretandSeed_fn)(const void *, size_t, const void *, size_t, uint64_t);
typedef XXH_errorcode (*XXH3_128bits_reset_fn)(XXH3_state_t *);
typedef XXH_errorcode (*XXH3_128bits_reset_withSeed_fn)(XXH3_state_t *, uint64_t);
typedef XXH_errorcode (*XXH3_128bits_reset_withSecret_fn)(XXH3_state_t *, const void *, size_t);
typedef XXH_errorcode (*XXH3_128bits_reset_withSecretandSeed_fn)(XXH3_state_t *, const void *, size_t, uint64_t);
typedef XXH_errorcode (*XXH3_128bits_update_fn)(XXH3_state_t *, const void *, size_t);
typedef XXH128_hash_t (*XXH3_128bits_digest_fn)(const XXH3_state_t *);

typedef int (*XXH128_isEqual_fn)(XXH128_hash_t, XXH128_hash_t);
typedef int (*XXH128_cmp_fn)(const void *, const void *);
typedef void (*XXH128_canonicalFromHash_fn)(XXH128_canonical_t *, XXH128_hash_t);
typedef XXH128_hash_t (*XXH128_hashFromCanonical_fn)(const XXH128_canonical_t *);
typedef XXH128_hash_t (*XXH128_fn)(const void *, size_t, uint64_t);
typedef XXH_errorcode (*XXH3_generateSecret_fn)(void *, size_t, const void *, size_t);
typedef void (*XXH3_generateSecret_fromSeed_fn)(void *, uint64_t);

typedef struct XxHashApi {
    XXH_versionNumber_fn XXH_versionNumber;
    XXH32_fn XXH32;
    XXH32_createState_fn XXH32_createState;
    XXH32_freeState_fn XXH32_freeState;
    XXH32_copyState_fn XXH32_copyState;
    XXH32_reset_fn XXH32_reset;
    XXH32_update_fn XXH32_update;
    XXH32_digest_fn XXH32_digest;
    XXH32_canonicalFromHash_fn XXH32_canonicalFromHash;
    XXH32_hashFromCanonical_fn XXH32_hashFromCanonical;
    XXH64_fn XXH64;
    XXH64_createState_fn XXH64_createState;
    XXH64_freeState_fn XXH64_freeState;
    XXH64_copyState_fn XXH64_copyState;
    XXH64_reset_fn XXH64_reset;
    XXH64_update_fn XXH64_update;
    XXH64_digest_fn XXH64_digest;
    XXH64_canonicalFromHash_fn XXH64_canonicalFromHash;
    XXH64_hashFromCanonical_fn XXH64_hashFromCanonical;
    XXH3_64bits_fn XXH3_64bits;
    XXH3_64bits_withSeed_fn XXH3_64bits_withSeed;
    XXH3_64bits_withSecret_fn XXH3_64bits_withSecret;
    XXH3_64bits_withSecretandSeed_fn XXH3_64bits_withSecretandSeed;
    XXH3_createState_fn XXH3_createState;
    XXH3_freeState_fn XXH3_freeState;
    XXH3_copyState_fn XXH3_copyState;
    XXH3_64bits_reset_fn XXH3_64bits_reset;
    XXH3_64bits_reset_withSeed_fn XXH3_64bits_reset_withSeed;
    XXH3_64bits_reset_withSecret_fn XXH3_64bits_reset_withSecret;
    XXH3_64bits_reset_withSecretandSeed_fn XXH3_64bits_reset_withSecretandSeed;
    XXH3_64bits_update_fn XXH3_64bits_update;
    XXH3_64bits_digest_fn XXH3_64bits_digest;
    XXH3_128bits_fn XXH3_128bits;
    XXH3_128bits_withSeed_fn XXH3_128bits_withSeed;
    XXH3_128bits_withSecret_fn XXH3_128bits_withSecret;
    XXH3_128bits_withSecretandSeed_fn XXH3_128bits_withSecretandSeed;
    XXH3_128bits_reset_fn XXH3_128bits_reset;
    XXH3_128bits_reset_withSeed_fn XXH3_128bits_reset_withSeed;
    XXH3_128bits_reset_withSecret_fn XXH3_128bits_reset_withSecret;
    XXH3_128bits_reset_withSecretandSeed_fn XXH3_128bits_reset_withSecretandSeed;
    XXH3_128bits_update_fn XXH3_128bits_update;
    XXH3_128bits_digest_fn XXH3_128bits_digest;
    XXH128_isEqual_fn XXH128_isEqual;
    XXH128_cmp_fn XXH128_cmp;
    XXH128_canonicalFromHash_fn XXH128_canonicalFromHash;
    XXH128_hashFromCanonical_fn XXH128_hashFromCanonical;
    XXH128_fn XXH128;
    XXH3_generateSecret_fn XXH3_generateSecret;
    XXH3_generateSecret_fromSeed_fn XXH3_generateSecret_fromSeed;
} XxHashApi;

typedef struct KnownAnswerVector {
    size_t length;
    uint32_t xxh32;
    uint64_t xxh64;
    uint64_t xxh3_64;
    uint64_t xxh3_64_seed;
    uint64_t xxh3_64_secret;
    uint64_t xxh3_64_secret_seed;
    uint64_t xxh3_128_high;
    uint64_t xxh3_128_low;
    uint64_t xxh3_128_seed_high;
    uint64_t xxh3_128_seed_low;
    uint64_t xxh3_128_secret_high;
    uint64_t xxh3_128_secret_low;
    uint64_t xxh3_128_secret_seed_high;
    uint64_t xxh3_128_secret_seed_low;
} KnownAnswerVector;

typedef struct SharedLibrary {
#if defined(_WIN32)
    HMODULE handle;
#else
    void *handle;
#endif
} SharedLibrary;

static const uint32_t Seed32 = UINT32_C(0x9E3779B1);
static const uint64_t Seed64 = UINT64_C(0x9E3779B185EBCA87);
static const size_t Xxh3SecretSizeMin = 136;
static const size_t Xxh3SecretDefaultSize = 192;
static const char Payload[] = "xxHash is built for very fast, non-cryptographic hashing.";
static const char SecretMaterial[] = "application-specific secret material for the demo";
static int CheckedCases = 0;

static const KnownAnswerVector KnownAnswerVectors[] = {
    {
        0,
        UINT32_C(0x36B78AE7),
        UINT64_C(0x6EC6D05F61C7E7A7),
        UINT64_C(0x2D06800538D394C2),
        UINT64_C(0x07F70F819703314D),
        UINT64_C(0x86EB764D11B4FDEB),
        UINT64_C(0x07F70F819703314D),
        UINT64_C(0x99AA06D3014798D8),
        UINT64_C(0x6001C324468D497F),
        UINT64_C(0x45EF6DDC7AFB225A),
        UINT64_C(0xF9ECE1036ECBB2ED),
        UINT64_C(0x65644854738D9DF0),
        UINT64_C(0xCCC34E65D681BF28),
        UINT64_C(0x45EF6DDC7AFB225A),
        UINT64_C(0xF9ECE1036ECBB2ED),
    },
    {
        31,
        UINT32_C(0x35E9C2F1),
        UINT64_C(0x72360FC425549DCF),
        UINT64_C(0x91F3E7DDD073ABE6),
        UINT64_C(0xF28BE02AF2F0BE6A),
        UINT64_C(0xC8E006D6EFCD6879),
        UINT64_C(0xF28BE02AF2F0BE6A),
        UINT64_C(0x73F882751EE37F9B),
        UINT64_C(0x1EF8DA5301EEBFB1),
        UINT64_C(0x9CDD10BCD3282B07),
        UINT64_C(0xCA06CF4E8BDC4493),
        UINT64_C(0x5F2BDFFF5AB376FF),
        UINT64_C(0x92C0A0B37EA94CBE),
        UINT64_C(0x9CDD10BCD3282B07),
        UINT64_C(0xCA06CF4E8BDC4493),
    },
    {
        241,
        UINT32_C(0x3D4B61B3),
        UINT64_C(0xBFC062B1D4C76A43),
        UINT64_C(0xED93572E52ACAC83),
        UINT64_C(0xC57A17FAE0736FD1),
        UINT64_C(0xDC79124023E1C0AC),
        UINT64_C(0xDC79124023E1C0AC),
        UINT64_C(0x06229596E1DE710A),
        UINT64_C(0xED93572E52ACAC83),
        UINT64_C(0x097B0293916E8E9C),
        UINT64_C(0xC57A17FAE0736FD1),
        UINT64_C(0x4F6844C1B6B1642D),
        UINT64_C(0xDC79124023E1C0AC),
        UINT64_C(0x4F6844C1B6B1642D),
        UINT64_C(0xDC79124023E1C0AC),
    },
};

static const unsigned char Expected128Default[16] = {
    0x19, 0x55, 0x96, 0x7F, 0xF8, 0xC1, 0xAD, 0xCD, 0xF4, 0xFA, 0x1D, 0xD8, 0x65, 0xF9, 0x44, 0xE7
};

static const unsigned char Expected128Seed[16] = {
    0x08, 0x23, 0xDC, 0x30, 0xAF, 0xBD, 0x82, 0x51, 0x49, 0x60, 0x4A, 0x40, 0x20, 0xF4, 0xD0, 0xB1
};

static const unsigned char Expected128Secret[16] = {
    0xF2, 0xA7, 0x8F, 0xAB, 0xC1, 0x2E, 0x7E, 0x93, 0xDC, 0xFF, 0xF2, 0x52, 0x0C, 0x09, 0x94, 0xD2
};

static const unsigned char Expected128Empty[16] = {
    0x99, 0xAA, 0x06, 0xD3, 0x01, 0x47, 0x98, 0xD8, 0x60, 0x01, 0xC3, 0x24, 0x46, 0x8D, 0x49, 0x7F
};

static const unsigned char ExpectedCustomSecret[136] = {
    0xE9, 0x3D, 0x25, 0x86, 0xE9, 0x31, 0x98, 0x2F, 0x48, 0xCF, 0x63, 0x95, 0x6B, 0x23, 0x05, 0x74,
    0x01, 0xEF, 0x92, 0xCC, 0x53, 0xDE, 0x03, 0x0A, 0xB1, 0x12, 0x44, 0x3D, 0xE3, 0x18, 0xA1, 0x15,
    0x05, 0x18, 0xDF, 0x2A, 0x9C, 0xA8, 0x89, 0x22, 0xB6, 0x65, 0xC2, 0x8C, 0xEB, 0xF6, 0xD7, 0x75,
    0x54, 0xE9, 0x96, 0xDE, 0x73, 0x06, 0xD5, 0x07, 0x6A, 0xEA, 0x40, 0xE8, 0x57, 0xAB, 0x4B, 0x93,
    0xE6, 0x76, 0xDD, 0xC7, 0xE7, 0x68, 0x94, 0x4D, 0x0E, 0xFF, 0x04, 0xEB, 0x76, 0x98, 0x42, 0x77,
    0x1F, 0xE8, 0x26, 0x2C, 0x7D, 0x53, 0xAB, 0x7D, 0x20, 0x75, 0xFE, 0x48, 0x61, 0xAE, 0x6D, 0xE6,
    0x9E, 0x2D, 0xE7, 0x25, 0x19, 0x8D, 0x51, 0xC3, 0xF5, 0x50, 0xB5, 0x14, 0x55, 0xBE, 0xD0, 0xAA,
    0x92, 0x59, 0x7D, 0x32, 0x23, 0x2B, 0x43, 0x2D, 0x28, 0xC9, 0x78, 0x53, 0xF1, 0x13, 0xEE, 0x76,
    0xCE, 0x19, 0x9D, 0x97, 0x40, 0x29, 0x78, 0x90
};

static const unsigned char ExpectedSeededSecret[192] = {
    0x3F, 0xC9, 0x58, 0xBF, 0xD4, 0x1D, 0x83, 0x5C, 0xF5, 0x36, 0x95, 0xA6, 0x45, 0xA8, 0x75, 0x7E,
    0x65, 0x9F, 0x59, 0x6F, 0x35, 0x0A, 0xCF, 0x79, 0xEB, 0x75, 0xB8, 0x1E, 0x06, 0x3A, 0x30, 0x81,
    0x52, 0x44, 0xD2, 0xD4, 0x7D, 0x3A, 0x1D, 0x17, 0xFB, 0x8F, 0xE4, 0xF7, 0x1A, 0x86, 0x3B, 0x83,
    0x3F, 0xD3, 0x31, 0xFA, 0xA8, 0xBD, 0x5B, 0x2C, 0x59, 0x6B, 0xA4, 0x60, 0xD0, 0xC0, 0xEE, 0xAD,
    0xC3, 0xF2, 0x3D, 0x41, 0x43, 0x3D, 0x38, 0x69, 0x01, 0x06, 0x7A, 0x05, 0x6A, 0xD9, 0xF6, 0x04,
    0xF8, 0x2E, 0x34, 0x1D, 0x54, 0x87, 0x30, 0xED, 0xB1, 0x4E, 0x03, 0xC1, 0xF7, 0x64, 0x75, 0x3A,
    0x2F, 0xC5, 0x62, 0xC5, 0x94, 0x16, 0x6C, 0xDD, 0x72, 0x12, 0xD0, 0x41, 0x16, 0x92, 0x17, 0x7F,
    0x11, 0x1C, 0xCC, 0xD1, 0x7E, 0x2E, 0x91, 0xCF, 0x41, 0xD5, 0x92, 0x43, 0x28, 0xFF, 0x3B, 0xC6,
    0x71, 0x90, 0x98, 0x09, 0xE6, 0x4C, 0x23, 0x62, 0x3E, 0xB7, 0xB4, 0x79, 0x49, 0x9A, 0x2B, 0x4D,
    0x9E, 0xD7, 0xC8, 0xD7, 0x68, 0x6A, 0x12, 0xE8, 0x4C, 0x4C, 0x69, 0xA0, 0x77, 0x5A, 0x31, 0x00,
    0xB2, 0xE0, 0xA9, 0xDE, 0x2E, 0xC1, 0xD8, 0x9A, 0x08, 0x2E, 0xCD, 0x4B, 0xC9, 0x56, 0xFA, 0x2F,
    0xCC, 0x95, 0x26, 0x15, 0x47, 0x90, 0x3B, 0xC6, 0x28, 0x0D, 0x10, 0x45, 0x0A, 0xD2, 0x08, 0xE0
};

static void bytes_to_hex(const unsigned char *bytes, size_t size, char *text)
{
    static const char Hex[] = "0123456789ABCDEF";
    size_t i;
    for (i = 0; i < size; i++) {
        text[i * 2] = Hex[bytes[i] >> 4];
        text[i * 2 + 1] = Hex[bytes[i] & 0x0F];
    }
    text[size * 2] = '\0';
}

static void write_be64(uint64_t value, unsigned char *bytes)
{
    int i;
    for (i = 7; i >= 0; i--) {
        bytes[i] = (unsigned char)value;
        value >>= 8;
    }
}

static void expected_128(uint64_t high64, uint64_t low64, unsigned char expected[16])
{
    write_be64(high64, expected);
    write_be64(low64, expected + 8);
}

static void known_answer_payload(unsigned char *payload, size_t length)
{
    size_t i;
    for (i = 0; i < length; i++)
        payload[i] = (unsigned char)(((i * 131u) + 17u) & 0xFFu);
}

static void known_answer_material(unsigned char *material, size_t length)
{
    size_t i;
    for (i = 0; i < length; i++)
        material[i] = (unsigned char)(((i * 73u) + 41u) & 0xFFu);
}

static int fail_missing_symbol(const char *name)
{
    fprintf(stderr, "missing required xxHash symbol: %s\n", name);
    return 2;
}

static int fail_load_library(const char *path)
{
#if defined(_WIN32)
    fprintf(stderr, "failed to load xxHash library '%s': Windows error %lu\n", path, (unsigned long)GetLastError());
#else
    const char *error = dlerror();
    fprintf(stderr, "failed to load xxHash library '%s': %s\n", path, error != NULL ? error : "unknown dlopen error");
#endif
    return 2;
}

static int fail_errorcode(const char *function, const char *case_name, size_t length, const char *mode, XXH_errorcode code)
{
    fprintf(
        stderr,
        "%s failed for case=%s length=%llu mode=%s: expected XXH_OK(0), actual=%d\n",
        function,
        case_name,
        (unsigned long long)length,
        mode,
        (int)code);
    return 3;
}

static int fail_null_state(const char *function, const char *case_name)
{
    fprintf(stderr, "%s failed for case=%s: returned null state\n", function, case_name);
    return 3;
}

static int fail_u32(const char *function, const char *case_name, size_t length, const char *mode, uint32_t expected, uint32_t actual)
{
    fprintf(
        stderr,
        "%s mismatch for case=%s length=%llu mode=%s: expected=0x%08" PRIX32 " actual=0x%08" PRIX32 "\n",
        function,
        case_name,
        (unsigned long long)length,
        mode,
        expected,
        actual);
    return 3;
}

static int fail_u64(const char *function, const char *case_name, size_t length, const char *mode, uint64_t expected, uint64_t actual)
{
    fprintf(
        stderr,
        "%s mismatch for case=%s length=%llu mode=%s: expected=0x%016" PRIX64 " actual=0x%016" PRIX64 "\n",
        function,
        case_name,
        (unsigned long long)length,
        mode,
        expected,
        actual);
    return 3;
}

static int fail_bytes(
    const char *function,
    const char *case_name,
    size_t length,
    const char *mode,
    const unsigned char *expected,
    const unsigned char *actual,
    size_t size)
{
    char expected_text[385];
    char actual_text[385];
    bytes_to_hex(expected, size, expected_text);
    bytes_to_hex(actual, size, actual_text);
    fprintf(
        stderr,
        "%s mismatch for case=%s length=%llu mode=%s: expected=%s actual=%s\n",
        function,
        case_name,
        (unsigned long long)length,
        mode,
        expected_text,
        actual_text);
    return 3;
}

static int fail_int(const char *function, const char *case_name, const char *mode, int expected, int actual)
{
    fprintf(stderr, "%s mismatch for case=%s mode=%s: expected=%d actual=%d\n", function, case_name, mode, expected, actual);
    return 3;
}

static int check_int(const char *function, const char *case_name, const char *mode, int expected, int actual)
{
    CheckedCases++;
    return expected == actual ? 0 : fail_int(function, case_name, mode, expected, actual);
}

static int check_u32(
    const char *function,
    const char *case_name,
    size_t length,
    const char *mode,
    uint32_t expected,
    uint32_t actual)
{
    CheckedCases++;
    return expected == actual ? 0 : fail_u32(function, case_name, length, mode, expected, actual);
}

static int check_u64(
    const char *function,
    const char *case_name,
    size_t length,
    const char *mode,
    uint64_t expected,
    uint64_t actual)
{
    CheckedCases++;
    return expected == actual ? 0 : fail_u64(function, case_name, length, mode, expected, actual);
}

static int check_error(
    const char *function,
    const char *case_name,
    size_t length,
    const char *mode,
    XXH_errorcode code)
{
    CheckedCases++;
    return code == XXH_OK ? 0 : fail_errorcode(function, case_name, length, mode, code);
}

static int check_bytes(
    const char *function,
    const char *case_name,
    size_t length,
    const char *mode,
    const unsigned char *expected,
    const unsigned char *actual,
    size_t size)
{
    CheckedCases++;
    return memcmp(expected, actual, size) == 0 ? 0 : fail_bytes(function, case_name, length, mode, expected, actual, size);
}

static int check_128(
    const XxHashApi *api,
    const char *function,
    const char *case_name,
    size_t length,
    const char *mode,
    const unsigned char expected[16],
    XXH128_hash_t actual)
{
    XXH128_canonical_t canonical;
    api->XXH128_canonicalFromHash(&canonical, actual);
    return check_bytes(function, case_name, length, mode, expected, canonical.digest, sizeof(canonical.digest));
}

static int check_128_parts(
    const XxHashApi *api,
    const char *function,
    const char *case_name,
    size_t length,
    const char *mode,
    uint64_t high64,
    uint64_t low64,
    XXH128_hash_t actual)
{
    unsigned char expected[16];
    expected_128(high64, low64, expected);
    return check_128(api, function, case_name, length, mode, expected, actual);
}

static int verify_known_answer_vector(
    const XxHashApi *api,
    const KnownAnswerVector *vector,
    const unsigned char *payload,
    const unsigned char *secret)
{
    int result = check_u32(
        "XXH32",
        "known-answer",
        vector->length,
        "seed",
        vector->xxh32,
        api->XXH32(payload, vector->length, Seed32));
    if (result == 0)
        result = check_u64("XXH64", "known-answer", vector->length, "seed", vector->xxh64, api->XXH64(payload, vector->length, Seed64));
    if (result == 0)
        result = check_u64(
            "XXH3_64bits",
            "known-answer",
            vector->length,
            "default",
            vector->xxh3_64,
            api->XXH3_64bits(payload, vector->length));
    if (result == 0)
        result = check_u64(
            "XXH3_64bits_withSeed",
            "known-answer",
            vector->length,
            "seed",
            vector->xxh3_64_seed,
            api->XXH3_64bits_withSeed(payload, vector->length, Seed64));
    if (result == 0)
        result = check_u64(
            "XXH3_64bits_withSecret",
            "known-answer",
            vector->length,
            "secret",
            vector->xxh3_64_secret,
            api->XXH3_64bits_withSecret(payload, vector->length, secret, Xxh3SecretDefaultSize));
    if (result == 0)
        result = check_u64(
            "XXH3_64bits_withSecretandSeed",
            "known-answer",
            vector->length,
            "secret+seed",
            vector->xxh3_64_secret_seed,
            api->XXH3_64bits_withSecretandSeed(payload, vector->length, secret, Xxh3SecretDefaultSize, Seed64));
    if (result == 0)
        result = check_128_parts(
            api,
            "XXH3_128bits",
            "known-answer",
            vector->length,
            "default",
            vector->xxh3_128_high,
            vector->xxh3_128_low,
            api->XXH3_128bits(payload, vector->length));
    if (result == 0)
        result = check_128_parts(
            api,
            "XXH3_128bits_withSeed",
            "known-answer",
            vector->length,
            "seed",
            vector->xxh3_128_seed_high,
            vector->xxh3_128_seed_low,
            api->XXH3_128bits_withSeed(payload, vector->length, Seed64));
    if (result == 0)
        result = check_128_parts(
            api,
            "XXH3_128bits_withSecret",
            "known-answer",
            vector->length,
            "secret",
            vector->xxh3_128_secret_high,
            vector->xxh3_128_secret_low,
            api->XXH3_128bits_withSecret(payload, vector->length, secret, Xxh3SecretDefaultSize));
    if (result == 0)
        result = check_128_parts(
            api,
            "XXH3_128bits_withSecretandSeed",
            "known-answer",
            vector->length,
            "secret+seed",
            vector->xxh3_128_secret_seed_high,
            vector->xxh3_128_secret_seed_low,
            api->XXH3_128bits_withSecretandSeed(payload, vector->length, secret, Xxh3SecretDefaultSize, Seed64));
    return result;
}

static int verify_known_answer_subset(const XxHashApi *api)
{
    unsigned char material[64];
    unsigned char secret[192];
    unsigned char payload[241];
    size_t i;
    int result;

    known_answer_material(material, sizeof(material));
    result = check_error(
        "XXH3_generateSecret",
        "known-answer-material",
        sizeof(material),
        "default-size",
        api->XXH3_generateSecret(secret, sizeof(secret), material, sizeof(material)));
    if (result != 0)
        return result;

    for (i = 0; i < sizeof(KnownAnswerVectors) / sizeof(KnownAnswerVectors[0]); i++) {
        const KnownAnswerVector *vector = &KnownAnswerVectors[i];
        known_answer_payload(payload, vector->length);
        result = verify_known_answer_vector(api, vector, payload, secret);
        if (result != 0)
            return result;
    }

    return 0;
}

static int load_library(const char *path, SharedLibrary *library)
{
#if defined(_WIN32)
    library->handle = LoadLibraryA(path);
#else
    library->handle = dlopen(path, RTLD_NOW | RTLD_LOCAL);
#endif
    return library->handle != NULL ? 0 : fail_load_library(path);
}

static void close_library(SharedLibrary *library)
{
    if (library->handle == NULL)
        return;
#if defined(_WIN32)
    FreeLibrary(library->handle);
#else
    dlclose(library->handle);
#endif
    library->handle = NULL;
}

static int load_symbol_function(SharedLibrary *library, const char *name, void *target, size_t target_size)
{
#if defined(_WIN32)
    FARPROC symbol = GetProcAddress(library->handle, name);
#else
    void *symbol = dlsym(library->handle, name);
#endif
    if (symbol == NULL)
        return fail_missing_symbol(name);
    if (sizeof(symbol) > target_size) {
        fprintf(stderr, "cannot store symbol %s: loader pointer is larger than function pointer storage\n", name);
        return 2;
    }

    memset(target, 0, target_size);
    memcpy(target, &symbol, sizeof(symbol));
    return 0;
}

#define LOAD_SYMBOL(api, library, name)                                           \
    do {                                                                          \
        int load_result = load_symbol_function(library, #name, &(api)->name, sizeof((api)->name)); \
        if (load_result != 0)                                                     \
            return load_result;                                                   \
    } while (0)

static int load_api(SharedLibrary *library, XxHashApi *api)
{
    memset(api, 0, sizeof(*api));
    LOAD_SYMBOL(api, library, XXH_versionNumber);
    LOAD_SYMBOL(api, library, XXH32);
    LOAD_SYMBOL(api, library, XXH32_createState);
    LOAD_SYMBOL(api, library, XXH32_freeState);
    LOAD_SYMBOL(api, library, XXH32_copyState);
    LOAD_SYMBOL(api, library, XXH32_reset);
    LOAD_SYMBOL(api, library, XXH32_update);
    LOAD_SYMBOL(api, library, XXH32_digest);
    LOAD_SYMBOL(api, library, XXH32_canonicalFromHash);
    LOAD_SYMBOL(api, library, XXH32_hashFromCanonical);
    LOAD_SYMBOL(api, library, XXH64);
    LOAD_SYMBOL(api, library, XXH64_createState);
    LOAD_SYMBOL(api, library, XXH64_freeState);
    LOAD_SYMBOL(api, library, XXH64_copyState);
    LOAD_SYMBOL(api, library, XXH64_reset);
    LOAD_SYMBOL(api, library, XXH64_update);
    LOAD_SYMBOL(api, library, XXH64_digest);
    LOAD_SYMBOL(api, library, XXH64_canonicalFromHash);
    LOAD_SYMBOL(api, library, XXH64_hashFromCanonical);
    LOAD_SYMBOL(api, library, XXH3_64bits);
    LOAD_SYMBOL(api, library, XXH3_64bits_withSeed);
    LOAD_SYMBOL(api, library, XXH3_64bits_withSecret);
    LOAD_SYMBOL(api, library, XXH3_createState);
    LOAD_SYMBOL(api, library, XXH3_freeState);
    LOAD_SYMBOL(api, library, XXH3_copyState);
    LOAD_SYMBOL(api, library, XXH3_64bits_reset);
    LOAD_SYMBOL(api, library, XXH3_64bits_reset_withSeed);
    LOAD_SYMBOL(api, library, XXH3_64bits_reset_withSecret);
    LOAD_SYMBOL(api, library, XXH3_64bits_update);
    LOAD_SYMBOL(api, library, XXH3_64bits_digest);
    LOAD_SYMBOL(api, library, XXH3_128bits);
    LOAD_SYMBOL(api, library, XXH3_128bits_withSeed);
    LOAD_SYMBOL(api, library, XXH3_128bits_withSecret);
    LOAD_SYMBOL(api, library, XXH3_128bits_reset);
    LOAD_SYMBOL(api, library, XXH3_128bits_reset_withSeed);
    LOAD_SYMBOL(api, library, XXH3_128bits_reset_withSecret);
    LOAD_SYMBOL(api, library, XXH3_128bits_update);
    LOAD_SYMBOL(api, library, XXH3_128bits_digest);
    LOAD_SYMBOL(api, library, XXH128_isEqual);
    LOAD_SYMBOL(api, library, XXH128_cmp);
    LOAD_SYMBOL(api, library, XXH128_canonicalFromHash);
    LOAD_SYMBOL(api, library, XXH128_hashFromCanonical);
    LOAD_SYMBOL(api, library, XXH128);
    LOAD_SYMBOL(api, library, XXH3_generateSecret);
    LOAD_SYMBOL(api, library, XXH3_generateSecret_fromSeed);
    LOAD_SYMBOL(api, library, XXH3_64bits_withSecretandSeed);
    LOAD_SYMBOL(api, library, XXH3_128bits_withSecretandSeed);
    LOAD_SYMBOL(api, library, XXH3_64bits_reset_withSecretandSeed);
    LOAD_SYMBOL(api, library, XXH3_128bits_reset_withSecretandSeed);
    return 0;
}

#undef LOAD_SYMBOL

static int verify_xxh32(const XxHashApi *api, const unsigned char *payload, size_t length)
{
    XXH32_state_t *state;
    XXH32_state_t *copy;
    XXH32_canonical_t canonical;
    uint32_t one_shot = api->XXH32(payload, length, Seed32);
    int result;

    result = check_u32("XXH32", "payload", length, "seed", UINT32_C(0x0BB86A6C), one_shot);
    if (result != 0)
        return result;
    result = check_u32("XXH32", "empty", 0, "seed", UINT32_C(0x36B78AE7), api->XXH32(NULL, 0, Seed32));
    if (result != 0)
        return result;

    state = api->XXH32_createState();
    copy = api->XXH32_createState();
    if (state == NULL)
        return fail_null_state("XXH32_createState", "payload");
    if (copy == NULL) {
        api->XXH32_freeState(state);
        return fail_null_state("XXH32_createState", "payload-copy");
    }

    result = check_error("XXH32_reset", "payload", length, "seed", api->XXH32_reset(state, Seed32));
    if (result == 0)
        result = check_error("XXH32_update", "payload", 28, "stream-first", api->XXH32_update(state, payload, 28));
    if (result != 0) {
        api->XXH32_freeState(copy);
        api->XXH32_freeState(state);
        return result;
    }

    api->XXH32_copyState(copy, state);
    result = check_error("XXH32_update", "payload", length - 28, "stream-second", api->XXH32_update(state, payload + 28, length - 28));
    if (result == 0)
        result = check_error("XXH32_update", "payload", length - 28, "copy-second", api->XXH32_update(copy, payload + 28, length - 28));
    if (result == 0)
        result = check_u32("XXH32_digest", "payload", length, "stream", one_shot, api->XXH32_digest(state));
    if (result == 0)
        result = check_u32("XXH32_digest", "payload", length, "copy", one_shot, api->XXH32_digest(copy));

    api->XXH32_freeState(copy);
    api->XXH32_freeState(state);
    if (result != 0)
        return result;

    api->XXH32_canonicalFromHash(&canonical, one_shot);
    result = check_bytes("XXH32_canonicalFromHash", "payload", length, "big-endian", (const unsigned char *)"\x0B\xB8\x6A\x6C", canonical.digest, 4);
    if (result != 0)
        return result;
    return check_u32("XXH32_hashFromCanonical", "payload", length, "roundtrip", one_shot, api->XXH32_hashFromCanonical(&canonical));
}

static int verify_xxh64(const XxHashApi *api, const unsigned char *payload, size_t length)
{
    XXH64_state_t *state;
    XXH64_state_t *copy;
    XXH64_canonical_t canonical;
    const unsigned char expected_canonical[8] = {0xB6, 0x57, 0xAC, 0xBD, 0x6B, 0x8F, 0xA1, 0x29};
    uint64_t one_shot = api->XXH64(payload, length, Seed64);
    int result;

    result = check_u64("XXH64", "payload", length, "seed", UINT64_C(0xB657ACBD6B8FA129), one_shot);
    if (result != 0)
        return result;
    result = check_u64("XXH64", "empty", 0, "seed", UINT64_C(0x6EC6D05F61C7E7A7), api->XXH64(NULL, 0, Seed64));
    if (result != 0)
        return result;

    state = api->XXH64_createState();
    copy = api->XXH64_createState();
    if (state == NULL)
        return fail_null_state("XXH64_createState", "payload");
    if (copy == NULL) {
        api->XXH64_freeState(state);
        return fail_null_state("XXH64_createState", "payload-copy");
    }

    result = check_error("XXH64_reset", "payload", length, "seed", api->XXH64_reset(state, Seed64));
    if (result == 0)
        result = check_error("XXH64_update", "payload", 28, "stream-first", api->XXH64_update(state, payload, 28));
    if (result != 0) {
        api->XXH64_freeState(copy);
        api->XXH64_freeState(state);
        return result;
    }

    api->XXH64_copyState(copy, state);
    result = check_error("XXH64_update", "payload", length - 28, "stream-second", api->XXH64_update(state, payload + 28, length - 28));
    if (result == 0)
        result = check_error("XXH64_update", "payload", length - 28, "copy-second", api->XXH64_update(copy, payload + 28, length - 28));
    if (result == 0)
        result = check_u64("XXH64_digest", "payload", length, "stream", one_shot, api->XXH64_digest(state));
    if (result == 0)
        result = check_u64("XXH64_digest", "payload", length, "copy", one_shot, api->XXH64_digest(copy));

    api->XXH64_freeState(copy);
    api->XXH64_freeState(state);
    if (result != 0)
        return result;

    api->XXH64_canonicalFromHash(&canonical, one_shot);
    result = check_bytes("XXH64_canonicalFromHash", "payload", length, "big-endian", expected_canonical, canonical.digest, sizeof(canonical.digest));
    if (result != 0)
        return result;
    return check_u64("XXH64_hashFromCanonical", "payload", length, "roundtrip", one_shot, api->XXH64_hashFromCanonical(&canonical));
}

static int verify_secret_generation(const XxHashApi *api, unsigned char *custom_secret, unsigned char *seeded_secret)
{
    int result = check_error(
        "XXH3_generateSecret",
        "secret-material",
        sizeof(SecretMaterial) - 1,
        "custom",
        api->XXH3_generateSecret(custom_secret, Xxh3SecretSizeMin, SecretMaterial, sizeof(SecretMaterial) - 1));
    if (result != 0)
        return result;

    result = check_bytes(
        "XXH3_generateSecret",
        "secret-material",
        sizeof(SecretMaterial) - 1,
        "custom",
        ExpectedCustomSecret,
        custom_secret,
        Xxh3SecretSizeMin);
    if (result != 0)
        return result;

    api->XXH3_generateSecret_fromSeed(seeded_secret, Seed64);
    return check_bytes(
        "XXH3_generateSecret_fromSeed",
        "seed64",
        0,
        "seed",
        ExpectedSeededSecret,
        seeded_secret,
        Xxh3SecretDefaultSize);
}

static int verify_xxh3_64(
    const XxHashApi *api,
    const unsigned char *payload,
    size_t length,
    const unsigned char *custom_secret)
{
    XXH3_state_t *state;
    XXH3_state_t *copy;
    uint64_t expected_default = UINT64_C(0x4DA6687A1E6727E8);
    uint64_t expected_seed = UINT64_C(0xB5330B0336F75F0B);
    uint64_t expected_secret = UINT64_C(0xAAC5884938086B46);
    int result;

    result = check_u64("XXH3_64bits", "payload", length, "default", expected_default, api->XXH3_64bits(payload, length));
    if (result == 0)
        result = check_u64("XXH3_64bits", "empty", 0, "default", UINT64_C(0x2D06800538D394C2), api->XXH3_64bits(NULL, 0));
    if (result == 0)
        result = check_u64("XXH3_64bits_withSeed", "payload", length, "seed", expected_seed, api->XXH3_64bits_withSeed(payload, length, Seed64));
    if (result == 0)
        result = check_u64(
            "XXH3_64bits_withSecret",
            "payload",
            length,
            "secret",
            expected_secret,
            api->XXH3_64bits_withSecret(payload, length, custom_secret, Xxh3SecretSizeMin));
    if (result == 0)
        result = check_u64(
            "XXH3_64bits_withSecretandSeed",
            "payload",
            length,
            "secret+seed",
            expected_seed,
            api->XXH3_64bits_withSecretandSeed(payload, length, custom_secret, Xxh3SecretSizeMin, Seed64));
    if (result != 0)
        return result;

    state = api->XXH3_createState();
    copy = api->XXH3_createState();
    if (state == NULL)
        return fail_null_state("XXH3_createState", "xxh3-64");
    if (copy == NULL) {
        api->XXH3_freeState(state);
        return fail_null_state("XXH3_createState", "xxh3-64-copy");
    }

    result = check_error("XXH3_64bits_reset", "payload", length, "default", api->XXH3_64bits_reset(state));
    if (result == 0)
        result = check_error("XXH3_64bits_update", "payload", 28, "stream-first", api->XXH3_64bits_update(state, payload, 28));
    if (result != 0) {
        api->XXH3_freeState(copy);
        api->XXH3_freeState(state);
        return result;
    }

    api->XXH3_copyState(copy, state);
    result = check_error("XXH3_64bits_update", "payload", length - 28, "stream-second", api->XXH3_64bits_update(state, payload + 28, length - 28));
    if (result == 0)
        result = check_error("XXH3_64bits_update", "payload", length - 28, "copy-second", api->XXH3_64bits_update(copy, payload + 28, length - 28));
    if (result == 0)
        result = check_u64("XXH3_64bits_digest", "payload", length, "stream", expected_default, api->XXH3_64bits_digest(state));
    if (result == 0)
        result = check_u64("XXH3_64bits_digest", "payload", length, "copy", expected_default, api->XXH3_64bits_digest(copy));
    if (result == 0)
        result = check_error("XXH3_64bits_reset_withSeed", "payload", length, "seed", api->XXH3_64bits_reset_withSeed(state, Seed64));
    if (result == 0)
        result = check_error("XXH3_64bits_update", "payload", length, "seed", api->XXH3_64bits_update(state, payload, length));
    if (result == 0)
        result = check_u64("XXH3_64bits_digest", "payload", length, "seed", expected_seed, api->XXH3_64bits_digest(state));
    if (result == 0)
        result = check_error(
            "XXH3_64bits_reset_withSecret",
            "payload",
            length,
            "secret",
            api->XXH3_64bits_reset_withSecret(state, custom_secret, Xxh3SecretSizeMin));
    if (result == 0)
        result = check_error("XXH3_64bits_update", "payload", length, "secret", api->XXH3_64bits_update(state, payload, length));
    if (result == 0)
        result = check_u64("XXH3_64bits_digest", "payload", length, "secret", expected_secret, api->XXH3_64bits_digest(state));
    if (result == 0)
        result = check_error(
            "XXH3_64bits_reset_withSecretandSeed",
            "payload",
            length,
            "secret+seed",
            api->XXH3_64bits_reset_withSecretandSeed(state, custom_secret, Xxh3SecretSizeMin, Seed64));
    if (result == 0)
        result = check_error("XXH3_64bits_update", "payload", length, "secret+seed", api->XXH3_64bits_update(state, payload, length));
    if (result == 0)
        result = check_u64("XXH3_64bits_digest", "payload", length, "secret+seed", expected_seed, api->XXH3_64bits_digest(state));

    api->XXH3_freeState(copy);
    api->XXH3_freeState(state);
    return result;
}

static int verify_xxh3_128(
    const XxHashApi *api,
    const unsigned char *payload,
    size_t length,
    const unsigned char *custom_secret)
{
    XXH3_state_t *state;
    XXH128_hash_t default_hash;
    XXH128_hash_t seed_hash;
    XXH128_hash_t secret_hash;
    XXH128_hash_t alias_hash;
    XXH128_canonical_t canonical;
    int comparison;
    int result;

    default_hash = api->XXH3_128bits(payload, length);
    seed_hash = api->XXH3_128bits_withSeed(payload, length, Seed64);
    secret_hash = api->XXH3_128bits_withSecret(payload, length, custom_secret, Xxh3SecretSizeMin);
    alias_hash = api->XXH128(payload, length, Seed64);

    result = check_128(api, "XXH3_128bits", "payload", length, "default", Expected128Default, default_hash);
    if (result == 0)
        result = check_128(api, "XXH3_128bits", "empty", 0, "default", Expected128Empty, api->XXH3_128bits(NULL, 0));
    if (result == 0)
        result = check_128(api, "XXH3_128bits_withSeed", "payload", length, "seed", Expected128Seed, seed_hash);
    if (result == 0)
        result = check_128(api, "XXH3_128bits_withSecret", "payload", length, "secret", Expected128Secret, secret_hash);
    if (result == 0)
        result = check_128(
            api,
            "XXH3_128bits_withSecretandSeed",
            "payload",
            length,
            "secret+seed",
            Expected128Seed,
            api->XXH3_128bits_withSecretandSeed(payload, length, custom_secret, Xxh3SecretSizeMin, Seed64));
    if (result == 0)
        result = check_128(api, "XXH128", "payload", length, "seed", Expected128Seed, alias_hash);
    if (result != 0)
        return result;

    state = api->XXH3_createState();
    if (state == NULL)
        return fail_null_state("XXH3_createState", "xxh3-128");

    result = check_error("XXH3_128bits_reset", "payload", length, "default", api->XXH3_128bits_reset(state));
    if (result == 0)
        result = check_error("XXH3_128bits_update", "payload", 28, "stream-first", api->XXH3_128bits_update(state, payload, 28));
    if (result == 0)
        result = check_error("XXH3_128bits_update", "payload", length - 28, "stream-second", api->XXH3_128bits_update(state, payload + 28, length - 28));
    if (result == 0)
        result = check_128(api, "XXH3_128bits_digest", "payload", length, "stream", Expected128Default, api->XXH3_128bits_digest(state));
    if (result == 0)
        result = check_error("XXH3_128bits_reset_withSeed", "payload", length, "seed", api->XXH3_128bits_reset_withSeed(state, Seed64));
    if (result == 0)
        result = check_error("XXH3_128bits_update", "payload", length, "seed", api->XXH3_128bits_update(state, payload, length));
    if (result == 0)
        result = check_128(api, "XXH3_128bits_digest", "payload", length, "seed", Expected128Seed, api->XXH3_128bits_digest(state));
    if (result == 0)
        result = check_error(
            "XXH3_128bits_reset_withSecret",
            "payload",
            length,
            "secret",
            api->XXH3_128bits_reset_withSecret(state, custom_secret, Xxh3SecretSizeMin));
    if (result == 0)
        result = check_error("XXH3_128bits_update", "payload", length, "secret", api->XXH3_128bits_update(state, payload, length));
    if (result == 0)
        result = check_128(api, "XXH3_128bits_digest", "payload", length, "secret", Expected128Secret, api->XXH3_128bits_digest(state));
    if (result == 0)
        result = check_error(
            "XXH3_128bits_reset_withSecretandSeed",
            "payload",
            length,
            "secret+seed",
            api->XXH3_128bits_reset_withSecretandSeed(state, custom_secret, Xxh3SecretSizeMin, Seed64));
    if (result == 0)
        result = check_error("XXH3_128bits_update", "payload", length, "secret+seed", api->XXH3_128bits_update(state, payload, length));
    if (result == 0)
        result = check_128(api, "XXH3_128bits_digest", "payload", length, "secret+seed", Expected128Seed, api->XXH3_128bits_digest(state));

    api->XXH3_freeState(state);
    if (result != 0)
        return result;

    result = check_int("XXH128_isEqual", "payload", "same", 1, api->XXH128_isEqual(default_hash, default_hash) != 0 ? 1 : 0);
    if (result == 0)
        result = check_int("XXH128_isEqual", "payload", "different", 0, api->XXH128_isEqual(default_hash, seed_hash) != 0 ? 1 : 0);
    comparison = api->XXH128_cmp(&default_hash, &seed_hash);
    if (result == 0)
        result = check_int("XXH128_cmp", "payload", "default-vs-seed-sign", 1, comparison > 0 ? 1 : comparison < 0 ? -1 : 0);
    comparison = api->XXH128_cmp(&seed_hash, &default_hash);
    if (result == 0)
        result = check_int("XXH128_cmp", "payload", "seed-vs-default-sign", -1, comparison > 0 ? 1 : comparison < 0 ? -1 : 0);
    if (result != 0)
        return result;

    api->XXH128_canonicalFromHash(&canonical, default_hash);
    result = check_bytes(
        "XXH128_canonicalFromHash",
        "payload",
        length,
        "big-endian",
        Expected128Default,
        canonical.digest,
        sizeof(canonical.digest));
    if (result != 0)
        return result;
    return check_128(api, "XXH128_hashFromCanonical", "payload", length, "roundtrip", Expected128Default, api->XXH128_hashFromCanonical(&canonical));
}

static int verify_unaligned(const XxHashApi *api)
{
    unsigned char prefixed[sizeof(Payload)];
    const unsigned char *unaligned;
    size_t length = sizeof(Payload) - 1;
    int result;

    prefixed[0] = '?';
    memcpy(prefixed + 1, Payload, length);
    unaligned = prefixed + 1;

    result = check_u32("XXH32", "payload+1", length, "unaligned+seed", UINT32_C(0x0BB86A6C), api->XXH32(unaligned, length, Seed32));
    if (result == 0)
        result = check_u64("XXH64", "payload+1", length, "unaligned+seed", UINT64_C(0xB657ACBD6B8FA129), api->XXH64(unaligned, length, Seed64));
    if (result == 0)
        result = check_u64("XXH3_64bits", "payload+1", length, "unaligned", UINT64_C(0x4DA6687A1E6727E8), api->XXH3_64bits(unaligned, length));
    if (result == 0)
        result = check_128(api, "XXH3_128bits", "payload+1", length, "unaligned", Expected128Default, api->XXH3_128bits(unaligned, length));
    return result;
}

static void json_write_string(FILE *file, const char *value)
{
    const unsigned char *cursor = (const unsigned char *)value;
    fputc('"', file);
    while (*cursor != '\0') {
        unsigned char c = *cursor++;
        if (c == '"' || c == '\\') {
            fputc('\\', file);
            fputc((int)c, file);
        } else if (c == '\b') {
            fputs("\\b", file);
        } else if (c == '\f') {
            fputs("\\f", file);
        } else if (c == '\n') {
            fputs("\\n", file);
        } else if (c == '\r') {
            fputs("\\r", file);
        } else if (c == '\t') {
            fputs("\\t", file);
        } else if (c < 0x20) {
            fprintf(file, "\\u%04X", (unsigned int)c);
        } else {
            fputc((int)c, file);
        }
    }
    fputc('"', file);
}

static int write_report(const char *report_path, const char *library_path, const char *rid, uint32_t version)
{
    FILE *file = fopen(report_path, "wb");
    if (file == NULL) {
        fprintf(stderr, "failed to open report '%s': %s\n", report_path, strerror(errno));
        return 4;
    }

    fputs("{\"status\":\"ok\",\"library\":", file);
    json_write_string(file, library_path);
    fputs(",\"rid\":", file);
    json_write_string(file, rid);
    fprintf(file, ",\"xxhashVersion\":%" PRIu32 ",\"cases\":%d}\n", version, CheckedCases);

    if (fclose(file) != 0) {
        fprintf(stderr, "failed to write report '%s': %s\n", report_path, strerror(errno));
        return 4;
    }

    return 0;
}

static int run_verification(const XxHashApi *api)
{
    const unsigned char *payload = (const unsigned char *)Payload;
    unsigned char custom_secret[136];
    unsigned char seeded_secret[192];
    size_t length = sizeof(Payload) - 1;
    uint32_t version;
    int result;

    version = api->XXH_versionNumber();
    result = check_u32("XXH_versionNumber", "version", 0, "runtime", UINT32_C(803), version);
    if (result != 0)
        return result;

    result = verify_secret_generation(api, custom_secret, seeded_secret);
    if (result != 0)
        return result;
    result = verify_known_answer_subset(api);
    if (result != 0)
        return result;
    result = verify_xxh32(api, payload, length);
    if (result != 0)
        return result;
    result = verify_xxh64(api, payload, length);
    if (result != 0)
        return result;
    result = verify_xxh3_64(api, payload, length, custom_secret);
    if (result != 0)
        return result;
    result = verify_xxh3_128(api, payload, length, custom_secret);
    if (result != 0)
        return result;
    return verify_unaligned(api);
}

int main(int argc, char **argv)
{
    SharedLibrary library;
    XxHashApi api;
    uint32_t version;
    int result;

    if (argc != 4) {
        fprintf(stderr, "usage: verify-xxhash <library-path> <report-path> <rid>\n");
        return 1;
    }

    library.handle = NULL;
    result = load_library(argv[1], &library);
    if (result != 0)
        return result;

    result = load_api(&library, &api);
    if (result == 0)
        result = run_verification(&api);
    if (result == 0) {
        version = api.XXH_versionNumber();
        result = write_report(argv[2], argv[1], argv[3], version);
    }

    close_library(&library);
    return result;
}
