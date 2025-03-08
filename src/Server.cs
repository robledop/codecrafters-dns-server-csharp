using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_dns_server;
using codecrafters_dns_server.Model;
using static System.Console;

// Uncomment this block to pass the first stage
// Resolve UDP address
IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
int port = 2053;
IPEndPoint udpEndPoint = new IPEndPoint(ipAddress, port);

// Create UDP socket
UdpClient udpClient = new UdpClient(udpEndPoint);

while (true)
{
    // Receive data
    IPEndPoint sourceEndPoint = new IPEndPoint(IPAddress.Any, 0);
    byte[] receivedData = udpClient.Receive(ref sourceEndPoint);
    string receivedString = Encoding.ASCII.GetString(receivedData);

    var dnsPacket = DnsPacket.Parse(receivedData);

    WriteLine($"DNS Packet received from {sourceEndPoint}: {dnsPacket}");

    var responsePacket = dnsPacket.Clone();
    ArgumentNullException.ThrowIfNull(responsePacket);
    responsePacket.IsResponse = true;

    WriteLine($"Sending response to {sourceEndPoint}: {responsePacket}");

    byte[] response = responsePacket.ToBytes();

    // Send response
    await udpClient.SendAsync(response, response.Length, sourceEndPoint);
}