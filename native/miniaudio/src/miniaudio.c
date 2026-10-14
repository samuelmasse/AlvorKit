#define MA_DLL
#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"
#include "extras/decoders/libvorbis/miniaudio_libvorbis.c"
#include "miniaudio_vorbis.h"

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
