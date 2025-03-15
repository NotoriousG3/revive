using System;
using System.Linq;
using System.Text;

public static class GuidUtils
{
	public static byte[] ToBytes(string uuid)
	{
		var bytes = Guid.Parse(uuid).ToByteArray();

		Array.Reverse(bytes, 0, 4);
		Array.Reverse(bytes, 4, 2);
		Array.Reverse(bytes, 6, 2);

		return bytes;
	}
	private static string Reverse(string s)
	{
		char[] charArray = s.ToCharArray();
		Array.Reverse(charArray);
		return new string(charArray);
	}
	public static UInt64 GetLowBits(string uuid)
	{
		string guid = BitConverter.ToString(Guid.Parse(Reverse(Guid.Parse(uuid).ToString().Replace("-", ""))).ToByteArray());

		StringBuilder stringBuilder = new StringBuilder();
		foreach (string splitted in guid.Split("-"))
		{
			stringBuilder.Append(Reverse(splitted));
		}

		byte[] a = Guid.Parse(stringBuilder.ToString()).ToByteArray();

		var low = a.Take(8).ToArray();

		return BitConverter.ToUInt64(low);
	}

	public static UInt64 GetHighBits(string uuid)
	{
		string guid = BitConverter.ToString(Guid.Parse(Reverse(Guid.Parse(uuid).ToString().Replace("-", ""))).ToByteArray());

		StringBuilder stringBuilder = new StringBuilder();
		foreach (string splitted in guid.Split("-"))
		{
			stringBuilder.Append(Reverse(splitted));
		}

		byte[] a = Guid.Parse(stringBuilder.ToString()).ToByteArray();

		var high = a.Skip(8).Take(8).ToArray();

		return BitConverter.ToUInt64(high);
	}
	public static string ToString(byte[] bytes)
	{
		// Copy.
		var copy = new byte[bytes.Length];
		Buffer.BlockCopy(bytes, 0, copy, 0, bytes.Length);

		// Reverse.
		Array.Reverse(copy, 0, 4);
		Array.Reverse(copy, 4, 2);
		Array.Reverse(copy, 6, 2);

		return new Guid(copy).ToString();
	}
}