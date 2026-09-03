namespace PitStopSim.Api.Models;

/// <summary>
/// Simplified tire model. Degradation is linear: each lap on a set adds
/// DegradationPerLap seconds to the base lap time. This is intentionally
/// illustrative — not sourced from real telemetry.
/// </summary>
public class TireCompound
{
    public string Name { get; init; } = string.Empty;
    public double BaseLapTime { get; init; }
    public double DegradationPerLap { get; init; }

    public double LapTime(int lapOnTire) => BaseLapTime + DegradationPerLap * lapOnTire;
}
