namespace codecrafters_dns_server.Model;

public class DnsPacket 
{
    public ushort Id { get; set; }
    public bool IsResponse { get; set; }
    public DnsOpCode OpCode { get; set; }
    public bool IsAuthoritativeAnswer { get; set; }
    public bool IsTruncated { get; set; }
    public bool IsRecursionDesired { get; set; }
    public bool IsRecursionAvailable { get; set; }
    public byte Reserved { get; set; }
    public DnsResponseCode ResponseCode { get; set; }
    public ushort QuestionCount { get; set; }
    public ushort AnswerCount { get; set; }
    public ushort AuthorityCount { get; set; }
    public ushort AdditionalCount { get; set; }
    public List<DnsQuestion>? Questions { get; set; }
    public List<DnsResourceRecord>? Answers { get; set; }
    public List<DnsResourceRecord>? Authority { get; set; }
    public List<DnsResourceRecord>? Additional { get; set; }

    public static DnsPacket Parse(byte[] data)
    {
        return new DnsPacket
        {
            Id = (ushort)((data[0] << 8) | data[1]),
            IsResponse = (data[2] & 0b10000000) != 0,
            OpCode = (DnsOpCode)((data[2] & 0b01111000) >> 3),
            IsAuthoritativeAnswer = (data[2] & 0b00000100) != 0,
            IsTruncated = (data[2] & 0b00001000) != 0,
            IsRecursionDesired = (data[2] & 0b00000001) != 0,
            IsRecursionAvailable = (data[2] & 0b00100000) != 0,
            Reserved = (byte)(data[2] & 0b11000000 >> 6),
            ResponseCode = (DnsResponseCode)(data[3] & 0b00001111),
            QuestionCount = (ushort)((data[4] << 8) | data[5]),
            AnswerCount = (ushort)((data[6] << 8) | data[7]),
            AuthorityCount = (ushort)((data[8] << 8) | data[9]),
            AdditionalCount = (ushort)((data[10] << 8) | data[11]),
        };
    }

    public override string ToString()
    {
        return
            $"Id: {Id}, IsResponse: {IsResponse}, OpCode: {OpCode}, IsAuthoritativeAnswer: {IsAuthoritativeAnswer}, IsTruncated: {IsTruncated}, IsRecursionDesired: {IsRecursionDesired}, IsRecursionAvailable: {IsRecursionAvailable}, ResponseCode: {ResponseCode}, QuestionCount: {QuestionCount}, AnswerCount: {AnswerCount}, AuthorityCount: {AuthorityCount}, AdditionalCount: {AdditionalCount}";
    }

    public byte[] ToBytes()
    {
        var response = new byte[12];

        response[0] = (byte)(Id >> 8);
        response[1] = (byte)Id;
        response[2] = (byte)(
            (IsResponse ? 0b10000000 : 0) |
            ((byte)OpCode << 3) |
            (IsAuthoritativeAnswer ? 0b00000100 : 0) |
            (IsTruncated ? 0b00001000 : 0) |
            (IsRecursionDesired ? 0b00000001 : 0)
        );
        response[3] = (byte)(
            (IsRecursionAvailable ? 0b00100000 : 0) |
            (Reserved << 4) |
            (byte)ResponseCode
        );
        response[4] = (byte)(QuestionCount >> 8);
        response[5] = (byte)QuestionCount;
        response[6] = (byte)(AnswerCount >> 8);
        response[7] = (byte)AnswerCount;
        response[8] = (byte)(AuthorityCount >> 8);
        response[9] = (byte)AuthorityCount;
        response[10] = (byte)(AdditionalCount >> 8);
        response[11] = (byte)AdditionalCount;

        return response;
    }
}