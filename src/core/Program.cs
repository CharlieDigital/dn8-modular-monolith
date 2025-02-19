// This runtime represents the core application API that is run as a web application.
// But the components in this application can be run in other "hosts"

using ChrlsChn.MoMo.Common.Config;
using ChrlsChn.MoMo.Common.Data;
using ChrlsChn.MoMo.Setup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MoMoConfig>(builder.Configuration.GetSection(nameof(MoMoConfig)));

Console.WriteLine("Starting app setup...");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCustomSwagger(); // Swagger/OpenAPI config
builder.Services.AddCustomControllers();
builder.Services.AddDataStore(); // Database
builder.Services.AddCustomServices(); // Services

var app = builder.Build();

// For this demo, we delete and recreate the database each time when we are not in codegen.
if (!RuntimeEnv.IsCodegen && !RuntimeEnv.SkipDbReset)
{
    Console.WriteLine("✨ Provisioning database..."); // 👈 Reset the database each time
    using var scope = app.Services.CreateScope();
    var tasks = scope.ServiceProvider.GetService<TaskDatabase>()!;
    tasks!.Database.EnsureDeleted();
    tasks!.Database.EnsureCreated();
}

// 👇 Only expose the swagger UI if we're in development or we explicitly want to
if (RuntimeEnv.IsDevelopment || RuntimeEnv.ExposeSwaggerUI)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.ShowCommonExtensions();

        // 👇 Note that we set up 3 endpoints for the different services.
        options.SwaggerEndpoint("v1-api/swagger.json", "Default API");
        options.SwaggerEndpoint("v1-admin/swagger.json", "Admin API");
        options.SwaggerEndpoint("v1-reporting/swagger.json", "Reporting API");
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();
