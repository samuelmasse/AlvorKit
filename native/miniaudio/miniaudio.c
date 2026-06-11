#define MA_DLL
#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"

/* ma_engine/ma_waveform/ma_sound are caller-allocated and their sizes vary by
   platform and miniaudio version, so managed callers query them at runtime. */
MA_API size_t alvorkit_sizeof_ma_engine(void)   { return sizeof(ma_engine); }
MA_API size_t alvorkit_sizeof_ma_waveform(void) { return sizeof(ma_waveform); }
MA_API size_t alvorkit_sizeof_ma_sound(void)    { return sizeof(ma_sound); }
