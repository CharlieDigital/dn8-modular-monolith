// This runtime represents the core application API that is run as a web application.
// But the components in this application can be run in other "hosts"

using System.Text.Json;
using System.Text.Json.Serialization;
using ChrlsChn.MoMo.Data;
using ChrlsChn.MoMo.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MoMoConfig>(
  builder.Configuration.GetSection(nameof(MoMoConfig))
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
  .AddJsonOptions(j => {
    // Without this, the Swagger is generated incorrectly.
    j.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    j.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    j.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    j.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
  });
builder.Services.AddDataStore();
builder.Services.AddCustomServices();

var app = builder.Build();

// For this demo, we delete and recreate the database each time
using var scope = app.Services.CreateScope();
var tasks = scope.ServiceProvider.GetService<TaskDatabase>()!;
tasks!.Database.EnsureDeleted();
tasks!.Database.EnsureCreated();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();
