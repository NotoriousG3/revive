using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Org.BouncyCastle.Asn1;
using SnapchatLib.Encryption;
using SnapchatLib.Exceptions;

namespace SnapchatLib.Extras;

internal interface IUtilities
{
    long LongRandom(long min, long max);
    ulong NextULong(ulong min, ulong max);
    string GenerateTemporaryRequestToken(string timestamp);
    string GenerateRequestToken(string authtoken, string timestamp);
    string RandomString(int length);
    T JsonDeserializeObject<T>(string data);
    string JsonSerializeObject(object obj);
    string NewGuid();
    Guid ParseGuid(string uuid);
    long UtcTimestamp();
    int GetAge(DateTime dateOfBirth);
    string UnwrapPublicKey(string publicKey);
}

internal class Utilities : IUtilities
{
    private readonly Sha m_Sha = new();
    private readonly Random m_Random = new();

    public string UnwrapPublicKey(string publicKey)
    {
        var publicKeyData = Convert.FromBase64String(publicKey);

        using (var memory = new MemoryStream(publicKeyData))
        using (var asn1 = new Asn1InputStream(memory))
        {
            var sequence = (DerSequence)asn1.ReadObject();
            var octetStringTwo = (DerBitString)sequence[1];

            return Convert.ToBase64String(octetStringTwo.GetBytes());
        }
    }
    public long LongRandom(long min, long max)
    {
        var buf = new byte[8];
        m_Random.NextBytes(buf);
        var longRand = BitConverter.ToInt64(buf, 0);

        return (Math.Abs(longRand % (max - min)) + min);
    }

    public ulong NextULong(ulong min, ulong max)
    {
        // Get a random 64 bit number.

        var buf = new byte[sizeof(ulong)];
        m_Random.NextBytes(buf);
        ulong n = BitConverter.ToUInt64(buf, 0);

        // Scale to between 0 inclusive and 1 exclusive; i.e. [0,1).

        double normalised = n / (ulong.MaxValue + 1.0);

        // Determine result by scaling range and adding minimum.

        double range = (double)max - min;

        return (ulong)(normalised * range) + min;
    }

    public string GenerateTemporaryRequestToken(string timestamp)
    {
        var s1 = "iEk21fuwZApXlz93750dmW22pw389dPwOk" + "m198sOkJEn37DjqZ32lpRu76xmw288xSQ9";
        var s2 = timestamp + "iEk21fuwZApXlz93750dmW22pw389dPwOk";
        var s3 = m_Sha.Sha256(s1);
        var s4 = m_Sha.Sha256(s2);

        var output = "";
        for (var i = 0; i < "0001110111101110001111010101111011010001001110011000110001000110".Length; i++)
        {
            var c = "0001110111101110001111010101111011010001001110011000110001000110"[i];

            if (c == '0')
                output += s3[i];
            else
                output += s4[i];
        }

        return output;
    }

    public string GenerateRequestToken(string authtoken, string timestamp)
    {
        var s1 = "iEk21fuwZApXlz93750dmW22pw389dPwOk" + authtoken;
        var s2 = timestamp + "iEk21fuwZApXlz93750dmW22pw389dPwOk";
        var s3 = m_Sha.Sha256(s1);
        var s4 = m_Sha.Sha256(s2);

        var output = "";
        for (var i = 0; i < "0001110111101110001111010101111011010001001110011000110001000110".Length; i++)
        {
            var c = "0001110111101110001111010101111011010001001110011000110001000110"[i];

            if (c == '0')
                output += s3[i];
            else
                output += s4[i];
        }

        return output;
    }

    public int GetAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;

        var a = (today.Year * 100 + today.Month) * 100 + today.Day;
        var b = (dateOfBirth.Year * 100 + dateOfBirth.Month) * 100 + dateOfBirth.Day;

        return (a - b) / 10000;
    }
    public string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[m_Random.Next(s.Length)]).ToArray());
    }
    public T JsonDeserializeObject<T>(string data)
    {

        var result = JsonSerializer.Deserialize<T>(data, new JsonSerializerOptions
        {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        if (result == null) throw new DeserializationException(nameof(T));
        return result;
    }

    public string JsonSerializeObject(object obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj);
        }
        catch (Exception e)
        {
            throw new SerializationException("Failed to serialize object", e);
        }
    }

    public string NewGuid()
    {
        return Guid.NewGuid().ToString();
    }

    public Guid ParseGuid(string uuid)
    {
        return Guid.Parse(uuid);
    }

    public long UtcTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}