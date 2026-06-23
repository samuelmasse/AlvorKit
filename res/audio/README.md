# Audio Demo Assets

`la-espero-demo.wav`, `la-espero-demo.mp3`, and `la-espero-demo.flac` are
20-second excerpts converted from the Wikimedia Commons file
`La Espero - 1 piano - 2020.ogg`.

Source: <https://commons.wikimedia.org/wiki/File:La_Espero_-_1_piano_-_2020.ogg>

Author: Florian CUNY / Poslovitch

License: Creative Commons CC0 1.0 Universal Public Domain Dedication.

The source page describes the file as the uploader's own work and marks it as
CC0/public domain dedication. The converted demo excerpts keep that license.

Conversion summary:

```powershell
ffmpeg -i out\audio-source\la-espero-source.ogg -t 20 -ar 48000 -ac 2 -c:a pcm_s16le res\audio\la-espero-demo.wav
ffmpeg -i out\audio-source\la-espero-source.ogg -t 20 -ar 48000 -ac 2 -c:a libmp3lame -b:a 160k res\audio\la-espero-demo.mp3
ffmpeg -i out\audio-source\la-espero-source.ogg -t 20 -ar 48000 -ac 2 -sample_fmt s16 -c:a flac res\audio\la-espero-demo.flac
```
