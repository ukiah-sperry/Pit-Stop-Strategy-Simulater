using PitStopSim.Api.Models;
using PitStopSim.Api.Services;
using Xunit;

namespace PitStopSim.Tests;

public class SimulationServiceTests
{
    private readonly SimulationService _svc = new();

    private static TireCompound Soft() => new() { Name = "Soft", BaseLapTime = 90.0, DegradationPerLap = 0.5 };
    private static TireCompound Medium() => new() { Name = "Medium", BaseLapTime = 91.0, DegradationPerLap = 0.2 };
    private static TireCompound Hard() => new() { Name = "Hard", BaseLapTime = 92.5, DegradationPerLap = 0.05 };

    [Fact]
    public void Simulate_ZeroStops_SingleStint_CorrectTotal()
    {
        // 5-lap race on Medium with 0.2s/lap degradation, no stops
        // Laps: 91.0, 91.2, 91.4, 91.6, 91.8 = 457.0
        var strategy = new Strategy
        {
            Stints = [new Stint { Compound = Medium(), Laps = 5 }],
            PitStopTimeLoss = 25.0
        };

        var result = _svc.Simulate(strategy);

        Assert.Equal(0, result.PitStops);
        Assert.Equal(0.0, result.PitStopTimeLoss);
        Assert.Equal(457.0, result.TotalTime, precision: 6);
        Assert.Equal(5, result.LapByLap.Count);
    }

    [Fact]
    public void Simulate_OneStop_AddsPitTimePenalty()
    {
        // Stint 1: 3 laps on Soft (90.0, 90.5, 91.0 = 271.5)
        // Stint 2: 2 laps on Medium (91.0, 91.2 = 182.2)
        // Pit penalty: 25.0
        // Total: 271.5 + 182.2 + 25.0 = 478.7
        var strategy = new Strategy
        {
            Stints =
            [
                new Stint { Compound = Soft(), Laps = 3 },
                new Stint { Compound = Medium(), Laps = 2 }
            ],
            PitStopTimeLoss = 25.0
        };

        var result = _svc.Simulate(strategy);

        Assert.Equal(1, result.PitStops);
        Assert.Equal(25.0, result.PitStopTimeLoss);
        Assert.Equal(478.7, result.TotalTime, precision: 6);
        Assert.Equal(5, result.LapByLap.Count);
    }

    [Fact]
    public void Simulate_LapByLap_LapOnTireResetsAfterPit()
    {
        var strategy = new Strategy
        {
            Stints =
            [
                new Stint { Compound = Soft(), Laps = 2 },
                new Stint { Compound = Hard(), Laps = 2 }
            ],
            PitStopTimeLoss = 20.0
        };

        var result = _svc.Simulate(strategy);

        // First stint: lapOnTire 0, 1
        Assert.Equal(0, result.LapByLap[0].LapOnTire);
        Assert.Equal(1, result.LapByLap[1].LapOnTire);
        // After pit: lapOnTire resets to 0
        Assert.Equal(0, result.LapByLap[2].LapOnTire);
        Assert.Equal(1, result.LapByLap[3].LapOnTire);
    }

    /// <summary>
    /// Strategic relationship test: under high tire degradation, a 2-stop strategy
    /// should produce a lower total time than 1-stop, because fresh tires outweigh
    /// the extra pit penalty.
    /// </summary>
    [Fact]
    public void Simulate_HighDegradation_TwoStopFasterThanOneStop()
    {
        // Use a compound with very high degradation (2.0s/lap) so tire wear dominates
        var graining = new TireCompound { Name = "Graining", BaseLapTime = 88.0, DegradationPerLap = 2.0 };
        const double pitPenalty = 25.0;
        const int raceLaps = 30;

        // 1-stop: split 15/15
        var oneStop = new Strategy
        {
            Stints =
            [
                new Stint { Compound = graining, Laps = 15 },
                new Stint { Compound = graining, Laps = 15 }
            ],
            PitStopTimeLoss = pitPenalty
        };

        // 2-stop: split 10/10/10
        var twoStop = new Strategy
        {
            Stints =
            [
                new Stint { Compound = graining, Laps = 10 },
                new Stint { Compound = graining, Laps = 10 },
                new Stint { Compound = graining, Laps = 10 }
            ],
            PitStopTimeLoss = pitPenalty
        };

        var oneStopTime = _svc.Simulate(oneStop).TotalTime;
        var twoStopTime = _svc.Simulate(twoStop).TotalTime;

        Assert.True(twoStopTime < oneStopTime,
            $"Expected 2-stop ({twoStopTime:F2}s) to be faster than 1-stop ({oneStopTime:F2}s) under high degradation.");
    }
}
