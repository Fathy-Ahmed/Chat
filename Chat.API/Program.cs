
using Chat.API.Extensions;
using Chat.API.Hubs;
using Chat.Application.DependencyInjection;
using Chat.Infrastructure.DependencyInjection;
using Chat.Infrastructure.Services;
using Microsoft.Extensions.FileProviders;

namespace Chat.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApi(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbSeeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await dbSeeder.SeedAsync();
        }

        var webRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "Web"));
        if (Directory.Exists(webRoot))
        {
            var fileProvider = new PhysicalFileProvider(webRoot);
            app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
            app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment() || true)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Support custom folder path if specified, but default UseStaticFiles covers wwwroot/uploads
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();
        app.MapHub<ChatHub>("/chathub").RequireCors("AllowAll");

        app.Run();
    }
}
