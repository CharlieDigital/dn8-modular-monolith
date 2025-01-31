using ChrlsChn.MoMo.Utils;

namespace ChrlsChn.MoMo.Setup;

public static class SetupSwaggerExtension
{
    /// <summary>
    /// Performs the setup of the Swagger endpoints.
    /// </summary>
    public static void AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(config =>
        {
            // This pulls the code comments into the Swagger docs.
            // See the core.csproj for how this is set up
            // See: https://github.com/domaindrivendev/Swashbuckle.AspNetCore#include-descriptions-from-xml-comments
            var filePath = Path.Combine(AppContext.BaseDirectory, "core.xml");

            config.IncludeXmlComments(filePath);

            config.DescribeAllParametersInCamelCase();

            config.SwaggerDoc(
                "v1-api",
                new()
                {
                    Version = "v1",
                    Title = "Main API",
                    Description = "Main API",
                    Contact = new() { Name = "MoMo Main API", }
                }
            );

            config.SwaggerDoc(
                "v1-admin",
                new()
                {
                    Version = "v1",
                    Title = "Admin API",
                    Description = "Admin API",
                    Contact = new() { Name = "MoMo Amin API", }
                }
            );

            // Now partition endpoints into the docs based on the attributes.
            config.DocInclusionPredicate(
                (name, def) =>
                {
                    if (name == "v1-admin")
                    {
                        // Only return true if it's an admin route
                        return def.GroupName == Constants.AdminApiGroup;
                    }

                    if (name == "v1-api")
                    {
                        // Only return true if it's a default API group
                        return def.GroupName == Constants.DefaultApiGroup;
                    }

                    // Untagged endpoints are counted as the default API
                    return false;
                }
            );
        });
    }
}
