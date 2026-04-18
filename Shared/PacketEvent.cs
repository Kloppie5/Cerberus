namespace Cerberus.Shared;

public class PacketEvent
{
    public required DateTime Timestamp { get; set; }

    public string? SourceIp { get; set; }
    public int? SourcePort { get; set; }

    public string? DestinationIp { get; set; }
    public int? DestinationPort { get; set; }

    public string Protocol { get; set; } = "Unknown";

    public int Length { get; set; }
}
