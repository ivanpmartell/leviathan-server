using System.IO;
using ICSharpCode.SharpZipLib.GZip;

internal class CompressUtils
{
	public static byte[] Compress(byte[] data)
	{
		if (data.Length == 0)
		{
			return new byte[0];
		}
		MemoryStream memoryStream = new MemoryStream();
		GZipOutputStream gZipOutputStream = new GZipOutputStream(memoryStream);
		BinaryWriter binaryWriter = new BinaryWriter(gZipOutputStream);
		binaryWriter.Write(data.Length);
		binaryWriter.Write(data);
		binaryWriter.Close();
		gZipOutputStream.Close();
		return memoryStream.ToArray();
	}

	public static byte[] Decompress(byte[] data)
	{
		if (data.Length == 0)
		{
			return new byte[0];
		}
		MemoryStream baseInputStream = new MemoryStream(data);
		GZipInputStream input = new GZipInputStream(baseInputStream);
		BinaryReader binaryReader = new BinaryReader(input);
		int count = binaryReader.ReadInt32();
		return binaryReader.ReadBytes(count);
	}
}
