using System.Collections;
using System.IO;
using System.Text;

namespace UnityEngine
{
	public sealed class WWWForm
	{
		private ArrayList formData;

		private ArrayList fieldNames;

		private ArrayList fileNames;

		private ArrayList types;

		private byte[] boundary;

		private bool containsFiles;

		public Hashtable headers
		{
			get
			{
				Hashtable hashtable = new Hashtable();
				if (containsFiles)
				{
					hashtable["Content-Type"] = "multipart/form-data; boundary=\"" + Encoding.UTF8.GetString(boundary) + "\"";
				}
				else
				{
					hashtable["Content-Type"] = "application/x-www-form-urlencoded";
				}
				return hashtable;
			}
		}

		public byte[] data
		{
			get
			{
				//Discarded unreachable code: IL_0315, IL_0405
				if (containsFiles)
				{
					byte[] bytes = Encoding.ASCII.GetBytes("--");
					byte[] bytes2 = Encoding.ASCII.GetBytes("\r\n");
					byte[] bytes3 = Encoding.ASCII.GetBytes("Content-Type: ");
					byte[] bytes4 = Encoding.ASCII.GetBytes("Content-disposition: form-data; name=\"");
					byte[] bytes5 = Encoding.ASCII.GetBytes("\"");
					byte[] bytes6 = Encoding.ASCII.GetBytes("; filename=\"");
					using MemoryStream memoryStream = new MemoryStream(1024);
					for (int i = 0; i < formData.Count; i++)
					{
						memoryStream.Write(bytes2, 0, bytes2.Length);
						memoryStream.Write(bytes, 0, bytes.Length);
						memoryStream.Write(boundary, 0, boundary.Length);
						memoryStream.Write(bytes2, 0, bytes2.Length);
						memoryStream.Write(bytes3, 0, bytes3.Length);
						byte[] bytes7 = Encoding.UTF8.GetBytes((string)types[i]);
						memoryStream.Write(bytes7, 0, bytes7.Length);
						memoryStream.Write(bytes2, 0, bytes2.Length);
						memoryStream.Write(bytes4, 0, bytes4.Length);
						string text = (string)fieldNames[i];
						if (!WWWTranscoder.SevenBitClean(text, Encoding.UTF8) || text.IndexOf("=?") > -1)
						{
							text = "=?" + Encoding.UTF8.HeaderName + "?Q?" + WWWTranscoder.QPEncode(text, Encoding.UTF8) + "?=";
						}
						byte[] bytes8 = Encoding.UTF8.GetBytes(text);
						memoryStream.Write(bytes8, 0, bytes8.Length);
						memoryStream.Write(bytes5, 0, bytes5.Length);
						if (fileNames[i] != null)
						{
							string text2 = (string)fileNames[i];
							if (!WWWTranscoder.SevenBitClean(text2, Encoding.UTF8) || text2.IndexOf("=?") > -1)
							{
								text2 = "=?" + Encoding.UTF8.HeaderName + "?Q?" + WWWTranscoder.QPEncode(text2, Encoding.UTF8) + "?=";
							}
							byte[] bytes9 = Encoding.UTF8.GetBytes(text2);
							memoryStream.Write(bytes6, 0, bytes6.Length);
							memoryStream.Write(bytes9, 0, bytes9.Length);
							memoryStream.Write(bytes5, 0, bytes5.Length);
						}
						memoryStream.Write(bytes2, 0, bytes2.Length);
						memoryStream.Write(bytes2, 0, bytes2.Length);
						byte[] array = (byte[])formData[i];
						memoryStream.Write(array, 0, array.Length);
					}
					memoryStream.Write(bytes2, 0, bytes2.Length);
					memoryStream.Write(bytes, 0, bytes.Length);
					memoryStream.Write(boundary, 0, boundary.Length);
					memoryStream.Write(bytes, 0, bytes.Length);
					memoryStream.Write(bytes2, 0, bytes2.Length);
					return memoryStream.ToArray();
				}
				byte[] bytes10 = Encoding.ASCII.GetBytes("&");
				byte[] bytes11 = Encoding.ASCII.GetBytes("=");
				using MemoryStream memoryStream2 = new MemoryStream(1024);
				for (int j = 0; j < formData.Count; j++)
				{
					byte[] array2 = WWWTranscoder.URLEncode(Encoding.UTF8.GetBytes((string)fieldNames[j]));
					byte[] toEncode = (byte[])formData[j];
					byte[] array3 = WWWTranscoder.URLEncode(toEncode);
					if (j > 0)
					{
						memoryStream2.Write(bytes10, 0, bytes10.Length);
					}
					memoryStream2.Write(array2, 0, array2.Length);
					memoryStream2.Write(bytes11, 0, bytes11.Length);
					memoryStream2.Write(array3, 0, array3.Length);
				}
				return memoryStream2.ToArray();
			}
		}

		public WWWForm()
		{
			formData = new ArrayList();
			fieldNames = new ArrayList();
			fileNames = new ArrayList();
			types = new ArrayList();
			boundary = new byte[40];
			for (int i = 0; i < 40; i++)
			{
				int num = Random.Range(48, 110);
				if (num > 57)
				{
					num += 7;
				}
				if (num > 90)
				{
					num += 6;
				}
				boundary[i] = (byte)num;
			}
		}

		public void AddField(string fieldName, string value)
		{
			Encoding uTF = Encoding.UTF8;
			AddField(fieldName, value, uTF);
		}

		public void AddField(string fieldName, string value, Encoding e)
		{
			fieldNames.Add(fieldName);
			fileNames.Add(null);
			formData.Add(e.GetBytes(value));
			types.Add("text/plain; charset=\"" + e.WebName + "\"");
		}

		public void AddField(string fieldName, int i)
		{
			AddField(fieldName, i.ToString());
		}

		public void AddBinaryData(string fieldName, byte[] contents, string fileName)
		{
			string mimeType = null;
			AddBinaryData(fieldName, contents, fileName, mimeType);
		}

		public void AddBinaryData(string fieldName, byte[] contents)
		{
			string mimeType = null;
			string fileName = null;
			AddBinaryData(fieldName, contents, fileName, mimeType);
		}

		public void AddBinaryData(string fieldName, byte[] contents, string fileName, string mimeType)
		{
			containsFiles = true;
			bool flag = contents.Length > 8 && contents[0] == 137 && contents[1] == 80 && contents[2] == 78 && contents[3] == 71 && contents[4] == 13 && contents[5] == 10 && contents[6] == 26 && contents[7] == 10;
			if (fileName == null)
			{
				fileName = fieldName + ((!flag) ? ".dat" : ".png");
			}
			if (mimeType == null)
			{
				mimeType = ((!flag) ? "application/octet-stream" : "image/png");
			}
			fieldNames.Add(fieldName);
			fileNames.Add(fileName);
			formData.Add(contents);
			types.Add(mimeType);
		}
	}
}
