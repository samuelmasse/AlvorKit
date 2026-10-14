# MiniAudio package verification

This executable deliberately consumes NuGet packages, including their RID-selected
managed assemblies, rather than generated project references. The MiniAudio release
workflow runs it on every supported runtime before publishing any package.

It exercises a no-device engine, resource-manager worker threads, waveform mixing,
volume, stop, and real Vorbis decoding, streaming, length queries, seeking, and
looping. Hardware audio-device output is outside this deterministic release gate.

`tone.ogg` is an authored test fixture, not a game asset: one second of a 440 Hz
sine wave at amplitude 0.4, 48 kHz, stereo, encoded as Vorbis with libsndfile.
It is distributed under the repository's license.
