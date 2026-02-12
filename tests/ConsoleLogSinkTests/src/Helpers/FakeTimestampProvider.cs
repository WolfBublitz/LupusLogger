using System;
using WB.Logging;

namespace ConsoleLogSinkTests;

internal sealed class FakeTimestampProvider : ITimestampProvider
{
    public DateTimeOffset CurrentTimestamp => DateTimeOffset.MinValue;
}
