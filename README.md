# PokerLogViewer

WPF desktop application for viewing poker logs stored in JSON files.

## Goal

The application scans a user-selected folder, parses JSON files with poker hands, and displays:
- unique table names
- hands for the selected table
- details for the selected hand

## Repository structure

- `src/` — application source code
- `tests/` — unit tests
- `sample-data/` — example JSON files

## Build plan

- .NET 8
- WPF
- MVVM
- Explicit `Thread`-based background scanning
- CMake-generated Visual Studio solution