# Build and Run Instructions for Circuit Simulator

## 1. Build the C++ DLL

From the project root, run:

```
mkdir -Force build
cd build
cmake ..
cmake --build . --config Debug
```

This will build the C++ DLL and automatically copy it to the correct C# output directories (Debug and Release).

## 2. Build and Run the C# WPF Application

From the project root, run:

```
dotnet run --project circuitUI/circuitUI.csproj
```

This ensures the correct project is built and run, and the DLL is found by the application.

---

**Note:**
- If you build in Release mode, use `--config Release` for both C++ and C# builds.
- The DLL must be present in `circuitUI/bin/Debug/net9.0-windows/` (or `Release/net9.0-windows/`).
- If you encounter DLL not found errors, rebuild the C++ project and ensure the DLL is copied as described above.
