using System.Text.Json.Serialization;
using C6_Sample_API;
using Microsoft.AspNetCore.Http.HttpResults;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

app.MapOpenApi();

List<Todo> sampleTodos =
[
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2))),
];

int TotalNumberOfRequests = 0;

var todosApi = app.MapGroup("/todos");
todosApi
    .MapGet(
        "/",
        () =>
        {
            app.Logger.LogInformation(
                "Fetching all todos - Total Requests: {TotalRequests}",
                Interlocked.Increment(ref TotalNumberOfRequests)
            );
            return sampleTodos;
        }
    )
    .WithName("GetTodos");

todosApi
    .MapGet(
        "/{id}",
        Results<Ok<Todo>, NotFound> (int id) =>
        {
            app.Logger.LogInformation(
                "Fetching todo with ID {TodoId} - Total Requests: {TotalRequests}",
                id,
                Interlocked.Increment(ref TotalNumberOfRequests)
            );
            return sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                ? TypedResults.Ok(todo)
                : TypedResults.NotFound();
        }
    )
    .WithName("GetTodoById");

await app.RunAsync();

namespace C6_Sample_API
{
    public sealed record Todo(
        int Id,
        string? Title,
        DateOnly? DueBy = null,
        bool IsComplete = false
    );

    [JsonSerializable(typeof(List<Todo>))]
    internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
}
