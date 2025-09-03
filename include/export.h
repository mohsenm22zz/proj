// #pragma once

// #ifdef _WIN32
// #ifdef CIRCUITSIMULATOR_EXPORTS
//         #define CIRCUITSIMULATOR_API __declspec(dllexport)
// #else
// #define CIRCUITSIMULATOR_API __declspec(dllimport)
// #endif

// #else
//     #define CIRCUITSIMULATOR_API

// #endif

#pragma once
#ifdef _WIN32
  #if defined(CIRCUITSIMULATOR_EXPORTS)
    #define CS_API __declspec(dllexport)
  #else
    #define CS_API __declspec(dllimport)
  #endif
#else
  #define CS_API
#endif

#ifdef __cplusplus
  #define CS_EXTERN extern "C"
#else
  #define CS_EXTERN
#endif