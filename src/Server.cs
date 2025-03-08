using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_dns_server;
using codecrafters_dns_server.Model;
using static System.Console;

// Resolve UDP address
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 2053;
IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, port);
IPEndPoint? resolverEndpoint = null;

// Create UDP socket
UdpClient udpClient = new UdpClient(udpEndPoint);

var requestPool = new List<(ushort Id, IPEndPoint EndPoint, DnsMessage Message)?>();
var responsePool = new List<(ushort Id, IPEndPoint EndPoint, DnsMessage Message)?>();


if (args.Contains("--resolver"))
{
    var index = args.ToList().IndexOf("--resolver");
    var resolver = args.ToList()[index + 1];

    var resolverIpString = resolver.Split(':')[0];
    IPAddress resolverIp = IPAddress.Parse(resolverIpString);
    int resolverPort = int.Parse(resolver.Split(':')[1]);

    resolverEndpoint = new IPEndPoint(resolverIp, resolverPort);
}

while (true)
{
    // Receive data
    IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
    byte[] receivedData = udpClient.Receive(ref sourceEndPoint);
    string receivedString = Encoding.ASCII.GetString(receivedData);

    var request = DnsMessage.Parse(receivedData);

    if (request.Header.IsResponse)
    {
        WriteLine($"Received response from {sourceEndPoint}: {request}");

        var responseFromForwarder = request.Clone();

        var originalRequest = requestPool.FirstOrDefault(x => x?.Id == request.Header.Id);

        ArgumentNullException.ThrowIfNull(originalRequest);

        if (originalRequest?.Message.Header.QuestionCount > 1)
        {
            var queuedResponse = responsePool.FirstOrDefault(x => x?.Id == originalRequest?.Message.Header.Id);

            if (queuedResponse is not null)
            {
                var responseMessage = queuedResponse?.Message;
                ArgumentNullException.ThrowIfNull(responseMessage);

                responseMessage.Questions = responseFromForwarder!.Questions;

                responseMessage.Answers!.AddRange(responseFromForwarder.Answers!);
                responseMessage.Header.AnswerCount += responseFromForwarder.Header.AnswerCount;

                if (responseMessage.Header.AnswerCount == originalRequest.Value.Message.Header.QuestionCount)
                {
                    WriteLine($"Sending COMBINED response to {queuedResponse!.Value.EndPoint}: {responseMessage}");

                    byte[] responseBytes = responseMessage.ToBytes();
                    await udpClient.SendAsync(responseBytes, responseBytes.Length, originalRequest.Value.EndPoint);
                }
                else
                {
                    WriteLine(
                        $"Answer count: {responseMessage.Header.AnswerCount}, Question count: {originalRequest.Value.Message.Header.QuestionCount}");
                }
            }
            else
            {
                responsePool.Add(
                    (originalRequest.Value.Message.Header.Id, originalRequest.Value.EndPoint, responseFromForwarder)!);
            }
        }
        else
        {
            WriteLine($"Sending response to {originalRequest!.Value.EndPoint}: {request}");
            byte[] responseBytes = responseFromForwarder!.ToBytes();
            await udpClient.SendAsync(responseBytes, responseBytes.Length, originalRequest.Value.EndPoint);

            var index = requestPool.FindIndex(x => x?.Id == responseFromForwarder.Header.Id);
            requestPool.RemoveAt(index);
        }
    }
    else
    {
        WriteLine($"Received query from {sourceEndPoint}: {request}");

        requestPool.Add((request.Header.Id, sourceEndPoint, request));

        if (request.Header.QuestionCount > 1)
        {
            ArgumentNullException.ThrowIfNull(request.Questions);

            foreach (var question in request.Questions)
            {
                var forwardedRequest = request.Clone()!;
                forwardedRequest.Header.QuestionCount = 1;
                forwardedRequest.Questions!.Clear();
                forwardedRequest.Questions.Add(question);
                forwardedRequest.Header.IsResponse = false;

                WriteLine($"Forwarding request to {resolverEndpoint}: {forwardedRequest}");

                byte[] forwardedRequestBytes = forwardedRequest.ToBytes();
                await udpClient.SendAsync(forwardedRequestBytes, forwardedRequestBytes.Length, resolverEndpoint);
            }
        }
        else
        {
            WriteLine($"Forwarding request to {resolverEndpoint}: {request}");

            byte[] forwardedRequestBytes = request.ToBytes();
            await udpClient.SendAsync(forwardedRequestBytes, forwardedRequestBytes.Length, resolverEndpoint);
        }
    }
}