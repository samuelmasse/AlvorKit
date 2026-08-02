cmake_minimum_required(VERSION 3.25)
project(AlvorKitInterceptionProfiler LANGUAGES CXX)

set(ALVORKIT_PROFILER_TARGET AlvorKit.Interception.Profiler.Native)
set(ALVORKIT_PROFILER_RID "" CACHE STRING "Target RID for the interception profiler")

if(WIN32)
    if(NOT ALVORKIT_PROFILER_RID STREQUAL "win-x64")
        message(FATAL_ERROR "Windows profiler builds require ALVORKIT_PROFILER_RID=win-x64.")
    endif()
elseif(CMAKE_SYSTEM_NAME STREQUAL "Linux")
    if(ALVORKIT_PROFILER_RID STREQUAL "linux-x64")
        if(NOT CMAKE_SYSTEM_PROCESSOR MATCHES "^(x86_64|amd64|AMD64)$")
            message(FATAL_ERROR "linux-x64 requires an x86-64 compiler target.")
        endif()
        set(ALVORKIT_PROFILER_ARCH_DEFINITIONS HOST_AMD64 TARGET_AMD64)
    elseif(ALVORKIT_PROFILER_RID STREQUAL "linux-arm64")
        if(NOT CMAKE_SYSTEM_PROCESSOR MATCHES "^(aarch64|arm64|ARM64)$")
            message(FATAL_ERROR "linux-arm64 requires an Arm64 compiler target.")
        endif()
        set(ALVORKIT_PROFILER_ARCH_DEFINITIONS HOST_ARM64 TARGET_ARM64)
    else()
        message(FATAL_ERROR "Linux profiler builds require ALVORKIT_PROFILER_RID=linux-x64 or linux-arm64.")
    endif()
else()
    message(FATAL_ERROR "The interception profiler supports only win-x64, linux-x64, and linux-arm64.")
endif()
if(NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "The interception profiler requires a 64-bit compiler target.")
endif()

set(CORECLR_INCLUDE_DIR "" CACHE PATH "Path to dotnet/runtime src/coreclr/inc")
set(CORECLR_PROFILING_INCLUDE_DIR "" CACHE PATH "Path to dotnet/runtime src/coreclr/pal/prebuilt/inc")
set(CORECLR_PAL_INCLUDE_DIR "" CACHE PATH "Path to dotnet/runtime src/coreclr/pal/inc")
set(CORECLR_PAL_RT_INCLUDE_DIR "" CACHE PATH "Path to dotnet/runtime src/coreclr/pal/inc/rt")
set(CORECLR_MINIPAL_INCLUDE_DIR "" CACHE PATH "Path to dotnet/runtime src/native")
set(ALVORKIT_CORECLR_SOURCE "" CACHE PATH "Path to the pinned dotnet/runtime source tree")
if(ALVORKIT_CORECLR_SOURCE STREQUAL "" AND DEFINED ENV{ALVORKIT_CORECLR_SOURCE})
    set(ALVORKIT_CORECLR_SOURCE "$ENV{ALVORKIT_CORECLR_SOURCE}")
endif()
if(CORECLR_INCLUDE_DIR STREQUAL "" AND NOT ALVORKIT_CORECLR_SOURCE STREQUAL "")
    set(CORECLR_INCLUDE_DIR "${ALVORKIT_CORECLR_SOURCE}/src/coreclr/inc")
endif()
if(CORECLR_PROFILING_INCLUDE_DIR STREQUAL "" AND NOT ALVORKIT_CORECLR_SOURCE STREQUAL "")
    set(CORECLR_PROFILING_INCLUDE_DIR "${ALVORKIT_CORECLR_SOURCE}/src/coreclr/pal/prebuilt/inc")
endif()
if(CORECLR_PAL_INCLUDE_DIR STREQUAL "" AND NOT ALVORKIT_CORECLR_SOURCE STREQUAL "")
    set(CORECLR_PAL_INCLUDE_DIR "${ALVORKIT_CORECLR_SOURCE}/src/coreclr/pal/inc")
endif()
if(CORECLR_PAL_RT_INCLUDE_DIR STREQUAL "" AND NOT ALVORKIT_CORECLR_SOURCE STREQUAL "")
    set(CORECLR_PAL_RT_INCLUDE_DIR "${ALVORKIT_CORECLR_SOURCE}/src/coreclr/pal/inc/rt")
endif()
if(CORECLR_MINIPAL_INCLUDE_DIR STREQUAL "" AND NOT ALVORKIT_CORECLR_SOURCE STREQUAL "")
    set(CORECLR_MINIPAL_INCLUDE_DIR "${ALVORKIT_CORECLR_SOURCE}/src/native")
endif()
if(NOT EXISTS "${CORECLR_INCLUDE_DIR}/cor.h")
    message(FATAL_ERROR "CORECLR_INCLUDE_DIR must contain cor.h for the target CoreCLR runtime.")
endif()
if(NOT EXISTS "${CORECLR_PROFILING_INCLUDE_DIR}/corprof.h")
    message(FATAL_ERROR "CORECLR_PROFILING_INCLUDE_DIR must contain corprof.h for the target CoreCLR runtime.")
endif()
if(NOT WIN32 AND NOT EXISTS "${CORECLR_PAL_RT_INCLUDE_DIR}/unknwn.h")
    message(FATAL_ERROR "CORECLR_PAL_RT_INCLUDE_DIR must contain the CoreCLR PAL COM headers.")
endif()
if(NOT WIN32 AND NOT EXISTS "${CORECLR_MINIPAL_INCLUDE_DIR}/minipal/guid.h")
    message(FATAL_ERROR "CORECLR_MINIPAL_INCLUDE_DIR must contain minipal/guid.h.")
endif()

file(GLOB ALVORKIT_PROFILER_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")

add_library(
    ${ALVORKIT_PROFILER_TARGET}
    SHARED
    ${ALVORKIT_PROFILER_SOURCES})
if(WIN32)
    target_sources(
        ${ALVORKIT_PROFILER_TARGET}
        PRIVATE
        src/exports.def)
endif()

target_compile_features(${ALVORKIT_PROFILER_TARGET} PRIVATE cxx_std_20)
target_compile_definitions(
    ${ALVORKIT_PROFILER_TARGET}
    PRIVATE
    ALVORKIT_INTERCEPTION_PROFILER_BUILD)
if(WIN32)
    target_compile_definitions(
        ${ALVORKIT_PROFILER_TARGET}
        PRIVATE
        UNICODE
        _UNICODE
        NOMINMAX)
endif()
if(MSVC)
    target_compile_options(
        ${ALVORKIT_PROFILER_TARGET}
        PRIVATE
        /EHsc
        /W4
        /permissive-)
elseif(CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
    target_compile_options(
        ${ALVORKIT_PROFILER_TARGET}
        PRIVATE
        /EHsc
        /W4
        /permissive-)
elseif(CMAKE_SYSTEM_NAME STREQUAL "Linux")
    target_compile_definitions(
        ${ALVORKIT_PROFILER_TARGET}
        PRIVATE
        DISABLE_CONTRACTS
        ${ALVORKIT_PROFILER_ARCH_DEFINITIONS}
        HOST_64BIT
        HOST_UNIX
        TARGET_64BIT
        TARGET_LINUX
        TARGET_UNIX
        _FILE_OFFSET_BITS=64
        _TIME_BITS=64)
    target_compile_options(
        ${ALVORKIT_PROFILER_TARGET}
        PRIVATE
        -Wall
        -Wextra
        -Wpedantic)
    set_target_properties(
        ${ALVORKIT_PROFILER_TARGET}
        PROPERTIES
        CXX_VISIBILITY_PRESET hidden
        VISIBILITY_INLINES_HIDDEN YES)
    target_link_options(
        ${ALVORKIT_PROFILER_TARGET}
        PRIVATE
        LINKER:--no-undefined
        "LINKER:--version-script=${CMAKE_CURRENT_SOURCE_DIR}/src/exports.map")
else()
    message(FATAL_ERROR "Unsupported interception-profiler compiler.")
endif()

target_include_directories(
    ${ALVORKIT_PROFILER_TARGET}
    PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}/include")
target_include_directories(
    ${ALVORKIT_PROFILER_TARGET}
    SYSTEM PRIVATE
    "${CORECLR_INCLUDE_DIR}"
    "${CORECLR_PROFILING_INCLUDE_DIR}")

if(WIN32)
    target_link_libraries(${ALVORKIT_PROFILER_TARGET} PRIVATE ole32)
else()
    find_package(Threads REQUIRED)
    target_include_directories(
        ${ALVORKIT_PROFILER_TARGET}
        SYSTEM PRIVATE
        "${CORECLR_PAL_RT_INCLUDE_DIR}"
        "${CORECLR_PAL_INCLUDE_DIR}"
        "${CORECLR_MINIPAL_INCLUDE_DIR}")
    target_link_libraries(${ALVORKIT_PROFILER_TARGET} PRIVATE Threads::Threads)
endif()
