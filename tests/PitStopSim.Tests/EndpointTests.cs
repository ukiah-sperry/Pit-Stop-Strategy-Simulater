using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PitStopSim.Api.Models;
using Xunit;

namespace PitStopSim.Tests;

public class EndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // --- /simulate ---

    [Fact]
    public async Task Simulate_ValidStrategy_Returns200WithTotal()
    {
        var payload = new SimulateRequest(
            Stints:
            [
                new StintDto(new TireCompoundDto("Medium", 91.0, 0.2), 10),
                new StintDto(new TireCompoundDto("Hard", 92.5, 0.05), 10)
            ],
            PitStopTimeLoss: 25.0,
            RaceLaps: 20);

        var response = await _client.PostAsJsonAsync("/simulate", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<SimulateResponse>();
        Assert.NotNull(result);
        Assert.Equal(20, result!.LapByLap.Count);
        Assert.Equal(1, result.PitStops);
        Assert.True(result.TotalTime > 0);
    }

    [Fact]
    public async Task Simulate_ZeroLapStint_Returns400()
    {
        var payload = new SimulateRequest(
            Stints: [new StintDto(new TireCompoundDto("Soft", 90.0, 0.5), 0)],
            PitStopTimeLoss: 25.0,
            RaceLaps: 0);

        var response = await _client.PostAsJsonAsync("/simulate", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Simulate_StintsDontSumToRaceLaps_Returns400()
    {
        var payload = new SimulateRequest(
            Stints: [new StintDto(new TireCompoundDto("Soft", 90.0, 0.5), 10)],
            PitStopTimeLoss: 25.0,
            RaceLaps: 20);

        var response = await _client.PostAsJsonAsync("/simulate", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- /optimize ---

    [Fact]
    public async Task Optimize_ValidRequest_Returns200WithBestStrategy()
    {
        var payload = new OptimizeRequest(
            RaceLaps: 20,
            AvailableCompounds:
            [
                new TireCompoundDto("Soft", 90.0, 0.5),
                new TireCompoundDto("Medium", 91.0, 0.2),
                new TireCompoundDto("Hard", 92.5, 0.05)
            ],
            NumberOfStops: 1,
            PitStopTimeLoss: 25.0);

        var response = await _client.PostAsJsonAsync("/optimize", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<OptimizeResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result!.Stints.Count);
        Assert.True(result.TotalTime > 0);
    }

    [Fact]
    public async Task Optimize_ZeroStops_ReturnsOneStintStrategy()
    {
        var payload = new OptimizeRequest(
            RaceLaps: 10,
            AvailableCompounds: [new TireCompoundDto("Hard", 92.5, 0.05)],
            NumberOfStops: 0,
            PitStopTimeLoss: 25.0);

        var response = await _client.PostAsJsonAsync("/optimize", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<OptimizeResponse>();
        Assert.NotNull(result);
        Assert.Single(result!.Stints);
    }

    [Fact]
    public async Task Optimize_NoCompounds_Returns400()
    {
        var payload = new OptimizeRequest(
            RaceLaps: 20,
            AvailableCompounds: [],
            NumberOfStops: 1,
            PitStopTimeLoss: 25.0);

        var response = await _client.PostAsJsonAsync("/optimize", payload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
