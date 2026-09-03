# Pit Stop Strategy Simulator

[![CI](https://github.com/ukiahsperry/pit_stop_sim/actions/workflows/ci.yml/badge.svg)](https://github.com/ukiahsperry/pit_stop_sim/actions/workflows/ci.yml)

An ASP.NET Core Web API that simulates and optimizes pit stop strategies for circuit racing. Given tire compounds and a race length, it finds the pit lap(s) that minimize total race time.

## Degradation Model

Tire wear is modeled as **linear degradation**: each lap on a set of tires adds a fixed `DegradationPerLap` penalty (in seconds) to the base lap time.

```
lapTime(lapOnTire) = BaseLapTime + DegradationPerLap × lapOnTire
```

This is a deliberately simplified model — not sourced from real telemetry. It captures the essential trade-off (fresh tires are faster; pit stops cost time) without requiring proprietary data.

## Endpoints

### `POST /simulate`

Compute the total race time for an explicit strategy.

**Request:**
```json
{
  "stints": [
    { "compound": { "name": "Soft", "baseLapTime": 90.0, "degradationPerLap": 0.5 }, "laps": 15 },
    { "compound": { "name": "Hard", "baseLapTime": 92.5, "degradationPerLap": 0.05 }, "laps": 35 }
  ],
  "pitStopTimeLoss": 25.0,
  "raceLaps": 50
}
```

**Response:**
```json
{
  "totalTime": 4912.5,
  "pitStops": 1,
  "pitStopTimeLoss": 25.0,
  "lapByLap": [
    { "lap": 1, "compound": "Soft", "lapOnTire": 0, "lapTime": 90.0 },
    ...
  ]
}
```

### `POST /optimize`

Brute-force search over all feasible pit lap combinations and compound assignments to find the minimum-time strategy for a given number of stops.

**Request:**
```json
{
  "raceLaps": 50,
  "availableCompounds": [
    { "name": "Soft",   "baseLapTime": 90.0, "degradationPerLap": 0.5  },
    { "name": "Medium", "baseLapTime": 91.0, "degradationPerLap": 0.2  },
    { "name": "Hard",   "baseLapTime": 92.5, "degradationPerLap": 0.05 }
  ],
  "numberOfStops": 1,
  "pitStopTimeLoss": 25.0
}
```

**Response:**
```json
{
  "totalTime": 4856.3,
  "stints": [
    { "compound": "Soft",   "startLap": 1,  "endLap": 12 },
    { "compound": "Hard",   "startLap": 13, "endLap": 50 }
  ]
}
```

## Running Locally

```bash
dotnet run --project src/PitStopSim.Api
```

The API starts on `http://localhost:5000`.

## Tests

```bash
dotnet test
```

The suite covers:
- Degradation model correctness (linear growth, relative compound behavior)
- Hand-calculated `simulate` totals
- Strategic relationship: 2-stop beats 1-stop under high degradation
- Edge cases: zero-lap stints rejected, stint laps must sum to race laps
- Integration tests for both endpoints via `WebApplicationFactory`

## Tech Stack

- C# / .NET 10
- ASP.NET Core Minimal API
- xUnit
- GitHub Actions CI
