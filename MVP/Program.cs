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

            device.OnPacketArrival += (sender, e) =>
            {
                var packet = e.GetPacket();
                Console.WriteLine($"{device.Name}: Packet: {packet.Timeval.Date} Length: {packet.Data.Length}");
            };

            device.Open(DeviceModes.Promiscuous);
            device.StartCapture();
        }

        Console.WriteLine("Capturing");
        Console.ReadLine();

        foreach (var device in devices)
        {
            device.StopCapture();
            device.Close();
        }
    }
}