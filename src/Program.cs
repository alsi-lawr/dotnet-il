using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Extensions.DependencyModel;

IReadOnlyList<string> validRids =
[
    "win-x64",
    "win-arm64",
    "win-x86",
    "osx-x64",
    "osx-arm64",
    "linux-x64",
    "linux-arm64",
    "linux-musl-x64",
    "linux-musl-arm64"
];

IEnumerable<string?> rids =
[
    .. DependencyContext
        .Default.RuntimeGraph.SelectMany(g => g.Fallbacks.ToList())
        .Select(g => g ?? string.Empty),
    .. DependencyContext.Default.RuntimeGraph.Select(g => g.Runtime)
];

var rid = rids.FirstOrDefault(r => validRids.Contains(r));
if (rid is null)
{
    Console.Error.WriteLine($"Unsupported RID: {RuntimeInformation.RuntimeIdentifier}");
    return 1;
}

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
    "asm" => RunIlTool(IlTool.ilasm, args[1..], rid),
    "dasm" => RunIlTool(IlTool.ildasm, args[1..], rid),
    _ => 1,
};

static int RunIlTool(IlTool tool, string[] args, string rid)
{
    var toolPath = Path.Combine(
        AppContext.BaseDirectory,
        "runtimes",
        rid,
        "native",
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
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    using var proc = Process.Start(psi);
    proc.OutputDataReceived += (_, e) =>
    {
        if (e.Data != null)
            Console.WriteLine(e.Data);
    };
    proc.ErrorDataReceived += (_, e) =>
    {
        if (e.Data != null)
            Console.Error.WriteLine(e.Data);
    };
    proc.BeginOutputReadLine();
    proc.BeginErrorReadLine();
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
