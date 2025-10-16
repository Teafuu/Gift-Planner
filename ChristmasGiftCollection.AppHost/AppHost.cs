var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL server with PgAdmin
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

// Add database for gift collection
var giftcollectiondb = postgres.AddDatabase("giftcollection");

// Add Blazor Web application with database reference
builder.AddProject<Projects.ChristmasGiftCollection_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(giftcollectiondb)
    .WaitFor(giftcollectiondb);

builder.Build().Run();
