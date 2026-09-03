using PitStopSim.Api.Models;

namespace PitStopSim.Api.Services;

public class SimulationService
{
    public SimulateResponse Simulate(Strategy strategy)
    {
        var lapDetails = new List<LapDetail>();
        double totalTime = 0;
        int globalLap = 1;

        foreach (var stint in strategy.Stints)
        {
            for (int lapOnTire = 0; lapOnTire < stint.Laps; lapOnTire++)
            {
                double lapTime = stint.Compound.LapTime(lapOnTire);
                lapDetails.Add(new LapDetail(globalLap, stint.Compound.Name, lapOnTire, lapTime));
                totalTime += lapTime;
                globalLap++;
            }
        }

        int pitStops = strategy.Stints.Count - 1;
        double pitTimeLoss = pitStops * strategy.PitStopTimeLoss;
        totalTime += pitTimeLoss;

        return new SimulateResponse(totalTime, pitStops, pitTimeLoss, lapDetails);
    }

    /// <summary>
    /// Brute-force search over all combinations of pit laps for the given number of
    /// stops, trying every permutation of available compounds per stint. Returns the
    /// strategy with the minimum total time.
    /// </summary>
    public OptimizeResponse Optimize(
        int raceLaps,
        IReadOnlyList<TireCompound> compounds,
        int numberOfStops,
        double pitStopTimeLoss)
    {
        int numberOfStints = numberOfStops + 1;
        Strategy? best = null;
        double bestTime = double.MaxValue;

        foreach (var pitLaps in EnumeratePitLapCombinations(raceLaps, numberOfStops))
        {
            var stintLengths = PitLapsToStintLengths(pitLaps, raceLaps);

            foreach (var compoundAssignment in EnumerateCompoundAssignments(compounds, numberOfStints))
            {
                var stints = stintLengths
                    .Zip(compoundAssignment, (laps, compound) => new Stint { Compound = compound, Laps = laps })
                    .ToList();

                var strategy = new Strategy { Stints = stints, PitStopTimeLoss = pitStopTimeLoss };
                var result = Simulate(strategy);

                if (result.TotalTime < bestTime)
                {
                    bestTime = result.TotalTime;
                    best = strategy;
                }
            }
        }

        if (best is null)
            throw new InvalidOperationException("No valid strategy found.");

        var summaries = BuildStintSummaries(best.Stints);
        return new OptimizeResponse(bestTime, summaries);
    }

    private static IReadOnlyList<StintSummary> BuildStintSummaries(IReadOnlyList<Stint> stints)
    {
        var summaries = new List<StintSummary>();
        int lap = 1;
        foreach (var stint in stints)
        {
            summaries.Add(new StintSummary(stint.Compound.Name, lap, lap + stint.Laps - 1));
            lap += stint.Laps;
        }
        return summaries;
    }

    /// <summary>
    /// Yields all combinations of <paramref name="stops"/> pit laps within a race of
    /// <paramref name="raceLaps"/> laps. Each pit lap is the last lap of a stint, so
    /// valid values are 1..(raceLaps-1) with strict ordering. Minimum stint length is
    /// 1 lap so pit laps must be at least <paramref name="stops"/> laps from the end.
    /// </summary>
    private static IEnumerable<int[]> EnumeratePitLapCombinations(int raceLaps, int stops)
    {
        if (stops == 0)
        {
            yield return [];
            yield break;
        }

        foreach (var c in Combinations(1, raceLaps - 1, stops))
            yield return c;
    }

    private static IEnumerable<int[]> Combinations(int min, int max, int count)
    {
        if (count == 0) { yield return []; yield break; }
        for (int i = min; i <= max - count + 1; i++)
        {
            foreach (var rest in Combinations(i + 1, max, count - 1))
            {
                var result = new int[count];
                result[0] = i;
                rest.CopyTo(result, 1);
                yield return result;
            }
        }
    }

    private static int[] PitLapsToStintLengths(int[] pitLaps, int raceLaps)
    {
        var lengths = new int[pitLaps.Length + 1];
        int prev = 0;
        for (int i = 0; i < pitLaps.Length; i++)
        {
            lengths[i] = pitLaps[i] - prev;
            prev = pitLaps[i];
        }
        lengths[^1] = raceLaps - prev;
        return lengths;
    }

    private static IEnumerable<TireCompound[]> EnumerateCompoundAssignments(
        IReadOnlyList<TireCompound> compounds, int stints)
    {
        if (stints == 0) { yield return []; yield break; }
        foreach (var compound in compounds)
        {
            foreach (var rest in EnumerateCompoundAssignments(compounds, stints - 1))
            {
                var result = new TireCompound[stints];
                result[0] = compound;
                rest.CopyTo(result, 1);
                yield return result;
            }
        }
    }
}
