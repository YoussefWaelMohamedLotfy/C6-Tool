using System;
using System.Collections.Generic;
using System.Text;

namespace C6.Commands;

internal sealed class C6Commands
{
    /// <summary>
    /// Run a test based on JSON config path or params
    /// </summary>
    /// <param name="connections">-c, Number of Concurrent Connections</param>
    /// <param name="numberOfRequests">-n, Number of Requests</param>
    /// <param name="url">-u, URL of Server Endpoint</param>
    /// <param name="filePath">-f, Path of File</param>
    /// <param name="ct"></param>
    public void Run(int connections = 0, int numberOfRequests = 0, string url = "", string filePath = "", CancellationToken ct = default)
    {
        Console.WriteLine($"{connections} - {numberOfRequests} - {url} - {(string.IsNullOrEmpty(filePath) ? "No path" : "filepath")}");
    }
}
