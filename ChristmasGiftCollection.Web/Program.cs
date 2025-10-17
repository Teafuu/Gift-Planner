using Akka.Actor;
using Akka.DependencyInjection;
using Azure.Core;
using Azure.Identity;
using ChristmasGiftCollection.Core.Actors;
using ChristmasGiftCollection.Core.Repositories;
using ChristmasGiftCollection.Web.Components;
using ChristmasGiftCollection.Web.Infrastructure;
using Microsoft.Azure.Cosmos;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Akka", Serilog.Events.LogEventLevel.Information);
});

// Configure direct CosmosClient with Azure AD authentication
var cosmosEndpoint = builder.Configuration.GetConnectionString("cosmosdb")
    ?? "https://cosmosdb-hapmzcec7olfy.documents.azure.com:443/";

builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILogger<Program>>();

    var credentialOptions = new DefaultAzureCredentialOptions
    {
        ExcludeVisualStudioCredential = true,
        ExcludeVisualStudioCodeCredential = true,
        ExcludeAzureCliCredential = false, // Keep this for local development
        ExcludeManagedIdentityCredential = false, // This is needed for Azure
        Diagnostics = { IsLoggingEnabled = true }
    };

    var credential = new DefaultAzureCredential(credentialOptions);

    var options = new CosmosClientOptions
    {
        Serializer = new CosmosSystemTextJsonSerializer(
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            })
    };

    try
    {
        logger.LogInformation("Attempting to connect to Cosmos DB at {Endpoint}", cosmosEndpoint);
        var client = new CosmosClient(cosmosEndpoint, credential, options);
        logger.LogInformation("Successfully created Cosmos DB client");
        return client;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to create Cosmos DB client");
        throw;
    }
});

// Register Cosmos DB repository (initialization will happen lazily)
builder.Services.AddSingleton<IEventStoreRepository, CosmosDbEventStoreRepository>();

// Add authentication services (Scoped for per-circuit authentication - each user gets their own instance)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ChristmasGiftCollection.Web.Services.IAuthenticationService, ChristmasGiftCollection.Web.Services.AuthenticationService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor services
builder.Services.AddMudServices();

// Configure Akka.NET Actor System
builder.Services.AddSingleton(sp =>
{
    var bootstrap = BootstrapSetup.Create();
    var di = DependencyResolverSetup.Create(sp);
    var actorSystemSetup = bootstrap.And(di);

    var actorSystem = ActorSystem.Create("ChristmasGiftCollection", actorSystemSetup);

    // Create the MemberActorSupervisor
    var eventStore = sp.GetRequiredService<IEventStoreRepository>();
    var memberSupervisor = actorSystem.ActorOf(MemberActorSupervisor.Props(eventStore), "member-supervisor");

    // Create the SecretSantaSupervisor
    var secretSantaSupervisor = actorSystem.ActorOf(SecretSantaSupervisor.Props(eventStore), "secretsanta-supervisor");

    return actorSystem;
});

// Register the MemberActorSupervisor reference
builder.Services.AddSingleton(sp =>
{
    var actorSystem = sp.GetRequiredService<ActorSystem>();
    return actorSystem.ActorSelection("/user/member-supervisor");
});

// Register services - using actor-based service for event sourcing
builder.Services.AddScoped<ChristmasGiftCollection.Core.Services.IMemberService, ChristmasGiftCollection.Core.Services.ActorMemberService>();
builder.Services.AddScoped<ChristmasGiftCollection.Core.Services.ISecretSantaService, ChristmasGiftCollection.Core.Services.ActorSecretSantaService>();

builder.Services.AddOutputCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Ensure Akka.NET actor system shuts down gracefully
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    var actorSystem = app.Services.GetRequiredService<ActorSystem>();
    actorSystem.Terminate().Wait();
});

app.Run();
