using System.Diagnostics;
using System.Runtime.InteropServices;

ReadOnlySpan<string> helpArgs = ["--help", "-h", "help", "/HELP", "/H"];
if (args.Length == 0 || helpArgs.Contains(args[0]))
{
    PrintHelp();
    return 0;
}

if (args[0].ToLowerInvariant() != "asm" && args[0].ToLowerInvariant() != "dasm")
{
    Console.WriteLine($"Unknown command: {args[0]}");
    PrintHelp();
    return 1;
}

return args[0].ToLowerInvariant() switch
{
    "asm" => RunIlTool(IlTool.ilasm, args[1..]),
    "dasm" => RunIlTool(IlTool.ildasm, args[1..]),
    _ => 1,
};

static int RunIlTool(IlTool tool, string[] args)
{
    var toolPath = Path.Combine(
        AppContext.BaseDirectory,
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{tool}.exe" : $"{tool}"
    );

    if (!File.Exists(toolPath))
    {
        Console.Error.WriteLine($"{tool} not found for RID {RuntimeInformation.RuntimeIdentifier}");
        PrintHelp();
        return 1;
    }
    var psi = new ProcessStartInfo(toolPath, string.Join(" ", args))
    {
        UseShellExecute = false,
        CreateNoWindow = true
    };
    using var proc = Process.Start(psi);
    proc!.WaitForExit();
    return proc.ExitCode;
}

static void PrintHelp()
{
    Console.WriteLine(
        """
        Usage: dotnet il <command> [<args>]

        Commands:
          asm      Assemble IL source into a managed assembly
          dasm     Disassemble a managed assembly into IL source


        Run `dotnet il <command> --help` to see the full list of options for that subcommand.
        """
    );
}

enum IlTool
{
    ilasm,
    ildasm
}
