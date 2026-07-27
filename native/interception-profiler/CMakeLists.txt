cmake_minimum_required(VERSION 3.25)
project(AlvorKitInterceptionProfiler LANGUAGES CXX)

set(ALVORKIT_PROFILER_TARGET AlvorKit.Interception.Profiler.Native)
set(ALVORKIT_PROFILER_RID "" CACHE STRING "Target RID for the interception profiler")

if(NOT WIN32 OR NOT ALVORKIT_PROFILER_RID STREQUAL "win-x64")
    message(FATAL_ERROR "The interception profiler currently supports only win-x64.")
endif()
if(NOT CMAKE_SIZEOF_VOID_P EQUAL 8)
    message(FATAL_ERROR "The win-x64 interception profiler requires a 64-bit compiler target.")
endif()

set(CORECLR_INCLUDE_DIR "" CACHE PATH "Path to dotnet/runtime src/coreclr/inc")
set(CORECLR_PROFILING_INCLUDE_DIR "" CACHE PATH "Path to dotnet/runtime src/coreclr/pal/prebuilt/inc")
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
if(NOT EXISTS "${CORECLR_INCLUDE_DIR}/cor.h")
    message(FATAL_ERROR "CORECLR_INCLUDE_DIR must contain cor.h for the target CoreCLR runtime.")
endif()
if(NOT EXISTS "${CORECLR_PROFILING_INCLUDE_DIR}/corprof.h")
    message(FATAL_ERROR "CORECLR_PROFILING_INCLUDE_DIR must contain corprof.h for the target CoreCLR runtime.")
endif()

file(GLOB ALVORKIT_PROFILER_SOURCES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cpp")

add_library(
    ${ALVORKIT_PROFILER_TARGET}
    SHARED
    ${ALVORKIT_PROFILER_SOURCES})
target_sources(
    ${ALVORKIT_PROFILER_TARGET}
    PRIVATE
    src/exports.def)

target_compile_features(${ALVORKIT_PROFILER_TARGET} PRIVATE cxx_std_20)
target_compile_definitions(
    ${ALVORKIT_PROFILER_TARGET}
    PRIVATE
    UNICODE
    _UNICODE
    NOMINMAX)
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
else()
    message(FATAL_ERROR "The win-x64 interception profiler requires MSVC or clang-cl.")
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

target_link_libraries(${ALVORKIT_PROFILER_TARGET} PRIVATE ole32)
