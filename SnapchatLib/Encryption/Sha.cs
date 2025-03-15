using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SnapchatLib.Encryption;

internal class Sha
{
    internal string Sha256(string data)
    {
        if (data == null)
            throw new ArgumentNullException("Sha256 data canno't be null");

        var hash = ((SHA256)CryptoConfig.CreateFromName("SHA256")).ComputeHash(Encoding.UTF8.GetBytes(data));
        return hash.Aggregate(string.Empty, (current, x) => current + string.Format("{0:x2}", x));
    }
}