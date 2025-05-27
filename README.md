A cross-platform .NET global tool for assembling and disassembling IL code using the platform’s native ILAsm/ILDasm binaries.

## Installation

```bash
dotnet tool install --global dotnet-il
````

## Usage

```bash
dotnet il <command> [<args>]

```

### Commands

* `asm`
  Assemble IL source into a managed assembly.

  ```bash
  dotnet il asm MyType.il -o MyAssembly.dll
  ```

* `dasm`
  Disassemble a managed assembly into IL source.

  ```bash
  dotnet il dasm MyAssembly.dll -o MyType.il
  ```

* `--help`, `-h`
  Show help for the tool or a specific sub-command.

## How It Works

1. Detects your current RID (falls back to a supported portable RID).
2. Looks under `runtimes/<rid>/native/` for the appropriate `ilasm` or `ildasm` binary.
3. Spawns the native process with your arguments and relays its exit code.

## Examples

```bash
# Disassemble
dotnet il dasm bin/Release/net9.0/MyApp.dll

# Re-assemble
dotnet il asm out/MyApp.il -o rebuilt.dll
```

## Troubleshooting

* **Unsupported RID**: Ensure you’re on one of the supported RIDs listed above.
* **Binary not found**: Confirm your package feed contains the RID-specific runtime packs.
