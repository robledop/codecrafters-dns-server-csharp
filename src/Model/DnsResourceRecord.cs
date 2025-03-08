namespace codecrafters_dns_server.Model;

public class DnsResourceRecord
{
    public string? Name { get; set; }
    public DnsRecordType Type { get; set; }
    public DnsRecordClass Class { get; set; }
    public uint TimeToLive { get; set; }
    public ushort DataLength { get; set; }
    public byte[]? Data { get; set; }
}