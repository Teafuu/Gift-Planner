// AppHost (Program.cs)
var builder = DistributedApplication.CreateBuilder(args);

// EventStoreDB container
var eventstore = builder.AddContainer("eventstore", "eventstore/eventstore")
    .WithHttpEndpoint(port: 2113, targetPort: 2113, name: "http")  // exposes 2113 on localhost:2113
    .WithEndpoint(port: 1113, targetPort: 1113, name: "tcp", scheme: "tcp")
    .WithEnvironment("EVENTSTORE_CLUSTER_SIZE", "1")
    .WithEnvironment("EVENTSTORE_RUN_PROJECTIONS", "All")
    .WithEnvironment("EVENTSTORE_START_STANDARD_PROJECTIONS", "true")
    .WithEnvironment("EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true")
    .WithEnvironment("EVENTSTORE_INSECURE", "true")
    .WithBindMount("eventstore-data-gift", "/var/lib/eventstore")
    .WithBindMount("eventstore-logs", "/var/log/eventstore");

// Blazor Web app (runs on host)
builder.AddProject<Projects.ChristmasGiftCollection_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WaitFor(eventstore)
    // Use localhost because the project is not in the container network
    .WithEnvironment("ConnectionStrings__eventstore", "esdb://localhost:2113?tls=false");

builder.Build().Run();
