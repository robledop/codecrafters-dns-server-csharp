using System.Text;

namespace codecrafters_dns_server.Model;

public class QName
{
    public static (StringBuilder, int, int) DecodeDomainName(byte[] bytes)
    {
        var domainName = new StringBuilder();
        int i = 0;
        int offset = 0;

        while (bytes[i] != 0)
        {
            int length = bytes[i];
            bool isPointer = (length & 0b11000000) == 0b11000000;

            if (isPointer)
            {
                offset = ((length & 0b00111111) << 8) | bytes[i + 1];
                i += 2;
            }
            else
            {
                i += 1;
                var part = bytes[i.. (i + length)];
                domainName.Append(Encoding.ASCII.GetString(part));
                i += length;
            }

            if (bytes[i] != 0)
            {
                domainName.Append('.');
            }
        }

        return (domainName, i + 1, offset);
    }
}