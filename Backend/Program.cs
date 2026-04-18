using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Collections;

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

        foreach (var device in devices)
        {
            Console.WriteLine($"{device.Name} - {device.Description}");

            device.OnPacketArrival += (sender, e) =>
            {
                var rawCapture = e.GetPacket();

                var parsed = PacketCaptureParser.Parse(rawCapture);

                if (parsed != null)
                {
                    Console.WriteLine(
                        $"{device.Name}: {parsed.Timestamp} " +
                        $"{parsed.SourceIp}:{parsed.SourcePort} -> " +
                        $"{parsed.DestinationIp}:{parsed.DestinationPort} " +
                        $"{parsed.Protocol} ({parsed.Length} bytes)"
                    );
                }
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
