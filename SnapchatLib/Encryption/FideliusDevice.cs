using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Janus.Crypto.Fidelius;

internal record FideliusDevice(byte[] Iwek, byte[] Public, byte[] PublicHash, byte[] Private)
{
    public byte[] PublicUnwrapped => FideliusCrypto.UnwrapPublicKey(Public);

    [JsonIgnore]
    public string PublicBase64 => Convert.ToBase64String(Public);

    /// <summary>
    ///     Used for RecipientKey in messages.
    /// </summary>
    [JsonIgnore]
    public string PublicUnwrappedBase64 => FideliusCrypto.UnwrapPublicKey(PublicBase64);

    [JsonIgnore]
    public string PrivateBase64 => Convert.ToBase64String(Private);

    public static FideliusDevice Create()
    {
        var data = new byte[32];

        RandomNumberGenerator.Fill(data);

        // Create secp256r1 keypair.
        var curve = SecNamedCurves.GetByName("secp256r1");
        var keyGenerator = new ECKeyPairGenerator();
        var domainParams = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());

        keyGenerator.Init(new ECKeyGenerationParameters(domainParams, new SecureRandom()));

        var keyPair = keyGenerator.GenerateKeyPair();

        // Create correct asn1 formatted keys.
        var keyPublic = FideliusCrypto.GetPublicKey((ECPublicKeyParameters)keyPair.Public);
        var keyPrivate = FideliusCrypto.GetPrivateKey((ECPrivateKeyParameters)keyPair.Private);

        return new FideliusDevice(data, keyPublic, HMACSHA256.HashData(data, keyPublic), keyPrivate);
    }
}