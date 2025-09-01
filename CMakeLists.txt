cmake_minimum_required(VERSION 3.10)

# Project name and version
project(CircuitSimulator VERSION 1.0)

# Specify C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Add DLL export definition
add_definitions(-DCIRCUITSIMULATOR_EXPORTS)

# Set the output directory for the DLL
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

# Include directories
include_directories(include)

# Copy to C# project directory after build

# Copy DLL to both Debug and Release C# output directories after build
add_custom_command(TARGET CircuitSimulator POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:CircuitSimulator> ${CMAKE_SOURCE_DIR}/circuitUI/bin/Debug/net9.0-windows/CircuitSimulator.dll
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:CircuitSimulator> ${CMAKE_SOURCE_DIR}/circuitUI/bin/Release/net9.0-windows/CircuitSimulator.dll
)

# Add include directories
include_directories(include)

# Source files
set(SOURCES
    src/ACVoltageSource.cpp
    src/Analysis.cpp
    src/Capacitor.cpp
    src/Circuit.cpp
    src/CircuitIO.cpp
    src/CircuitSimulatorInterface.cpp
    src/Component.cpp
    src/CurrentSource.cpp
    src/Diode.cpp
    src/Inductor.cpp
    src/LinearSolver.cpp
    src/Node.cpp
    src/Resistor.cpp
    src/VoltageSource.cpp
)

# Header files for IDEs
set(HEADERS
    include/ACVoltageSource.h
    include/Analysis.h
    include/Capacitor.h
    include/Circuit.h
    include/CircuitIO.h
    include/CircuitSimulatorInterface.h
    include/Component.h
    include/CurrentSource.h
    include/Diode.h
    include/Inductor.h
    include/LinearSolver.h
    include/Node.h
    include/Resistor.h
    include/VoltageSource.h
    include/export.h
)

# Create shared library (DLL on Windows)
add_library(CircuitSimulator SHARED ${SOURCES} ${HEADERS})

# Compiler-specific options
if(MSVC)
    # Windows-specific definitions
    target_compile_definitions(CircuitSimulator PRIVATE CIRCUITSIMULATOR_EXPORTS)
endif()

# Link math library if needed (for some math functions)
if(UNIX AND NOT APPLE)
    target_link_libraries(CircuitSimulator m)
endif()

# Create executable for standalone testing
add_executable(CircuitSimulatorTest src/main.cpp ${SOURCES} ${HEADERS})

# Link math library for the test executable if needed
if(UNIX AND NOT APPLE)
    target_link_libraries(CircuitSimulatorTest m)
endif()