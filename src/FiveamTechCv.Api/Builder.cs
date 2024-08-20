namespace Microsoft.Extensions.DependencyInjection;

public static class Builder
{
    public static IServiceCollection AddFiveamTechCvApi(
        this IServiceCollection services, 
        IConfiguration configuration
    )
    {
        services.AddControllers();

        return services;
    }

    public static IApplicationBuilder UseFiveamTechCvApi(this WebApplication app)
    {
        app.MapControllers();

        return app;
    }
}