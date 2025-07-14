
using biogenom_test.Data.Database;
using biogenom_test.Services;
using Microsoft.EntityFrameworkCore;

namespace biogenom_test;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddDbContext<PersonalReportContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<PersonalReportService>();
        builder.Services.AddScoped<SupplementKitRecommendationService>();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.MapOpenApi();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI v1")
        );
        

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
