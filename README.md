# Circuit Simulator Project

A WPF-based circuit simulation application that allows users to design and analyze electrical circuits with real-time visualization and multiple analysis modes.

## Project Overview

This project implements a circuit simulator with a graphical user interface for circuit design and analysis. It combines C++ for the core simulation engine with C# WPF for the user interface, providing a seamless integration between high-performance computation and intuitive user interaction.

### Key Features

- Interactive circuit design with drag-and-drop components
- Support for multiple component types:
  - Resistors (R)
  - Capacitors (C)
  - Inductors (L)
  - Diodes (D)
  - DC Voltage Sources (V)
  - AC Voltage Sources (ACV)
  - Current Sources (I)
- Multiple analysis modes:
  - DC Operating Point Analysis
  - Transient Analysis
  - AC Sweep Analysis
  - Phase Sweep Analysis
- Real-time circuit visualization
- Netlist generation and verification
- Component probing and measurement

## Architecture

### Core Components

1. **Circuit Simulation Engine (C++)**
   - Located in: `src/` and `include/` directories
   - Key files:
     - `CircuitSimulatorInterface.h/cpp`: C++/C# interop layer
     - `Circuit.h/cpp`: Core circuit model implementation
     - `Component.h/cpp`: Base component class
     - Various component implementations (Resistor.h/cpp, etc.)
     - `Analysis.h/cpp`: Different analysis methods
     - `LinearSolver.h/cpp`: Matrix operations for circuit analysis

2. **User Interface (C# WPF)**
   - Located in: `circuitUI/` directory
   - Key files:
     - `MainWindow.xaml/cs`: Main application window
     - `CircuitSimulatorService.cs`: C# wrapper for C++ DLL
     - Component controls (ComponentControl.xaml/cs, etc.)
     - Various UI windows for settings and results

### Interop Layer

The project uses P/Invoke for C++/C# interoperability:
- DLL exports defined in `CircuitSimulatorInterface.h`
- Implementation in `CircuitSimulatorInterface.cpp`
- C# wrapper in `CircuitSimulatorService.cs`

## Known Issues and Solutions

1. **Memory Management**
   - Issue: Potential memory leaks in C++ layer
   - Solution: Implemented proper RAII patterns and smart pointers

2. **Component Connection Handling**
   - Issue: Occasional connection issues between components
   - Solution: Improved node detection and wire routing algorithms

3. **Analysis Performance**
   - Issue: Slow performance with large circuits
   - Solution: Optimized matrix operations and component updates

4. **UI Responsiveness**
   - Issue: UI freezing during long computations
   - Solution: Implemented background processing for analysis tasks

## Development Guidelines

### Adding New Components

1. Create C++ component class inheriting from `Component`
2. Implement required virtual methods
3. Add DLL exports in `CircuitSimulatorInterface.h`
4. Create corresponding WPF control
5. Update `NetlistGenerator` to handle the new component

### Adding Analysis Types

1. Implement analysis method in `Analysis.cpp`
2. Add corresponding exports in interface layer
3. Update `SimulationParameters` enum
4. Add UI controls for new analysis settings
5. Implement results visualization

### UI Customization

- Component visuals defined in XAML
- Styling guidelines in resource dictionaries
- Custom controls inherit from base WPF controls

## Building the Project

### Prerequisites

- Visual Studio 2022 or later
- .NET 9.0 SDK
- C++ build tools
- CMake (for C++ portion)

### Build Steps

1. Build C++ library:
   ```powershell
   cmake -B build
   cmake --build build --config Release
   ```

2. Build C# application:
   ```powershell
   dotnet build circuitUI/circuitUI.csproj
   ```

## Example Circuits

The project includes pre-built example circuits:
1. Simple V-R circuit (DC analysis demonstration)
2. AC-R-C circuit (AC analysis demonstration)

Access these through File > Load Example in the application.

## Common Debugging Tips

1. **Component Connection Issues**
   - Check wire endpoints match component connectors exactly
   - Verify ground node connections
   - Use grid snapping for precise placement

2. **Analysis Failures**
   - Verify circuit has a ground node
   - Check for floating nodes
   - Ensure component values are within reasonable ranges

3. **Performance Issues**
   - Limit unnecessary UI updates during analysis
   - Use appropriate timestep settings
   - Consider circuit complexity vs. analysis requirements

## Future Improvements

- [ ] Additional component types (transistors, op-amps)
- [ ] Subcircuit support
- [ ] Enhanced visualization options
- [ ] Circuit validation tools
- [ ] Performance optimizations for large circuits
- [ ] Integration with external tools and formats

## Contributing

1. Fork the repository
2. Create a feature branch
3. Implement changes with appropriate tests
4. Submit a pull request with detailed description

## License

[Specify your license here]

## Contact

[Add contact information here]
