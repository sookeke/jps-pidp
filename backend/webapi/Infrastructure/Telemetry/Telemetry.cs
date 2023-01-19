namespace Pidp.Infrastructure.Telemetry;

using System.Diagnostics;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);

}
