# Resistor Network Solver

Reduce nested series and parallel resistor groups and compute each branch's ideal voltage and current.

![Sample run from Resistor Network Solver](docs/demo.webp)

## Run

Install .NET 8 SDK and runtime. Run these commands from the repository root.

```sh
dotnet build --nologo
dotnet bin/Debug/net8.0/App.dll sample.json
```

Replace `sample.json` with a JSON file following the same structure. The app prints a JSON report to standard output.

A network is a positive resistance in ohms, or an object with one nonempty series or parallel array of child networks. Supply the applied voltage. The model assumes ideal fixed resistances.

## Verify

Run `python check.py` with Python 3.11 or newer after the build step when applicable. The check verifies the bundled scenario and confirms that an invalid input returns an error.
