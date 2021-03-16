using System;
using System.IO;

internal class NewsPost
{
	public string m_title;

	public DateTime m_date;

	public string m_category;

	public string m_content;

	public NewsPost()
	{
	}

	public NewsPost(byte[] data)
	{
		FromArray(data);
	}

	public byte[] ToArray()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(m_title);
		binaryWriter.Write(m_date.ToBinary());
		binaryWriter.Write(m_category);
		binaryWriter.Write(m_content);
		return memoryStream.ToArray();
	}

	public void FromArray(byte[] data)
	{
		MemoryStream input = new MemoryStream(data);
		BinaryReader binaryReader = new BinaryReader(input);
		m_title = binaryReader.ReadString();
		m_date = DateTime.FromBinary(binaryReader.ReadInt64());
		m_category = binaryReader.ReadString();
		m_content = binaryReader.ReadString();
	}
}
