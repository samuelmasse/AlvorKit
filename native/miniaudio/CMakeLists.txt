cmake_minimum_required(VERSION 3.25)
project(AlvorKitMiniAudio LANGUAGES C)

include(FetchContent)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_C_VISIBILITY_PRESET hidden)
set(CMAKE_POLICY_VERSION_MINIMUM 3.5)
set(BUILD_SHARED_LIBS OFF CACHE BOOL "Build codec dependencies statically" FORCE)
set(BUILD_TESTING OFF CACHE BOOL "Disable upstream codec tests" FORCE)
set(INSTALL_DOCS OFF CACHE BOOL "Disable upstream documentation" FORCE)
set(INSTALL_CMAKE_PACKAGE_MODULE OFF CACHE BOOL "Disable codec package exports" FORCE)

file(STRINGS "${CMAKE_CURRENT_SOURCE_DIR}/version/TAG" MINIAUDIO_TAG LIMIT_COUNT 1)
FetchContent_Declare(miniaudio_source
    URL "https://github.com/mackron/miniaudio/archive/refs/tags/${MINIAUDIO_TAG}.tar.gz"
    URL_HASH SHA256=b900edcffe979816e2560a0580b9b1216d674b4f17fbadeca8f777a7f8ab0274
    SOURCE_SUBDIR alvorkit-no-subproject)
FetchContent_Declare(ogg
    URL "https://downloads.xiph.org/releases/ogg/libogg-1.3.6.tar.xz"
    URL_HASH SHA256=5c8253428e181840cd20d41f3ca16557a9cc04bad4a3d04cce84808677fa1061)
FetchContent_Declare(vorbis
    URL "https://downloads.xiph.org/releases/vorbis/libvorbis-1.3.7.tar.xz"
    URL_HASH SHA256=b33cc4934322bcbf6efcbacf49e3ca01aadbea4114ec9589d1b1e9d20f72954b)
FetchContent_MakeAvailable(miniaudio_source ogg)

# Resolve Vorbis's FindOgg against the pinned source target, never a system codec.
set(OGG_LIBRARY ogg)
set(OGG_INCLUDE_DIR "${ogg_SOURCE_DIR}/include")
FetchContent_MakeAvailable(vorbis)

add_library(miniaudio SHARED src/miniaudio.c)
target_include_directories(miniaudio PRIVATE "${miniaudio_source_SOURCE_DIR}")
target_link_libraries(miniaudio PRIVATE vorbisfile)
if(UNIX)
    find_package(Threads REQUIRED)
    target_link_libraries(miniaudio PRIVATE Threads::Threads ${CMAKE_DL_LIBS} m)
endif()

# Ship the static codecs' required notices with the native package.
file(READ "${ogg_SOURCE_DIR}/COPYING" OGG_LICENSE)
file(READ "${vorbis_SOURCE_DIR}/COPYING" VORBIS_LICENSE)
file(WRITE "${CMAKE_CURRENT_BINARY_DIR}/THIRD-PARTY-NOTICES.txt"
    "libogg 1.3.6\n\n${OGG_LICENSE}\nlibvorbis 1.3.7\n\n${VORBIS_LICENSE}")
