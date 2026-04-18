using Cerberus.Shared;
using SharpPcap;

namespace Cerberus.Backend;

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

        data = data.Skip(14).ToArray();

        if (etherType == (int)EtherType.IPv4)
        {
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

            if (protocol == (int)Protocol.TCP)
            {
                var srcPort = (data[0] << 8) | data[1];
                var dstPort = (data[2] << 8) | data[3];
                var seqNum = BitConverter.ToUInt32(data, 4);
                var ackNum = BitConverter.ToUInt32(data, 8);
                var dataOffset = data[12] >> 4;
                var tcpFlags = data[13] & 0x3F;
                var windowSize = (data[14] << 8) | data[15];
                var checksum = (data[16] << 8) | data[17];
                var urgentPointer = (data[18] << 8) | data[19];
                var tcpOptions = data.Skip(20).Take((dataOffset - 5) * 4).ToArray();

                data = data.Skip(20 + tcpOptions.Length).ToArray();

                /*
                Console.WriteLine(
                    $"Protocol: TCP, SrcPort: {srcPort}, DstPort: {dstPort}, " +
                    $"SeqNum: {seqNum}, AckNum: {ackNum}, Flags: {tcpFlags}, " +
                    $"WindowSize: {windowSize}, Checksum: {checksum}, UrgentPointer: {urgentPointer}, Length: {data.Length} bytes"
                );
                */

                return new PacketEvent
                {
                    Timestamp = timestamp,
                    SourceIp = srcIp,
                    SourcePort = srcPort,
                    DestinationIp = dstIp,
                    DestinationPort = dstPort,
                    Protocol = "TCP",
                    Length = data.Length
                };
            }
            else if (protocol == (int)Protocol.UDP)
            {
                var srcPort = (data[0] << 8) | data[1];
                var dstPort = (data[2] << 8) | data[3];
                var length = (data[4] << 8) | data[5];
                var checksum = (data[6] << 8) | data[7];

                data = data.Skip(8).ToArray();

                /*
                Console.WriteLine(
                    $"Protocol: UDP, SrcPort: {srcPort}, DstPort: {dstPort}, Checksum: {checksum}, " +
                    $"Length: {length} bytes"
                );
                */

                return new PacketEvent
                {
                    Timestamp = timestamp,
                    SourceIp = srcIp,
                    SourcePort = srcPort,
                    DestinationIp = dstIp,
                    DestinationPort = dstPort,
                    Protocol = "UDP",
                    Length = data.Length
                };
            }
            else
            {
                Console.WriteLine($"Protocol: {protocol}");
                // 2
            }
        }
        else
        {
            Console.WriteLine($"EtherType: 0x{etherType:X}, SrcMAC: {srcMac}, DstMAC: {destMac}");
            // 0x8006
            // 0x8011
        }

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
