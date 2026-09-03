namespace PitStopSim.Api.Models;

public record LapDetail(int Lap, string Compound, int LapOnTire, double LapTime);

public record SimulateResponse(
    double TotalTime,
    int PitStops,
    double PitStopTimeLoss,
    IReadOnlyList<LapDetail> LapByLap);

public record StintSummary(string Compound, int StartLap, int EndLap);

public record OptimizeResponse(
    double TotalTime,
    IReadOnlyList<StintSummary> Stints);
