using System.Text.Json;

namespace codecrafters_dns_server.Model;

public class DnsMessage
{
    public required DnsHeader Header { get; set; }
    public List<DnsQuestion>? Questions { get; set; }
    public List<DnsResourceRecord>? Answers { get; set; }
    public List<DnsResourceRecord>? Authority { get; set; }
    public List<DnsResourceRecord>? Additional { get; set; }

    public static DnsMessage Parse(byte[] data)
    {
        var header = DnsHeader.Parse(data);

        var message = new DnsMessage
        {
            Header = header,
        };

        var (questions, questionsLength) = DnsQuestion.Parse(data, header.QuestionCount);
        message.Questions = questions;

        var answers = DnsResourceRecord.Parse(data, header.AnswerCount, questionsLength);
        message.Answers = answers;

        return message;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
    }

    public byte[] ToBytes()
    {
        List<byte> response = [..Header.ToBytes()];

        if (Questions != null)
        {
            foreach (var question in Questions)
            {
                response.AddRange(question.ToBytes());
            }
        }

        if (Answers != null)
        {
            foreach (var answer in Answers)
            {
                response.AddRange(answer.ToBytes());
            }
        }

        return response.ToArray();
    }

    public void AddAnswer(DnsResourceRecord resourceRecord)
    {
        Answers ??= [];
        Answers.Add(resourceRecord);
        Header.AnswerCount++;
    }
}