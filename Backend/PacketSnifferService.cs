using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using SharpPcap;

namespace Cerberus.Backend;

public class PacketSnifferService : BackgroundService
{
    private readonly IHubContext<PacketEventHub> _hubContext;
    private List<ICaptureDevice>? _devices;

    private bool _isCapturing;
    public bool IsCapturing => _isCapturing;

    public PacketSnifferService(IHubContext<PacketEventHub> hubContext)
    {
        _hubContext = hubContext;
        _isCapturing = false;
    }

    public void StartCapture()
    {
        if (_isCapturing)
            return;

        _devices = CaptureDeviceList.Instance.ToList<ICaptureDevice>();

        foreach (var device in _devices)
        {
            device.Open(DeviceModes.Promiscuous);
            device.OnPacketArrival += OnPacketArrival;
            device.StartCapture();
            Console.WriteLine($"Started capturing on {device.Description}");
        }

        _isCapturing = true;

    }

    public void StopCapture()
    {
        if (!_isCapturing || _devices == null)
            return;

        foreach (var device in _devices)
        {
            device.StopCapture();
            device.OnPacketArrival -= OnPacketArrival;
            device.Close();
        }
        _isCapturing = false;
        Console.WriteLine("Stopped capturing packets");
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        try
        {
            var rawCapture = e.GetPacket();
            var packet = PacketCaptureParser.Parse(rawCapture);

            if (packet != null)
            {
                _hubContext.Clients.All.SendAsync("ReceivePacket", packet).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing packet: {ex.Message}");
        }
    }

    public List<string> GetAvailableDevices()
    {
        return CaptureDeviceList.Instance
            .Select(d => d.Description)
            .ToList();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
