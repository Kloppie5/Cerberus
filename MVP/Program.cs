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

        foreach(var device in devices)
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


public class PacketCaptureParser
{
    public static PacketEvent? Parse(RawCapture rawCapture)
    {
        var data = rawCapture.Data;
        var timestamp = rawCapture.Timeval.Date;

        Console.Write($"{timestamp}: ");

        var destMac = BitConverter.ToString(data, 0, 6);
        var srcMac = BitConverter.ToString(data, 6, 6);
        var etherType = (data[12] << 8) | data[13];

        Console.Write(
            $"Dest: {destMac}, Src: {srcMac}, EtherType: 0x{etherType:X4} - "
        );

        data = data.Skip(14).ToArray();

        if (etherType != 0x0800)
        {
            Console.WriteLine("Not IPv4, skipping");
            return null;
        }

        var version = data[0] >> 4;
        var ihl = data[0] & 0x0F;
        var dscp = data[1] >> 2;
        var ecn = data[1] & 0x03;
        var totalLength = (data[2] << 8) | data[3];
        var identification = (data[4] << 8) | data[5];
        var flags = data[6] >> 5;
        var fragmentOffset = ((data[6] & 0x1F) << 8) | data[7];
        var ttl = data[8];
        var protocol = data[9];
        var headerChecksum = (data[10] << 8) | data[11];
        var srcIp = $"{data[12]}.{data[13]}.{data[14]}.{data[15]}";
        var dstIp = $"{data[16]}.{data[17]}.{data[18]}.{data[19]}";
        var options = data.Skip(20).Take((ihl - 5) * 4).ToArray();

        data = data.Skip(20 + options.Length).ToArray();

        Console.WriteLine(
            $"{version}, IHL: {ihl * 4} bytes, DSCP: {dscp}, ECN: {ecn}, Total Length: {totalLength}, " +
            $"ID: {identification}, Flags: {flags}, Fragment Offset: {fragmentOffset}, TTL: {ttl}, " +
            $"Protocol: {protocol}, Header Checksum: 0x{headerChecksum:X4}, " +
            $"Src IP: {srcIp}, Dst IP: {dstIp}, Options: {options.Length} bytes, Payload: {data.Length} bytes"
        );

        return null;

        /*
        return new PacketEvent
        {
            Timestamp = timestamp,
            SourceIp = srcIp,
            DestinationIp = dstIp,
            SourcePort = srcPort,
            DestinationPort = dstPort,
            Protocol = protoName,
            Length = data.Length
        };
        */
    }
}
