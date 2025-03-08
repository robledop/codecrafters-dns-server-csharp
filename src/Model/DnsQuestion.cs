namespace codecrafters_dns_server.Model;

public class DnsQuestion
{
    public string? Name { get; set; }
    public DnsRecordType Type { get; set; }
    public DnsRecordClass Class { get; set; }
}