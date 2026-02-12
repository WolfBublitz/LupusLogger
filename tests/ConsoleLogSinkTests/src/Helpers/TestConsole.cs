using System;
using System.IO;

namespace ConsoleLogSinkTests;

public sealed class TestConsole : IDisposable
{
    private readonly TextWriter originalOut;
    private readonly TextWriter originalError;
    private readonly StringWriter writer;
    private bool disposed;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "TUnit0055:Do not overwrite the Console writer", Justification = "This class is specifically designed to capture console output for testing purposes.")]
    public TestConsole()
    {
        originalOut = Console.Out;
        originalError = Console.Error;
        writer = new StringWriter();

        Console.SetOut(writer);
        Console.SetError(writer);
    }

    public string Output
    {
        get
        {
            writer.Flush();

            return writer.ToString();
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "TUnit0055:Do not overwrite the Console writer", Justification = "This class is specifically designed to capture console output for testing purposes.")]
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Console.SetOut(originalOut);
        Console.SetError(originalError);
        writer.Dispose();
        disposed = true;
    }
}
