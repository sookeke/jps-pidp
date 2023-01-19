namespace edt.service.Telemetry;

using System.Diagnostics;
using edt.service.Infrastructure.Telemetry;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new(TelemetryConstants.ServiceName);

}
