using PitStopSim.Api.Models;
using PitStopSim.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SimulationService>();

var app = builder.Build();

app.MapPost("/simulate", (SimulateRequest req, SimulationService svc) =>
{
    if (req.Stints.Count == 0)
        return Results.BadRequest("Strategy must contain at least one stint.");

    if (req.Stints.Any(s => s.Laps <= 0))
        return Results.BadRequest("Each stint must have at least one lap.");

    int totalLaps = req.Stints.Sum(s => s.Laps);
    if (totalLaps != req.RaceLaps)
        return Results.BadRequest($"Stint laps ({totalLaps}) must sum to race laps ({req.RaceLaps}).");

    var strategy = new Strategy
    {
        Stints = req.Stints.Select(s => new Stint
        {
            Compound = new TireCompound
            {
                Name = s.Compound.Name,
                BaseLapTime = s.Compound.BaseLapTime,
                DegradationPerLap = s.Compound.DegradationPerLap
            },
            Laps = s.Laps
        }).ToList(),
        PitStopTimeLoss = req.PitStopTimeLoss
    };

    return Results.Ok(svc.Simulate(strategy));
});

app.MapPost("/optimize", (OptimizeRequest req, SimulationService svc) =>
{
    if (req.RaceLaps <= 0)
        return Results.BadRequest("Race laps must be positive.");

    if (req.NumberOfStops < 0)
        return Results.BadRequest("Number of stops cannot be negative.");

    if (req.AvailableCompounds.Count == 0)
        return Results.BadRequest("At least one compound must be provided.");

    var compounds = req.AvailableCompounds.Select(c => new TireCompound
    {
        Name = c.Name,
        BaseLapTime = c.BaseLapTime,
        DegradationPerLap = c.DegradationPerLap
    }).ToList();

    return Results.Ok(svc.Optimize(req.RaceLaps, compounds, req.NumberOfStops, req.PitStopTimeLoss));
});

app.Run();

// Exposed for WebApplicationFactory in integration tests
public partial class Program { }
