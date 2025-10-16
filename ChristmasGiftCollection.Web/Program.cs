using Akka.Actor;
using Akka.DependencyInjection;
using ChristmasGiftCollection.Core.Actors;
using ChristmasGiftCollection.Web.Components;
using Marten;
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

builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("giftcollection");

builder.Services.AddMarten(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("giftcollection")
        ?? throw new InvalidOperationException("PostgreSQL connection string 'giftcollection' not found.");

    var options = new StoreOptions();
    options.Connection(connectionString);
    options.DatabaseSchemaName = "gift_collection";

    // Configure for event sourcing
    options.Events.DatabaseSchemaName = "gift_collection_events";

    return options;
})
.UseLightweightSessions()
.ApplyAllDatabaseChangesOnStartup();

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
    var documentStore = sp.GetRequiredService<IDocumentStore>();
    var supervisor = actorSystem.ActorOf(MemberActorSupervisor.Props(documentStore), "member-supervisor");

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

app.MapDefaultEndpoints();

// Ensure Akka.NET actor system shuts down gracefully
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    var actorSystem = app.Services.GetRequiredService<ActorSystem>();
    actorSystem.Terminate().Wait();
});

app.Run();
