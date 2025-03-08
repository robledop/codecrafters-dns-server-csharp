using System.Text;

namespace codecrafters_dns_server.Model;

public class DnsQuestion
{
    public string? Name { get; set; }
    public DnsRecordType Type { get; set; }
    public DnsRecordClass Class { get; set; }

    public static (List<DnsQuestion>, int) Parse(byte[] bytes, ushort questionsCount)
    {
        const int HEADER_LENGTH = 12;
        var questions = new List<DnsQuestion>();
        var questionsLength = 0;
        for (int i = 0; i < questionsCount; i++)
        {
            var (domainName, qnameLength, offset) = QName.DecodeDomainName(bytes[(HEADER_LENGTH + questionsLength)..]);

            if (offset > 0)
            {
                var (compressedLabel, _, _) = QName.DecodeDomainName(bytes[offset..]);
                domainName.Append(compressedLabel.ToString());
                qnameLength -= 1;
            }

            var qtype = ((bytes[HEADER_LENGTH + questionsLength + qnameLength]) << 8)
                        | (bytes[HEADER_LENGTH + questionsLength + qnameLength + 1]);

            var qclase = ((bytes[HEADER_LENGTH + questionsLength + qnameLength + 2]) << 8)
                         | (bytes[HEADER_LENGTH + questionsLength + qnameLength + 3]);

            questions.Add(new DnsQuestion
            {
                Name = domainName.ToString(),
                Type = (DnsRecordType)qtype,
                Class = (DnsRecordClass)qclase,
            });
        }

        return (questions, questionsLength);
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

        return bytes.ToArray();
    }
}