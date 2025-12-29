# GEMINI.md - AI Context for Azos

This file provides project-specific context and instructions for AI assistants working on the Azos codebase.

## Project Overview
Azos is a "Full-stack Application Chassis" (Application Framework) for .NET. It prioritizes a unified architecture over fragmentation,
 providing its own implementations for logging, configuration, serialization, security, and more.

**Core Philosophy:**
- **Avoid Fragmentation:** Use Azos intrinsic services instead of third-party libraries (e.g., don't use Serilog; use `Azos.Log`).
- **Performance:** High-performance code using intrinsics and optimized memory patterns (e.g., `Azos.Pile`).
- **Unification:** The same chassis hosts CLI, Web, and Cloud (Sky) applications.

## Strict Naming Conventions
Azos follows its own standard. **Do not use standard .NET/FxCop guidelines.**

### Fields and Variables
- **Private Instance Fields:** Must start with `m_` (e.g., `m_Data`).
- **Thread Static Fields:** Must start with `ts_` (e.g., `ts_Cache`).
- **Async Local Fields:** Must start with `ats_` (e.g., `ats_Context`).
- **Constants:** `UPPER_CASE` with unit suffixes (e.g., `TIMEOUT_MSEC`, `RETRY_COUNT`).

### Methods and Members
- **Public/Protected:** PascalCase.
- **Private:** camelCase.
- **Template Method Pattern:** Virtual implementation methods must have the `Do` prefix (e.g., `Connect()` calls `DoConnect()`).

### File Structure
- **One Type Per File:** For types > 100 LOC.
- **Standard Order:**
  1. COPY header (License)
  2. Usings
  3. Inner Types
  4. Static Members
  5. Constructors / Destructors (`Destroy()`)
  6. Private Fields (placed near constructors)
  7. Properties
  8. Public Methods
  9. Protected Methods
  10. `.pvt` implementation (at the very end)

## Key Directories
- `src/Azos`: Core framework library.
- `src/Azos.Sky`: Distributed/Cloud "Sky" operating system.
- `src/Azos.Wave`: Web server/pipeline.
- `elm/build`: Build and package scripts.

## Guidance for AI
- Always check `src/philosophy.md` and `src/naming-conventions.md` before refactoring.
- Favor `Aver.IsNotNull()` or `Guard.NotNull()` over standard null checks if applicable.
- Use `App.TimeSource.UTCNow` instead of `DateTime.UtcNow`.
- Adhere to the `m_` prefix for private fields without exception.
