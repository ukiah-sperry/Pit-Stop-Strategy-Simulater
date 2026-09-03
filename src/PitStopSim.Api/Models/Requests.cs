namespace PitStopSim.Api.Models;

public record TireCompoundDto(string Name, double BaseLapTime, double DegradationPerLap);
public record StintDto(TireCompoundDto Compound, int Laps);

public record SimulateRequest(IReadOnlyList<StintDto> Stints, double PitStopTimeLoss, int RaceLaps);
public record OptimizeRequest(
    int RaceLaps,
    IReadOnlyList<TireCompoundDto> AvailableCompounds,
    int NumberOfStops,
    double PitStopTimeLoss);
