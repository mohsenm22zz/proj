#pragma once

#ifdef _WIN32
#ifdef CIRCUITSIMULATOR_EXPORTS
        #define CIRCUITSIMULATOR_API __declspec(dllexport)
#else
#define CIRCUITSIMULATOR_API __declspec(dllimport)
#endif

#else
    #define CIRCUITSIMULATOR_API

#endif
