using SharpPcap;
using SharpPcap.LibPcap;

internal class Program
{
    private static void Main(string[] args)
    {
        var devices = CaptureDeviceList.Instance;

        if (devices.Count == 0)
        {
            Console.WriteLine("No devices found");
            return;
        }

        foreach(var device in devices)
        {
            Console.WriteLine($"{device.Name} - {device.Description}");
        }
    }
}