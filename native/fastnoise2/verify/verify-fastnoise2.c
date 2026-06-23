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
#include <sys/stat.h>
#include <sys/types.h>
#endif

typedef void *FnNode;

typedef FnNode (*fnNewFromEncodedNodeTree_fn)(const char *, uint32_t);
typedef void (*fnDeleteNodeRef_fn)(FnNode);
typedef uint32_t (*fnGetActiveFeatureSet_fn)(FnNode);
typedef int (*fnGetMetadataID_fn)(FnNode);
typedef void (*fnGenUniformGrid2D_fn)(FnNode, float *, float, float, int, int, float, float, int, float *);
typedef void (*fnGenUniformGrid3D_fn)(FnNode, float *, float, float, float, int, int, int, float, float, float, int, float *);
typedef int (*fnGetMetadataCount_fn)(void);
typedef const char *(*fnGetMetadataName_fn)(int);
typedef FnNode (*fnNewFromMetadata_fn)(int, uint32_t);
typedef int (*fnGetMetadataVariableCount_fn)(int);
typedef const char *(*fnGetMetadataVariableName_fn)(int, int);
typedef unsigned char (*fnSetVariableFloat_fn)(FnNode, int, float);
typedef unsigned char (*fnSetVariableIntEnum_fn)(FnNode, int, int);
typedef int (*fnGetMetadataNodeLookupCount_fn)(int);
typedef const char *(*fnGetMetadataNodeLookupName_fn)(int, int);
typedef unsigned char (*fnSetNodeLookup_fn)(FnNode, int, FnNode);
typedef int (*fnGetMetadataHybridCount_fn)(int);
typedef const char *(*fnGetMetadataHybridName_fn)(int, int);
typedef unsigned char (*fnSetHybridFloat_fn)(FnNode, int, float);

typedef struct FastNoiseApi {
    fnNewFromEncodedNodeTree_fn fnNewFromEncodedNodeTree;
    fnDeleteNodeRef_fn fnDeleteNodeRef;
    fnGetActiveFeatureSet_fn fnGetActiveFeatureSet;
    fnGetMetadataID_fn fnGetMetadataID;
    fnGenUniformGrid2D_fn fnGenUniformGrid2D;
    fnGenUniformGrid3D_fn fnGenUniformGrid3D;
    fnGetMetadataCount_fn fnGetMetadataCount;
    fnGetMetadataName_fn fnGetMetadataName;
    fnNewFromMetadata_fn fnNewFromMetadata;
    fnGetMetadataVariableCount_fn fnGetMetadataVariableCount;
    fnGetMetadataVariableName_fn fnGetMetadataVariableName;
    fnSetVariableFloat_fn fnSetVariableFloat;
    fnSetVariableIntEnum_fn fnSetVariableIntEnum;
    fnGetMetadataNodeLookupCount_fn fnGetMetadataNodeLookupCount;
    fnGetMetadataNodeLookupName_fn fnGetMetadataNodeLookupName;
    fnSetNodeLookup_fn fnSetNodeLookup;
    fnGetMetadataHybridCount_fn fnGetMetadataHybridCount;
    fnGetMetadataHybridName_fn fnGetMetadataHybridName;
    fnSetHybridFloat_fn fnSetHybridFloat;
} FastNoiseApi;

typedef struct SharedLibrary {
#if defined(_WIN32)
    HMODULE handle;
#else
    void *handle;
#endif
} SharedLibrary;

typedef struct NodePair {
    FnNode root;
    FnNode owned_source;
} NodePair;

typedef enum FixtureGraphKind {
    FixtureGraphEncoded = 0,
    FixtureGraphMetadataSimplex = 1,
    FixtureGraphMetadataFbmSimplex = 2
} FixtureGraphKind;

typedef enum FixtureDimension {
    FixtureDimension2D = 2,
    FixtureDimension3D = 3
} FixtureDimension;

typedef struct Fixture {
    const char *name;
    FixtureGraphKind graph_kind;
    const char *encoded_tree;
    FixtureDimension dimensions;
    int x_count;
    int y_count;
    int z_count;
    float x_offset;
    float y_offset;
    float z_offset;
    float x_step;
    float y_step;
    float z_step;
    int seed;
    const char *expected_digest;
} Fixture;

typedef struct FixtureResult {
    const Fixture *fixture;
    const char *root_metadata_name;
    uint32_t active_feature_set;
    float min_value;
    float max_value;
    float sample_values[3];
    uint32_t sample_bits[3];
    size_t sample_indexes[3];
    char digest[17];
    int passed;
} FixtureResult;

static const char EncodedWikiNodeTree[] = "DQkGDA==";

static const Fixture Fixtures[] = {
    {
        "metadata-simplex-2d",
        FixtureGraphMetadataSimplex,
        NULL,
        FixtureDimension2D,
        9,
        4,
        1,
        -17.5f,
        4.25f,
        0.0f,
        0.625f,
        1.75f,
        1.0f,
        0,
        "99BCAB2C67431326",
    },
    {
        "encoded-wiki-2d",
        FixtureGraphEncoded,
        EncodedWikiNodeTree,
        FixtureDimension2D,
        7,
        5,
        1,
        -13.25f,
        7.5f,
        0.0f,
        0.75f,
        1.25f,
        1.0f,
        0,
        "A25EA23EAA75E68E",
    },
    {
        "encoded-wiki-3d",
        FixtureGraphEncoded,
        EncodedWikiNodeTree,
        FixtureDimension3D,
        6,
        4,
        3,
        -8.5f,
        2.25f,
        60.0f,
        1.0f,
        0.5f,
        1.5f,
        1337,
        "C116197CB2AAA561",
    },
    {
        "metadata-fbm-simplex-3d",
        FixtureGraphMetadataFbmSimplex,
        NULL,
        FixtureDimension3D,
        8,
        5,
        2,
        -11.0f,
        3.5f,
        60.0f,
        1.0f,
        1.0f,
        2.0f,
        -8675309,
        "303D3F700DE06D4E",
    },
};

static int CheckedCases = 0;

static int fail_missing_symbol(const char *name)
{
    fprintf(stderr, "missing required FastNoise2 symbol: %s\n", name);
    return 2;
}

static int fail_load_library(const char *path)
{
#if defined(_WIN32)
    fprintf(stderr, "failed to load FastNoise2 library '%s': Windows error %lu\n", path, (unsigned long)GetLastError());
#else
    const char *error = dlerror();
    fprintf(stderr, "failed to load FastNoise2 library '%s': %s\n", path, error != NULL ? error : "unknown dlopen error");
#endif
    return 2;
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
    void *symbol;
    dlerror();
    symbol = dlsym(library->handle, name);
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

#define LOAD_FASTNOISE_SYMBOL(api, name) \
    do { \
        int load_result = load_symbol_function(library, #name, &(api)->name, sizeof((api)->name)); \
        if (load_result != 0) \
            return load_result; \
    } while (0)

static int load_api(SharedLibrary *library, FastNoiseApi *api)
{
    memset(api, 0, sizeof(*api));
    LOAD_FASTNOISE_SYMBOL(api, fnNewFromEncodedNodeTree);
    LOAD_FASTNOISE_SYMBOL(api, fnDeleteNodeRef);
    LOAD_FASTNOISE_SYMBOL(api, fnGetActiveFeatureSet);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataID);
    LOAD_FASTNOISE_SYMBOL(api, fnGenUniformGrid2D);
    LOAD_FASTNOISE_SYMBOL(api, fnGenUniformGrid3D);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataCount);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataName);
    LOAD_FASTNOISE_SYMBOL(api, fnNewFromMetadata);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataVariableCount);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataVariableName);
    LOAD_FASTNOISE_SYMBOL(api, fnSetVariableFloat);
    LOAD_FASTNOISE_SYMBOL(api, fnSetVariableIntEnum);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataNodeLookupCount);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataNodeLookupName);
    LOAD_FASTNOISE_SYMBOL(api, fnSetNodeLookup);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataHybridCount);
    LOAD_FASTNOISE_SYMBOL(api, fnGetMetadataHybridName);
    LOAD_FASTNOISE_SYMBOL(api, fnSetHybridFloat);
    return 0;
}

static int ascii_lower(int c)
{
    return c >= 'A' && c <= 'Z' ? c + ('a' - 'A') : c;
}

static int string_equal_ignore_case(const char *left, const char *right)
{
    while (*left != '\0' && *right != '\0') {
        if (ascii_lower((unsigned char)*left) != ascii_lower((unsigned char)*right))
            return 0;
        left++;
        right++;
    }

    return *left == '\0' && *right == '\0';
}

static int find_metadata_id(const FastNoiseApi *api, const char *wanted_name, int *id)
{
    int count = api->fnGetMetadataCount();
    int i;
    for (i = 0; i < count; i++) {
        const char *name = api->fnGetMetadataName(i);
        if (name != NULL && string_equal_ignore_case(name, wanted_name)) {
            *id = i;
            return 0;
        }
    }

    fprintf(stderr, "metadata node not found: %s\n", wanted_name);
    return 3;
}

static int find_variable_index(const FastNoiseApi *api, FnNode node, const char *variable_name, int *index)
{
    int metadata_id = api->fnGetMetadataID(node);
    int count = api->fnGetMetadataVariableCount(metadata_id);
    int i;
    for (i = 0; i < count; i++) {
        const char *name = api->fnGetMetadataVariableName(metadata_id, i);
        if (name != NULL && string_equal_ignore_case(name, variable_name)) {
            *index = i;
            return 0;
        }
    }

    fprintf(stderr, "metadata variable not found: node=%d variable=%s\n", metadata_id, variable_name);
    return 3;
}

static int find_node_lookup_index(const FastNoiseApi *api, FnNode node, const char *lookup_name, int *index)
{
    int metadata_id = api->fnGetMetadataID(node);
    int count = api->fnGetMetadataNodeLookupCount(metadata_id);
    int i;
    for (i = 0; i < count; i++) {
        const char *name = api->fnGetMetadataNodeLookupName(metadata_id, i);
        if (name != NULL && string_equal_ignore_case(name, lookup_name)) {
            *index = i;
            return 0;
        }
    }

    fprintf(stderr, "metadata node lookup not found: node=%d lookup=%s\n", metadata_id, lookup_name);
    return 3;
}

static int find_hybrid_index(const FastNoiseApi *api, FnNode node, const char *hybrid_name, int *index)
{
    int metadata_id = api->fnGetMetadataID(node);
    int count = api->fnGetMetadataHybridCount(metadata_id);
    int i;
    for (i = 0; i < count; i++) {
        const char *name = api->fnGetMetadataHybridName(metadata_id, i);
        if (name != NULL && string_equal_ignore_case(name, hybrid_name)) {
            *index = i;
            return 0;
        }
    }

    fprintf(stderr, "metadata hybrid not found: node=%d hybrid=%s\n", metadata_id, hybrid_name);
    return 3;
}

static int set_variable_float(const FastNoiseApi *api, FnNode node, const char *variable_name, float value)
{
    int index;
    int result = find_variable_index(api, node, variable_name, &index);
    if (result != 0)
        return result;
    if (api->fnSetVariableFloat(node, index, value) == 0) {
        fprintf(stderr, "FastNoise2 rejected float variable '%s'\n", variable_name);
        return 3;
    }

    return 0;
}

static int set_variable_int(const FastNoiseApi *api, FnNode node, const char *variable_name, int value)
{
    int index;
    int result = find_variable_index(api, node, variable_name, &index);
    if (result != 0)
        return result;
    if (api->fnSetVariableIntEnum(node, index, value) == 0) {
        fprintf(stderr, "FastNoise2 rejected int variable '%s'\n", variable_name);
        return 3;
    }

    return 0;
}

static int set_node_lookup(const FastNoiseApi *api, FnNode node, const char *lookup_name, FnNode source)
{
    int index;
    int result = find_node_lookup_index(api, node, lookup_name, &index);
    if (result != 0)
        return result;
    if (api->fnSetNodeLookup(node, index, source) == 0) {
        fprintf(stderr, "FastNoise2 rejected node lookup '%s'\n", lookup_name);
        return 3;
    }

    return 0;
}

static int set_hybrid_float(const FastNoiseApi *api, FnNode node, const char *hybrid_name, float value)
{
    int index;
    int result = find_hybrid_index(api, node, hybrid_name, &index);
    if (result != 0)
        return result;
    if (api->fnSetHybridFloat(node, index, value) == 0) {
        fprintf(stderr, "FastNoise2 rejected hybrid '%s'\n", hybrid_name);
        return 3;
    }

    return 0;
}

static int create_metadata_fbm_simplex(const FastNoiseApi *api, NodePair *pair)
{
    int simplex_id;
    int fractal_id;
    int result;

    memset(pair, 0, sizeof(*pair));
    result = find_metadata_id(api, "Simplex", &simplex_id);
    if (result == 0)
        result = find_metadata_id(api, "FractalFBm", &fractal_id);
    if (result != 0)
        return result;

    pair->owned_source = api->fnNewFromMetadata(simplex_id, UINT32_MAX);
    if (pair->owned_source == NULL) {
        fprintf(stderr, "FastNoise2 failed to create Simplex metadata node\n");
        return 3;
    }

    result = set_variable_float(api, pair->owned_source, "Feature Scale", 100.0f);
    if (result == 0)
        result = set_variable_int(api, pair->owned_source, "Seed Offset", 0);
    if (result == 0)
        result = set_variable_float(api, pair->owned_source, "Output Min", -1.0f);
    if (result == 0)
        result = set_variable_float(api, pair->owned_source, "Output Max", 1.0f);
    if (result != 0)
        return result;

    pair->root = api->fnNewFromMetadata(fractal_id, UINT32_MAX);
    if (pair->root == NULL) {
        fprintf(stderr, "FastNoise2 failed to create FractalFBm metadata node\n");
        return 3;
    }

    result = set_variable_int(api, pair->root, "Octaves", 3);
    if (result == 0)
        result = set_variable_float(api, pair->root, "Lacunarity", 2.0f);
    if (result == 0)
        result = set_hybrid_float(api, pair->root, "Gain", 0.5f);
    if (result == 0)
        result = set_hybrid_float(api, pair->root, "Weighted Strength", 0.0f);
    if (result == 0)
        result = set_node_lookup(api, pair->root, "Source", pair->owned_source);

    return result;
}

static int create_metadata_simplex(const FastNoiseApi *api, NodePair *pair)
{
    int simplex_id;
    int result;

    memset(pair, 0, sizeof(*pair));
    result = find_metadata_id(api, "Simplex", &simplex_id);
    if (result != 0)
        return result;

    pair->root = api->fnNewFromMetadata(simplex_id, UINT32_MAX);
    if (pair->root == NULL) {
        fprintf(stderr, "FastNoise2 failed to create Simplex metadata node\n");
        return 3;
    }

    result = set_variable_float(api, pair->root, "Feature Scale", 100.0f);
    if (result == 0)
        result = set_variable_int(api, pair->root, "Seed Offset", 0);
    if (result == 0)
        result = set_variable_float(api, pair->root, "Output Min", -1.0f);
    if (result == 0)
        result = set_variable_float(api, pair->root, "Output Max", 1.0f);

    return result;
}

static int create_fixture_graph(const FastNoiseApi *api, const Fixture *fixture, NodePair *pair)
{
    memset(pair, 0, sizeof(*pair));
    if (fixture->graph_kind == FixtureGraphEncoded) {
        pair->root = api->fnNewFromEncodedNodeTree(fixture->encoded_tree, UINT32_MAX);
        if (pair->root == NULL) {
            fprintf(stderr, "FastNoise2 failed to load encoded node tree for fixture '%s'\n", fixture->name);
            return 3;
        }

        return 0;
    }

    if (fixture->graph_kind == FixtureGraphMetadataSimplex)
        return create_metadata_simplex(api, pair);

    return create_metadata_fbm_simplex(api, pair);
}

static void destroy_fixture_graph(const FastNoiseApi *api, NodePair *pair)
{
    if (pair->root != NULL)
        api->fnDeleteNodeRef(pair->root);
    if (pair->owned_source != NULL)
        api->fnDeleteNodeRef(pair->owned_source);
    pair->root = NULL;
    pair->owned_source = NULL;
}

static uint64_t fnv1a64(const void *data, size_t size)
{
    const unsigned char *bytes = (const unsigned char *)data;
    uint64_t hash = UINT64_C(14695981039346656037);
    size_t i;
    for (i = 0; i < size; i++) {
        hash ^= (uint64_t)bytes[i];
        hash *= UINT64_C(1099511628211);
    }

    return hash;
}

static void digest_to_hex(uint64_t digest, char text[17])
{
    snprintf(text, 17, "%016" PRIX64, digest);
}

static uint32_t float_bits(float value)
{
    uint32_t bits;
    memcpy(&bits, &value, sizeof(bits));
    return bits;
}

static size_t fixture_value_count(const Fixture *fixture)
{
    return (size_t)fixture->x_count * (size_t)fixture->y_count * (size_t)fixture->z_count;
}

static const char *fixture_graph_kind_name(FixtureGraphKind kind)
{
    return kind == FixtureGraphEncoded ? "encoded" : "metadata";
}

static int run_fixture(const FastNoiseApi *api, const Fixture *fixture, FixtureResult *result)
{
    NodePair pair;
    float *values;
    float min_max[2] = {0.0f, 0.0f};
    size_t value_count = fixture_value_count(fixture);
    size_t byte_count = value_count * sizeof(float);
    uint64_t digest;
    int metadata_id;
    int create_result;

    memset(result, 0, sizeof(*result));
    result->fixture = fixture;
    result->sample_indexes[0] = 0;
    result->sample_indexes[1] = value_count / 2;
    result->sample_indexes[2] = value_count == 0 ? 0 : value_count - 1;

    values = (float *)calloc(value_count, sizeof(float));
    if (values == NULL) {
        fprintf(stderr, "failed to allocate %llu floats for fixture '%s'\n", (unsigned long long)value_count, fixture->name);
        return 5;
    }

    create_result = create_fixture_graph(api, fixture, &pair);
    if (create_result != 0) {
        destroy_fixture_graph(api, &pair);
        free(values);
        return create_result;
    }

    metadata_id = api->fnGetMetadataID(pair.root);
    result->root_metadata_name = api->fnGetMetadataName(metadata_id);
    result->active_feature_set = api->fnGetActiveFeatureSet(pair.root);

    if (fixture->dimensions == FixtureDimension2D) {
        api->fnGenUniformGrid2D(
            pair.root,
            values,
            fixture->x_offset,
            fixture->y_offset,
            fixture->x_count,
            fixture->y_count,
            fixture->x_step,
            fixture->y_step,
            fixture->seed,
            min_max);
    } else {
        api->fnGenUniformGrid3D(
            pair.root,
            values,
            fixture->x_offset,
            fixture->y_offset,
            fixture->z_offset,
            fixture->x_count,
            fixture->y_count,
            fixture->z_count,
            fixture->x_step,
            fixture->y_step,
            fixture->z_step,
            fixture->seed,
            min_max);
    }

    result->min_value = min_max[0];
    result->max_value = min_max[1];
    result->sample_values[0] = values[result->sample_indexes[0]];
    result->sample_values[1] = values[result->sample_indexes[1]];
    result->sample_values[2] = values[result->sample_indexes[2]];
    result->sample_bits[0] = float_bits(result->sample_values[0]);
    result->sample_bits[1] = float_bits(result->sample_values[1]);
    result->sample_bits[2] = float_bits(result->sample_values[2]);

    digest = fnv1a64(values, byte_count);
    digest_to_hex(digest, result->digest);
    result->passed = strcmp(result->digest, fixture->expected_digest) == 0;
    CheckedCases++;

    printf(
        "  fixture=%s graph=%s root=%s featureSet=%" PRIu32 " seed=%d dims=%d count=%dx%dx%d\n",
        fixture->name,
        fixture_graph_kind_name(fixture->graph_kind),
        result->root_metadata_name != NULL ? result->root_metadata_name : "(null)",
        result->active_feature_set,
        fixture->seed,
        (int)fixture->dimensions,
        fixture->x_count,
        fixture->y_count,
        fixture->z_count);
    printf(
        "    offset=(%.9g,%.9g,%.9g) step=(%.9g,%.9g,%.9g) min=%.9g max=%.9g digest=%s expected=%s\n",
        fixture->x_offset,
        fixture->y_offset,
        fixture->z_offset,
        fixture->x_step,
        fixture->y_step,
        fixture->z_step,
        result->min_value,
        result->max_value,
        result->digest,
        fixture->expected_digest);
    printf(
        "    sample[%llu]=%.9g/0x%08" PRIX32 " sample[%llu]=%.9g/0x%08" PRIX32 " sample[%llu]=%.9g/0x%08" PRIX32 "\n",
        (unsigned long long)result->sample_indexes[0],
        result->sample_values[0],
        result->sample_bits[0],
        (unsigned long long)result->sample_indexes[1],
        result->sample_values[1],
        result->sample_bits[1],
        (unsigned long long)result->sample_indexes[2],
        result->sample_values[2],
        result->sample_bits[2]);

    if (!result->passed) {
        fprintf(
            stderr,
            "digest mismatch for fixture '%s': expected=%s actual=%s\n",
            fixture->name,
            fixture->expected_digest,
            result->digest);
    }

    destroy_fixture_graph(api, &pair);
    free(values);
    return 0;
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

static int create_directory_if_missing(const char *path)
{
#if defined(_WIN32)
    if (CreateDirectoryA(path, NULL) || GetLastError() == ERROR_ALREADY_EXISTS)
        return 0;
    fprintf(stderr, "failed to create directory '%s': Windows error %lu\n", path, (unsigned long)GetLastError());
#else
    if (mkdir(path, 0777) == 0 || errno == EEXIST)
        return 0;
    fprintf(stderr, "failed to create directory '%s': %s\n", path, strerror(errno));
#endif
    return 4;
}

static int ensure_parent_directories(const char *path)
{
    char buffer[4096];
    size_t length = strlen(path);
    size_t i;
    if (length >= sizeof(buffer)) {
        fprintf(stderr, "report path is too long: %s\n", path);
        return 4;
    }

    memcpy(buffer, path, length + 1);
    for (i = 0; i < length; i++) {
        if (buffer[i] != '/' && buffer[i] != '\\')
            continue;
        if (i == 0 || (i == 2 && buffer[1] == ':'))
            continue;

        buffer[i] = '\0';
        if (buffer[0] != '\0') {
            int result = create_directory_if_missing(buffer);
            if (result != 0)
                return result;
        }
        buffer[i] = path[i];
    }

    return 0;
}

static void write_sample_json(FILE *file, const FixtureResult *result, int sample_index)
{
    fprintf(
        file,
        "{\"index\":%llu,\"value\":%.9g,\"bits\":\"0x%08" PRIX32 "\"}",
        (unsigned long long)result->sample_indexes[sample_index],
        result->sample_values[sample_index],
        result->sample_bits[sample_index]);
}

static void write_fixture_json(FILE *file, const FixtureResult *result)
{
    const Fixture *fixture = result->fixture;

    fputs("{\"name\":", file);
    json_write_string(file, fixture->name);
    fputs(",\"graphKind\":", file);
    json_write_string(file, fixture_graph_kind_name(fixture->graph_kind));
    fputs(",\"rootMetadata\":", file);
    json_write_string(file, result->root_metadata_name != NULL ? result->root_metadata_name : "");
    fprintf(
        file,
        ",\"activeFeatureSet\":%" PRIu32 ",\"seed\":%d,\"dimensions\":%d",
        result->active_feature_set,
        fixture->seed,
        (int)fixture->dimensions);
    fprintf(
        file,
        ",\"xCount\":%d,\"yCount\":%d,\"zCount\":%d",
        fixture->x_count,
        fixture->y_count,
        fixture->z_count);
    fprintf(
        file,
        ",\"xOffset\":%.9g,\"yOffset\":%.9g,\"zOffset\":%.9g",
        fixture->x_offset,
        fixture->y_offset,
        fixture->z_offset);
    fprintf(
        file,
        ",\"xStep\":%.9g,\"yStep\":%.9g,\"zStep\":%.9g",
        fixture->x_step,
        fixture->y_step,
        fixture->z_step);
    fprintf(file, ",\"min\":%.9g,\"max\":%.9g", result->min_value, result->max_value);
    fputs(",\"samples\":[", file);
    write_sample_json(file, result, 0);
    fputc(',', file);
    write_sample_json(file, result, 1);
    fputc(',', file);
    write_sample_json(file, result, 2);
    fputc(']', file);
    fputs(",\"digest\":", file);
    json_write_string(file, result->digest);
    fputs(",\"expectedDigest\":", file);
    json_write_string(file, fixture->expected_digest);
    fputs(",\"passed\":", file);
    fputs(result->passed ? "true" : "false", file);
    fputc('}', file);
}

static int write_report(
    const char *report_path,
    const char *library_path,
    const char *rid,
    int metadata_count,
    const FixtureResult *results,
    size_t result_count,
    int mismatches)
{
    FILE *file;
    size_t i;
    int directory_result = ensure_parent_directories(report_path);
    if (directory_result != 0)
        return directory_result;

    file = fopen(report_path, "wb");
    if (file == NULL) {
        fprintf(stderr, "failed to open report '%s': %s\n", report_path, strerror(errno));
        return 4;
    }

    fputs("{\"status\":", file);
    json_write_string(file, mismatches == 0 ? "ok" : "failed");
    fputs(",\"library\":", file);
    json_write_string(file, library_path);
    fputs(",\"rid\":", file);
    json_write_string(file, rid);
    fprintf(file, ",\"metadataCount\":%d,\"cases\":%d,\"mismatches\":%d,\"fixtures\":[", metadata_count, CheckedCases, mismatches);

    for (i = 0; i < result_count; i++) {
        if (i != 0)
            fputc(',', file);
        write_fixture_json(file, &results[i]);
    }

    fputs("]}\n", file);
    if (fclose(file) != 0) {
        fprintf(stderr, "failed to write report '%s': %s\n", report_path, strerror(errno));
        return 4;
    }

    return 0;
}

static int run_verification(const FastNoiseApi *api, FixtureResult *results, size_t result_count, int *mismatches)
{
    int metadata_count = api->fnGetMetadataCount();
    size_t i;
    *mismatches = 0;

    printf("  metadataCount=%d\n", metadata_count);
    for (i = 0; i < result_count; i++) {
        int result = run_fixture(api, &Fixtures[i], &results[i]);
        if (result != 0)
            return result;
        if (!results[i].passed)
            (*mismatches)++;
    }

    printf("  verified cases=%d mismatches=%d\n", CheckedCases, *mismatches);
    return 0;
}

int main(int argc, char **argv)
{
    SharedLibrary library;
    FastNoiseApi api;
    FixtureResult results[sizeof(Fixtures) / sizeof(Fixtures[0])];
    int mismatches = 0;
    int metadata_count = 0;
    int result;

    if (argc != 4) {
        fprintf(stderr, "usage: verify-fastnoise2 <library-path> <report-path> <rid>\n");
        return 1;
    }

    printf("fastnoise2 verify: rid=%s\n", argv[3]);
    printf("  library=%s\n", argv[1]);
    library.handle = NULL;
    result = load_library(argv[1], &library);
    if (result != 0)
        return result;
    printf("  loaded native library\n");

    result = load_api(&library, &api);
    if (result == 0) {
        printf("  resolved required symbols\n");
        result = run_verification(&api, results, sizeof(results) / sizeof(results[0]), &mismatches);
        metadata_count = api.fnGetMetadataCount();
    }
    if (result == 0) {
        result = write_report(
            argv[2],
            argv[1],
            argv[3],
            metadata_count,
            results,
            sizeof(results) / sizeof(results[0]),
            mismatches);
        if (result == 0)
            printf("  report=%s\n", argv[2]);
    }

    close_library(&library);
    if (result != 0)
        return result;
    if (mismatches != 0) {
        printf("fastnoise2 verify: FAIL cases=%d mismatches=%d\n", CheckedCases, mismatches);
        return 3;
    }

    printf("fastnoise2 verify: PASS cases=%d\n", CheckedCases);
    return 0;
}
