namespace PitStopSim.Api.Models;

public class Strategy
{
    public IReadOnlyList<Stint> Stints { get; init; } = [];
    public double PitStopTimeLoss { get; init; }
}
