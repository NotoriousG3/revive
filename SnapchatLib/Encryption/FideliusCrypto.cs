using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using System.IO;
using System;

namespace Janus.Crypto.Fidelius;

internal static class FideliusCrypto
{
    public static byte[] GetPublicKey(AsymmetricKeyParameter publicKey)
    {
        var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);

        using (var memory = new MemoryStream())
        using (var asn1 = Asn1OutputStream.Create(memory))
        {
            asn1.WriteObject(new DerSequence(new Asn1EncodableVector
            {
                new DerSequence(new Asn1EncodableVector
                {
                    X9ObjectIdentifiers.IdECPublicKey,
                    X9ObjectIdentifiers.Prime256v1
                }),
                publicKeyInfo.PublicKeyData
            }));

            asn1.Flush();

            return memory.ToArray();
        }
    }

    public static byte[] GetPrivateKey(ECPrivateKeyParameters privateKey)
    {
        using (var memory = new MemoryStream())
        using (var asn1 = Asn1OutputStream.Create(memory))
        {
            asn1.WriteObject(new DerSequence(new Asn1EncodableVector
            {
                new DerInteger(0),
                new DerSequence(new Asn1EncodableVector
                {
                    X9ObjectIdentifiers.IdECPublicKey,
                    X9ObjectIdentifiers.Prime256v1
                }),
                new DerOctetString(new DerSequence(new Asn1EncodableVector
                {
                    new DerInteger(1),
                    new DerOctetString(BigIntegers.AsUnsignedByteArray((privateKey.D.BitLength + 7) / 8, privateKey.D))
                }))
            }));

            asn1.Flush();

            return memory.ToArray();
        }
    }

    public static string UnwrapPublicKey(string publicKey)
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

    public static byte[] UnwrapPublicKey(byte[] publicKey)
    {
        using (var memory = new MemoryStream(publicKey))
        using (var asn1 = new Asn1InputStream(memory))
        {
            var sequence = (DerSequence)asn1.ReadObject();
            var octetStringTwo = (DerBitString)sequence[1];

            return octetStringTwo.GetBytes();
        }
    }

    public static string WrapPublicKey(string publicKey)
    {
        using (var memory = new MemoryStream())
        using (var asn1 = Asn1OutputStream.Create(memory))
        {
            asn1.WriteObject(new DerSequence(new Asn1EncodableVector
            {
                new DerSequence(new Asn1EncodableVector
                {
                    X9ObjectIdentifiers.IdECPublicKey,
                    X9ObjectIdentifiers.Prime256v1
                }),
                new DerBitString(Convert.FromBase64String(publicKey))
            }));

            asn1.Flush();

            return Convert.ToBase64String(memory.ToArray());
        }
    }

    public static BigInteger UnwrapPrivateKey(byte[] data)
    {
        using (var memory = new MemoryStream(data))
        using (var asn1 = new Asn1InputStream(memory))
        {
            var sequence = (DerSequence)asn1.ReadObject();
            var octetString = (DerOctetString)sequence[2];

            using (var asn1InsideOctet = octetString.Parser.GetOctetStream())
            using (var asn1Inside = new Asn1InputStream(asn1InsideOctet))
            {
                var sequenceTwo = (DerSequence)asn1Inside.ReadObject();
                var octetStringTwo = (DerOctetString)sequenceTwo[1];

                using (var stream = octetStringTwo.Parser.GetOctetStream())
                {
                    var fullData = new byte[stream.Length];
                    var read = stream.Read(fullData, 0, (int)stream.Length);
                    if (read != stream.Length)
                    {
                        throw new Exception("Read invalid amount.");
                    }

                    return new BigInteger(1, fullData);
                }
            }
        }
    }
}