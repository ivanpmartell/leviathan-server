using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace UnityEngine
{
	public sealed class WWW : IDisposable
	{
		private IntPtr wwwWrapper;

		public Dictionary<string, string> responseHeaders
		{
			get
			{
				if (!isDone)
				{
					throw new UnityException("WWW is not finished downloading yet");
				}
				return ParseHTTPHeaderString(responseHeadersString);
			}
		}

		private string responseHeadersString
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string text
		{
			get
			{
				if (!isDone)
				{
					throw new UnityException("WWW is not ready downloading yet");
				}
				return GetTextEncoder().GetString(bytes);
			}
		}

		[Obsolete("Please use WWW.text instead")]
		public string data => text;

		public byte[] bytes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int size
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string error
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Texture2D texture => GetTexture(markNonReadable: false);

		public Texture2D textureNonReadable => GetTexture(markNonReadable: true);

		public AudioClip audioClip => GetAudioClip(threeD: true);

		public MovieTexture movie
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public bool isDone
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float progress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float uploadProgress
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[Obsolete(".oggVorbis accessor is deprecated, use .audioClip or GetAudioClip() instead.")]
		public AudioClip oggVorbis
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public string url
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public AssetBundle assetBundle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public ThreadPriority threadPriority
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public WWW(string url)
		{
			InitWWW(url, null, null);
		}

		public WWW(string url, WWWForm form)
		{
			InitWWW(url, form.data, FlattenedHeadersFrom(form.headers));
		}

		public WWW(string url, byte[] postData)
		{
			InitWWW(url, postData, null);
		}

		public WWW(string url, byte[] postData, Hashtable headers)
		{
			InitWWW(url, postData, FlattenedHeadersFrom(headers));
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		internal extern WWW(string url, int version);

		private static string[] FlattenedHeadersFrom(Hashtable headers)
		{
			if (headers == null)
			{
				return null;
			}
			string[] array = new string[headers.Count * 2];
			int num = 0;
			foreach (DictionaryEntry header in headers)
			{
				array[num++] = header.Key.ToString();
				array[num++] = header.Value.ToString();
			}
			return array;
		}

		internal static Dictionary<string, string> ParseHTTPHeaderString(string input)
		{
			if (input == null)
			{
				throw new ArgumentException("input was null to ParseHTTPHeaderString");
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			StringReader stringReader = new StringReader(input);
			int num = 0;
			while (true)
			{
				string text = stringReader.ReadLine();
				if (text == null)
				{
					break;
				}
				if (num++ == 0 && text.StartsWith("HTTP"))
				{
					dictionary["STATUS"] = text;
					continue;
				}
				int num2 = text.IndexOf(": ");
				if (num2 != -1)
				{
					string key = text.Substring(0, num2).ToUpper();
					string text3 = (dictionary[key] = text.Substring(num2 + 2));
				}
			}
			return dictionary;
		}

		public void Dispose()
		{
			DestroyWWW(cancel: true);
		}

		~WWW()
		{
			DestroyWWW(cancel: false);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void DestroyWWW(bool cancel);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void InitWWW(string url, byte[] postData, string[] iHeaders);

		public static string EscapeURL(string s)
		{
			Encoding uTF = Encoding.UTF8;
			return EscapeURL(s, uTF);
		}

		public static string EscapeURL(string s, Encoding e)
		{
			if (s == null)
			{
				return null;
			}
			if (s == string.Empty)
			{
				return string.Empty;
			}
			return WWWTranscoder.URLEncode(s, e);
		}

		public static string UnEscapeURL(string s)
		{
			Encoding uTF = Encoding.UTF8;
			return UnEscapeURL(s, uTF);
		}

		public static string UnEscapeURL(string s, Encoding e)
		{
			if (s == null)
			{
				return null;
			}
			if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
			{
				return s;
			}
			return WWWTranscoder.URLDecode(s, e);
		}

		private Encoding GetTextEncoder()
		{
			//Discarded unreachable code: IL_0074
			string value = null;
			if (responseHeaders.TryGetValue("CONTENT-TYPE", out value))
			{
				int num = value.IndexOf("charset", StringComparison.OrdinalIgnoreCase);
				if (num > -1)
				{
					int num2 = value.IndexOf('=', num);
					if (num2 > -1)
					{
						string text = value.Substring(num2 + 1).Trim().Trim('\'', '"')
							.Trim();
						try
						{
							return Encoding.GetEncoding(text);
						}
						catch (Exception)
						{
							Debug.Log("Unsupported encoding: '" + text + "'");
						}
					}
				}
			}
			return Encoding.UTF8;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern Texture2D GetTexture(bool markNonReadable);

		public AudioClip GetAudioClip(bool threeD)
		{
			return GetAudioClip(threeD, stream: false);
		}

		public AudioClip GetAudioClip(bool threeD, bool stream)
		{
			return GetAudioClip(threeD, stream, AudioType.UNKNOWN);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern AudioClip GetAudioClip(bool threeD, bool stream, AudioType audioType);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void LoadImageIntoTexture(Texture2D tex);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		[Obsolete("All blocking WWW functions have been deprecated, please use one of the asynchronous functions instead.", true)]
		public static extern string GetURL(string url);

		[Obsolete("All blocking WWW functions have been deprecated, please use one of the asynchronous functions instead.", true)]
		public static Texture2D GetTextureFromURL(string url)
		{
			return new WWW(url).texture;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void LoadUnityWeb();

		public static WWW LoadFromCacheOrDownload(string url, int version)
		{
			return new WWW(url, version);
		}
	}
}
