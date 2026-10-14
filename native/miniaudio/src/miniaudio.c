#define MA_DLL
#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"
#include "extras/decoders/libvorbis/miniaudio_libvorbis.c"
#include "miniaudio_vorbis.h"
#include <stddef.h>
#include "miniaudio_abi.inc"

/* Layout probes are used by the managed release gate on each supported runtime. */
MA_API size_t alvorkit_miniaudio_abi_count(void) {
  return sizeof(alvorkit_abi_values) / sizeof(alvorkit_abi_values[0]);
}

MA_API size_t alvorkit_miniaudio_abi_value(size_t index) {
  return index < alvorkit_miniaudio_abi_count() ? alvorkit_abi_values[index] : (size_t)-1;
}

static ma_decoding_backend_vtable *vorbis_backends[] = {
    &ma_gDecodingBackendVTable_libvorbis};

MA_API ma_resource_manager_config ma_resource_manager_config_init_vorbis(void) {
  ma_resource_manager_config config = ma_resource_manager_config_init();
  config.ppCustomDecodingBackendVTables = vorbis_backends;
  config.customDecodingBackendCount = 1;
  return config;
}

MA_API ma_decoder_config ma_decoder_config_init_vorbis(ma_format format,
                                                       ma_uint32 channels,
                                                       ma_uint32 sampleRate) {
  ma_decoder_config config =
      ma_decoder_config_init(format, channels, sampleRate);
  config.ppCustomBackendVTables = vorbis_backends;
  config.customBackendCount = 1;
  return config;
}
