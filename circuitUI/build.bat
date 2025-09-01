@echo off
echo Building Circuit Simulator...

REM Create build directory if it doesn't exist
if not exist build mkdir build

REM Change to build directory
cd build

REM Generate build files with CMake
cmake .. -G "Visual Studio 17 2022"

REM Build the project
cmake --build . --config Release

REM Copy the DLL to the main directory for the C# application
if exist bin\Release\CircuitSimulator.dll (
    copy bin\Release\CircuitSimulator.dll ..\circuitUI\
    echo CircuitSimulator.dll copied to circuitUI directory
) else (
    echo Warning: CircuitSimulator.dll not found
)

echo Build complete!
pause