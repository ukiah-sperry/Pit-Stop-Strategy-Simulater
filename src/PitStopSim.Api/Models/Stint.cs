namespace PitStopSim.Api.Models;

public class Stint
{
    public TireCompound Compound { get; init; } = null!;
    public int Laps { get; init; }
}
