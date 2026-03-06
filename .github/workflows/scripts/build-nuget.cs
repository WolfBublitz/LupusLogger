#!/usr/bin/env dotnet
#:package WB.Logging@1.0.0

using System.Diagnostics;
using WB.Logging;

await using Logger logger = new("BuildNuget");

logger.AttachConsole();

string[] commandLineArgs = Environment.GetCommandLineArgs();

if (commandLineArgs.Length < 2)
{
    logger.Error("No project file specified. Usage: dotnet run build-nuget.cs -- <project-file>");
    return 1;
}

string projectFile = commandLineArgs[1];

ProcessStartInfo startInfo = new()
{
    FileName = "dotnet",
    ArgumentList = { "pack", projectFile, "-c", "Release" },
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
    CreateNoWindow = true
};

Process process = new()
{
    StartInfo = startInfo
};

process.OutputDataReceived += (_, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
    {
        logger.Info(e.Data);
    }
};
process.ErrorDataReceived += (_, e) =>
{
    if (!string.IsNullOrEmpty(e.Data))
    {
        logger.Error(e.Data);
    }
};

process.Start();
process.BeginOutputReadLine();
process.BeginErrorReadLine();

await process.WaitForExitAsync().ConfigureAwait(false);

if (process.ExitCode != 0)
{
    logger.Error($"dotnet pack failed with exit code {process.ExitCode}");
}
else
{
    logger.Info("dotnet pack completed successfully.");
}

return process.ExitCode;