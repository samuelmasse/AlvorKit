#ifndef ALVORKIT_MINIAUDIO_VORBIS_H
#define ALVORKIT_MINIAUDIO_VORBIS_H

#include "miniaudio.h"

#ifdef __cplusplus
extern "C" {
#endif

/** Initializes a resource manager configuration with the bundled libvorbis decoder.
 * The backend table has native-library lifetime and must not be modified or freed.
 * Supports streaming, length queries, and seeking through MiniAudio's VFS.
 * All other settings retain ma_resource_manager_config_init defaults.
 */
MA_API ma_resource_manager_config ma_resource_manager_config_init_vorbis(void);

/** Initializes a decoder configuration with the bundled libvorbis decoder.
 * The backend table has native-library lifetime and must not be modified or freed.
 * All other settings retain ma_decoder_config_init defaults.
 */
MA_API ma_decoder_config ma_decoder_config_init_vorbis(ma_format format, ma_uint32 channels, ma_uint32 sampleRate);

#ifdef __cplusplus
}
#endif
#endif
