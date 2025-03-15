using System;
using System.IO;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace SnapchatLib.Encryption;

public static class Aes
{
    internal static async Task<byte[]> DecryptData(this SnapchatClient client, byte[] data, byte[] key)
    {
        // AES algorthim with ECB cipher & PKCS5 padding...
        var cipher = CipherUtilities.GetCipher("AES/ECB/PKCS5Padding");

        // Initialise the cipher...
        cipher.Init(false, new KeyParameter(key));

        // Decrypt the data and write the 'final' byte stream...
        var decryptionBytes = cipher.ProcessBytes(data);
        var decryptedFinal = cipher.DoFinal();

        // Write the decrypt bytes & final to memory...
        var decryptedStream = new MemoryStream(decryptionBytes.Length);
        await decryptedStream.WriteAsync(decryptionBytes, 0, decryptionBytes.Length);
        await decryptedStream.WriteAsync(decryptedFinal, 0, decryptedFinal.Length);
        await decryptedStream.FlushAsync();

        var decryptedData = new byte[decryptedStream.Length];
        decryptedStream.Position = 0;
        decryptedStream.Read(decryptedData, 0, (int) decryptedStream.Length);

        return decryptedData;
    }

    internal static async Task<Stream> EncryptData(this SnapchatClient client, byte[] data) //byte[] key, byte[] iv
    {
        // AES algorthim with ECB cipher & PKCS5 padding...
        var engine = new AesEngine();
        var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine), new Pkcs7Padding());

        var randomkey = new SecureRandom();
        var keyBytes = new byte[32]; //key = 32 Bytes = 256 Bits || iv = 16 bytes = 128 bits
        randomkey.NextBytes(keyBytes);
        var kgpkey = new KeyGenerationParameters(randomkey, 256);

        var randomiv = new SecureRandom();
        var ivBytes = new byte[16]; //key = 32 Bytes = 256 Bits || iv = 16 bytes = 128 bits
        randomiv.NextBytes(ivBytes);
        var kgpiv = new KeyGenerationParameters(randomiv, 128);

        var kgkey = new CipherKeyGenerator();
        kgkey.Init(kgpkey);

        var kgiv = new CipherKeyGenerator();
        kgiv.Init(kgpiv);


        var key = kgkey.GenerateKey();
        var iv = kgiv.GenerateKey();

        client.IV = Convert.ToBase64String(iv);
        client.KEY = Convert.ToBase64String(key);

        var keyParam = new KeyParameter(key);

        var keyParamWithIv = new ParametersWithIV(keyParam, iv);

        cipher.Init(true, keyParamWithIv);

        // Encrypt the data and write the 'final' byte stream...
        var encryptionBytes = cipher.ProcessBytes(data);
        var encryptedFinal = cipher.DoFinal();

        cipher.Reset();
        engine.Reset();

        // Write the encrypt bytes & final to memory...
        var enryptedStream = new MemoryStream(encryptionBytes.Length);
        await enryptedStream.WriteAsync(encryptionBytes, 0, encryptionBytes.Length);
        await enryptedStream.WriteAsync(encryptedFinal, 0, encryptedFinal.Length);
        enryptedStream.Seek(0, SeekOrigin.Begin);
        return enryptedStream;
    }
}