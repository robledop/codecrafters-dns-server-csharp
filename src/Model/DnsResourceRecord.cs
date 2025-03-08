using System.Text;
using static System.Console;

namespace codecrafters_dns_server.Model;

public class DnsResourceRecord
{
    public string? Name { get; set; }
    public DnsRecordType Type { get; set; }
    public DnsRecordClass Class { get; set; }
    public uint TimeToLive { get; set; }
    public ushort DataLength { get; set; }
    public byte[]? Data { get; set; }

    public static List<DnsResourceRecord> Parse(byte[] bytes, int answerCount, int offset)
    {
        const int HEADER_LENGTH = 12;

        var records = new List<DnsResourceRecord>();

        var recordsLength = 0;

        for (int i = 0; i < answerCount; i++)
        {
            var (qname, length, _) = QName.DecodeDomainName(bytes[(HEADER_LENGTH + recordsLength + offset)..]);

            var qType = (bytes[HEADER_LENGTH + recordsLength + length + offset] << 8) |
                        bytes[HEADER_LENGTH + recordsLength + length + offset + 1];

            var qClass = (bytes[HEADER_LENGTH + recordsLength + length + offset + 2] << 8)
                         | bytes[HEADER_LENGTH + recordsLength + length + offset + 3];

            var ttl = bytes[HEADER_LENGTH + recordsLength + length + offset + 4] << 24
                      | bytes[HEADER_LENGTH + recordsLength + length + offset + 5] << 16
                      | bytes[HEADER_LENGTH + recordsLength + length + offset + 6] << 8
                      | bytes[HEADER_LENGTH + recordsLength + length + offset + 7];

            var rdLength = bytes[HEADER_LENGTH + recordsLength + length + offset + 8] << 8
                           | bytes[HEADER_LENGTH + recordsLength + length + offset + 9];

            WriteLine($"RecordsLength: {recordsLength}, Lenght: {length}, RdLength: {rdLength}, Offset: {offset}");
            var rdData = rdLength > 0
                ? bytes[
                    (HEADER_LENGTH + recordsLength + length + offset + 10)
                    ..
                    (HEADER_LENGTH + recordsLength + length + +offset + 10 + rdLength)]
                : [];

            records.Add(new DnsResourceRecord
            {
                Name = qname.ToString(),
                Type = (DnsRecordType)qType,
                Class = (DnsRecordClass)qClass,
                TimeToLive = (uint)ttl,
                DataLength = (ushort)rdLength,
                Data = rdData,
            });

            recordsLength += i + length + 10 + rdLength;
        }

        return records;
    }

    public byte[] ToBytes()
    {
        if (Name is null)
        {
            return [];
        }

        var bytes = new List<byte>();

        foreach (var part in Name.Split('.'))
        {
            bytes.Add(Convert.ToByte(part.Length));
            bytes.AddRange(Encoding.ASCII.GetBytes(part));
        }

        bytes.Add(0);
        bytes.Add(Convert.ToByte((int)Type >> 8));
        bytes.Add(Convert.ToByte((int)Type));

        bytes.Add(Convert.ToByte((int)Class >> 8));
        bytes.Add(Convert.ToByte((int)Class));

        bytes.AddRange(BitConverter.GetBytes(TimeToLive));
        bytes.Add(Convert.ToByte(DataLength >> 8));
        bytes.Add(Convert.ToByte((int)DataLength));
        if (Data != null) bytes.AddRange(Data);

        return bytes.ToArray();
    }
}