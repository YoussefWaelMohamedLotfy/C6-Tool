# C6 Tool

C6 is a high-performance command-line load testing and performance benchmarking tool built with .NET 10. It enables developers and DevOps engineers to simulate concurrent user traffic against HTTP endpoints, measure performance metrics, and analyze system behavior under load.

## Features

- **Virtual User Simulation**: Create and manage concurrent virtual users to simulate realistic load patterns
- **Staged Load Testing**: Define multi-stage test scenarios with ramping load profiles
- **JSON Configuration**: Configure test scenarios via JSON config files for reproducibility
- **Comprehensive Metrics**: Collect and aggregate detailed performance metrics including response times, success/failure rates
- **Graceful Shutdown**: Properly handles test cancellation and shutdown of virtual users
- **Real-time Monitoring**: Display live progress of active virtual users and targets during test execution
- **Console Reporting**: Generate detailed reports of test results with aggregated statistics

## Getting Started

### Prerequisites

- .NET 10 or later

### Usage

Run a test based on a JSON configuration file:

```bash
C6 run-file -f path/to/config.json
```

## Configuration

Tests are defined using JSON configuration files that specify:
- Target endpoint URL
- HTTP method (GET, POST, PUT, DELETE, PATCH)
- Request payloads and headers
- Multi-stage load profile (target virtual users and duration per stage)
- Timeout and sleep parameters

Example configuration structure:
```json
{
  "testName": "My Load Test",
  "targetUrl": "https://api.example.com/endpoint",
  "method": "GET",
  "stages": [
    {
      "target": 10,
      "duration": "00:00:30"
    },
    {
      "target": 50,
      "duration": "00:01:00"
    }
  ]
}
```

## Sample To-do API

The solution includes a sample ASP.NET Core API (`C6-Sample-API`) that can be used for testing. This API provides a simple Todo endpoint suitable for load testing scenarios.

## Architecture

- **C6**: Core load testing engine and CLI
- **C6-Sample-API**: Sample ASP.NET Core API for testing purposes

