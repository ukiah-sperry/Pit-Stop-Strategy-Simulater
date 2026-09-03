using PitStopSim.Api.Models;
using Xunit;

namespace PitStopSim.Tests;

public class TireCompoundTests
{
    [Fact]
    public void LapTime_OnFreshTire_ReturnsBaseLapTime()
    {
        var soft = new TireCompound { Name = "Soft", BaseLapTime = 90.0, DegradationPerLap = 0.2 };
        Assert.Equal(90.0, soft.LapTime(0));
    }

    [Fact]
    public void LapTime_IncreasesWithTireAge()
    {
        var soft = new TireCompound { Name = "Soft", BaseLapTime = 90.0, DegradationPerLap = 0.5 };
        Assert.True(soft.LapTime(1) > soft.LapTime(0));
        Assert.True(soft.LapTime(5) > soft.LapTime(1));
    }

    [Fact]
    public void LapTime_LinearDegradation_IsCorrect()
    {
        var medium = new TireCompound { Name = "Medium", BaseLapTime = 91.0, DegradationPerLap = 0.3 };
        // Lap 0: 91.0, Lap 3: 91.0 + 0.3*3 = 91.9
        Assert.Equal(91.9, medium.LapTime(3), precision: 6);
    }

    [Fact]
    public void HarderCompound_Degrades_SlowerThanSofter()
    {
        var soft = new TireCompound { Name = "Soft", BaseLapTime = 90.0, DegradationPerLap = 0.5 };
        var hard = new TireCompound { Name = "Hard", BaseLapTime = 92.0, DegradationPerLap = 0.1 };
        // After enough laps the hard tire should be faster due to lower degradation
        Assert.True(hard.LapTime(30) < soft.LapTime(30));
    }
}
