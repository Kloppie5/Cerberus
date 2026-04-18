using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Cerberus.Shared;
using Cerberus.Backend;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddSignalR();
        builder.Services.AddSingleton<PacketSnifferService>();
        builder.Services.AddHostedService(sp => sp.GetRequiredService<PacketSnifferService>());

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("https://localhost:7000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.UseCors();

        app.MapHub<PacketEventHub>("/packetHub");

        app.MapPost("/api/capture/start", async (PacketSnifferService service) =>
        {
            service.StartCapture();
            return Results.Ok("Capture started");
        });

        app.MapPost("/api/capture/stop", async (PacketSnifferService service) =>
        {
            service.StopCapture();
            return Results.Ok("Capture stopped");
        });

        app.MapGet("/api/capture/devices", (PacketSnifferService service) =>
        {
            return service.GetAvailableDevices();
        });

        app.MapGet("/api/capture/status", (PacketSnifferService service) =>
        {
            return new { isCapturing = service.IsCapturing };
        });

        app.Run();
    }
}