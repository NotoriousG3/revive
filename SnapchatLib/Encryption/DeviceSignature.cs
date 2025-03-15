using System;
using System.Security.Cryptography;
using System.Text;

namespace SnapchatLib.Encryption;

internal class DeviceSignature
{
    internal string Sign(string deviceTokenValue, string username, string timestamp, string requestToken)
    {
        using (var mac = new HMACSHA256(Encoding.UTF8.GetBytes(deviceTokenValue)))
        {
            var token = new StringBuilder();
            token.Append(username);
            token.Append("|");
            token.Append(timestamp);
            token.Append("|");
            token.Append(requestToken);

            var result = mac.ComputeHash(Encoding.UTF8.GetBytes(token.ToString())).AsSpan();

            return BitConverter
                .ToString(result.Slice(0, 10).ToArray())
                .Replace("-", "")
                .ToUpper();
        }
    }
}