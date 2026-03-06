#!/usr/bin/env dotnet
#:package WB.Logging@1.0.0
#:package Spectre.Console@0.54.0

using System.Diagnostics;
using System.Text;
using Spectre.Console;
using WB.Logging;

await using Logger logger = new("Run Tests");

logger.AttachConsole();

logger.Info("Starting test execution...");

string[] commandLineArgs = Environment.GetCommandLineArgs();

if (commandLineArgs.Length < 2)
{
    logger.Error("No project file specified. Usage: ./run-all-tests.cs -- <base-dir>");
    return 1;
}

DirectoryInfo baseDir = new(commandLineArgs[1]);

logger.Info($"Searching for .csproj files in {baseDir.FullName} and running tests...");

int exitCode = 0;

foreach (FileInfo file in baseDir.GetFiles("*Tests.csproj", SearchOption.AllDirectories))
{
    logger.Info($"Running tests for {file.FullName}");

    StringBuilder outputBuilder = new();

    ProcessStartInfo startInfo = new()
    {
        FileName = "dotnet",
        ArgumentList = { "run", "--project", file.FullName, "-c", "Release" },
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8
    };

    startInfo.Environment["TERM"] = "xterm-256color";
    startInfo.Environment["NO_COLOR"] = "0";
    startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en"; // optional

    Process process = new()
    {
        StartInfo = startInfo
    };

    process.OutputDataReceived += (_, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            outputBuilder.AppendLine(e.Data);
        }
    };
    process.ErrorDataReceived += (_, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            outputBuilder.AppendLine(e.Data);
        }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    await AnsiConsole.Status()
    .StartAsync($"Running tests {file.Name}", async ctx =>
    {
        await process.WaitForExitAsync().ConfigureAwait(false);
    });

    exitCode = process.ExitCode > 0 ? process.ExitCode : exitCode;

    if (exitCode == 0)
    {
        logger.Info($"Tests for {file.Name} completed successfully.");
    }
    else
    {
        logger.Error($"Tests for {file.Name} failed");
        logger.Error("Output:");
        logger.Error(outputBuilder.ToString());
    }
}

if (exitCode != 0)
{
    logger.Error($"dotnet test failed with exit code {exitCode}");
}
else
{
    logger.Info("dotnet test completed successfully.");
}

return exitCode;
