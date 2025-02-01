using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChrlsChn.MoMo.Controllers;
using ChrlsChn.MoMo.Reporting.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ChrlsChn.MoMo.Setup;

public static class SetupControllersExtension
{
    /// <summary>
    /// Performs custom setup of controllers.  Here we have the option of using
    /// the default controller initialization or loading specific controllers.
    /// </summary>
    public static void AddCustomControllers(this IServiceCollection services)
    {
        var loadAdminControllers =
            Environment.GetEnvironmentVariable("ENABLE_ADMIN_ROUTES") == "true";
        var loadApiControllers =
            Environment.GetEnvironmentVariable("ENABLE_DEFAULT_ROUTES") == "true";
        var loadReportingControllers =
            Environment.GetEnvironmentVariable("ENABLE_REPORTING_ROUTES") == "true";

        var loadDefault = !(loadAdminControllers || loadApiControllers || loadReportingControllers);

        static void ConfigJsonOptions(Microsoft.AspNetCore.Mvc.JsonOptions j)
        {
            // Without this, the Swagger is generated incorrectly.
            j.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            j.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            j.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            j.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        if (loadDefault)
        {
            Console.WriteLine(" ⮑  Loading all controllers");

            // Default controller setup; use for local development.
            services.AddControllers().AddJsonOptions(ConfigJsonOptions);
        }
        else
        {
            Console.WriteLine(" ⮑  Loading enabled controllers");

            // TODO: Use more clever loading and partitioning logic here to remove manual registration
            // We build up our controller types here
            var enabledControllers = new List<Type>();

            if (loadAdminControllers)
            {
                Console.WriteLine("   ⮑  Loading admin controllers");
                enabledControllers.Add(typeof(AdminController));
            }

            if (loadApiControllers)
            {
                Console.WriteLine("   ⮑  Loading API controllers");
                enabledControllers.Add(typeof(ProjectController));
                enabledControllers.Add(typeof(UserController));
                enabledControllers.Add(typeof(WorkItemController));
            }

            if (loadReportingControllers)
            {
                Console.WriteLine("   ⮑  Loading reporting controllers");
                enabledControllers.Add(typeof(ReportingController));
            }

            // Partition the controllers and load only the enabled ones.
            // Add the controllers manually; we do not use .AddControllers directly as
            // this will load ALL controllers
            // See this logic here: https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc/src/MvcServiceCollectionExtensions.cs#L132
            // And see this gist: https://gist.github.com/damianh/5d69be0e3004024f03b6cc876d7b0bd3
            services
                .AddMvcCore()
                .AddApiExplorer()
                .AddAuthorization()
                .AddCors()
                .AddDataAnnotations()
                .AddFormatterMappings()
                .AddJsonOptions(ConfigJsonOptions)
                .ConfigureApplicationPartManager(manager =>
                {
                    // See: https://stackoverflow.com/a/68551696/116051
                    // See: https://gist.github.com/damianh/5d69be0e3004024f03b6cc876d7b0bd3
                    manager.FeatureProviders.Add(new InternalControllerFeatureProvider());
                    manager.ApplicationParts.Clear();
                    manager.ApplicationParts.Add(
                        new SelectedControllersApplicationParts(enabledControllers)
                    );
                });
        }
    }

    /// <summary>
    /// Only instantiates selected controllers, not all of them. Prevents application scanning for controllers.
    /// This is needed to prevent instantiation of the Publishing controllers.
    /// </summary>
    private sealed class SelectedControllersApplicationParts
        : ApplicationPart,
            IApplicationPartTypeProvider
    {
        public SelectedControllersApplicationParts()
        {
            Name = "Only allow selected controllers";

            Types = [.. Assembly.GetExecutingAssembly().GetTypes().Select(t => t.GetTypeInfo())];
        }

        public SelectedControllersApplicationParts(IEnumerable<Type> types)
        {
            Name = "Only allow selected controllers";

            Types = [.. types.Select(x => x.GetTypeInfo())];
        }

        public override string Name { get; }

        public IEnumerable<TypeInfo> Types { get; }
    }

    /// <summary>
    /// Ensure that internal controllers are also allowed. The default controllerfeatureprovider
    /// hides internal controllers, but this one allows it.
    /// </summary>
    private sealed class InternalControllerFeatureProvider : ControllerFeatureProvider
    {
        private const string ControllerTypeNameSuffix = "Controller";

        /// <summary>
        /// Determines if a given <paramref name="typeInfo"/> is a controller. The default controllerfeatureprovider
        /// hides internal controllers, but this one allows it.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/> candidate.</param>
        /// <returns><code>true</code> if the type is a controller; otherwise <code>false</code>.</returns>
        protected override bool IsController(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
            {
                return false;
            }

            if (typeInfo.IsAbstract)
            {
                return false;
            }

            if (typeInfo.ContainsGenericParameters)
            {
                return false;
            }

            if (typeInfo.IsDefined(typeof(Microsoft.AspNetCore.Mvc.NonControllerAttribute)))
            {
                return false;
            }

            if (
                !typeInfo.Name.EndsWith(
                    ControllerTypeNameSuffix,
                    StringComparison.OrdinalIgnoreCase
                ) && !typeInfo.IsDefined(typeof(Microsoft.AspNetCore.Mvc.ControllerAttribute))
            )
            {
                return false;
            }

            return true;
        }
    }
}
