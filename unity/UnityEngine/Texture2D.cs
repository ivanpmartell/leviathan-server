using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Texture2D : Texture
	{
		public int mipmapCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public TextureFormat format
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Texture2D(int width, int height)
		{
			Internal_Create(this, width, height, TextureFormat.ARGB32, mipmap: true, linear: false);
		}

		public Texture2D(int width, int height, TextureFormat format, bool mipmap)
		{
			Internal_Create(this, width, height, format, mipmap, linear: false);
		}

		public Texture2D(int width, int height, TextureFormat format, bool mipmap, bool linear)
		{
			Internal_Create(this, width, height, format, mipmap, linear);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_Create(Texture2D mono, int width, int height, TextureFormat format, bool mipmap, bool linear);

		public void SetPixel(int x, int y, Color color)
		{
			INTERNAL_CALL_SetPixel(this, x, y, ref color);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetPixel(Texture2D self, int x, int y, ref Color color);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color GetPixel(int x, int y);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color GetPixelBilinear(float u, float v);

		public void SetPixels(Color[] colors)
		{
			int miplevel = 0;
			SetPixels(colors, miplevel);
		}

		public void SetPixels(Color[] colors, int miplevel)
		{
			int num = width >> miplevel;
			if (num < 1)
			{
				num = 1;
			}
			int num2 = height >> miplevel;
			if (num2 < 1)
			{
				num2 = 1;
			}
			SetPixels(0, 0, num, num2, colors, miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetPixels(int x, int y, int blockWidth, int blockHeight, Color[] colors, int miplevel);

		public void SetPixels(int x, int y, int blockWidth, int blockHeight, Color[] colors)
		{
			int miplevel = 0;
			SetPixels(x, y, blockWidth, blockHeight, colors, miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetPixels32(Color32[] colors, int miplevel);

		public void SetPixels32(Color32[] colors)
		{
			int miplevel = 0;
			SetPixels32(colors, miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool LoadImage(byte[] data);

		public Color[] GetPixels()
		{
			int miplevel = 0;
			return GetPixels(miplevel);
		}

		public Color[] GetPixels(int miplevel)
		{
			int num = width >> miplevel;
			if (num < 1)
			{
				num = 1;
			}
			int num2 = height >> miplevel;
			if (num2 < 1)
			{
				num2 = 1;
			}
			return GetPixels(0, 0, num, num2, miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color[] GetPixels(int x, int y, int blockWidth, int blockHeight, int miplevel);

		public Color[] GetPixels(int x, int y, int blockWidth, int blockHeight)
		{
			int miplevel = 0;
			return GetPixels(x, y, blockWidth, blockHeight, miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color32[] GetPixels32(int miplevel);

		public Color32[] GetPixels32()
		{
			int miplevel = 0;
			return GetPixels32(miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Apply(bool updateMipmaps, bool makeNoLongerReadable);

		public void Apply(bool updateMipmaps)
		{
			bool makeNoLongerReadable = false;
			Apply(updateMipmaps, makeNoLongerReadable);
		}

		public void Apply()
		{
			bool makeNoLongerReadable = false;
			bool updateMipmaps = true;
			Apply(updateMipmaps, makeNoLongerReadable);
		}

		public bool Resize(int width, int height, TextureFormat format, bool hasMipMap)
		{
			return INTERNAL_CALL_Resize(this, width, height, format, hasMipMap);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern bool INTERNAL_CALL_Resize(Texture2D self, int width, int height, TextureFormat format, bool hasMipMap);

		public bool Resize(int width, int height)
		{
			return Internal_ResizeWH(width, height);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern bool Internal_ResizeWH(int width, int height);

		public void Compress(bool highQuality)
		{
			INTERNAL_CALL_Compress(this, highQuality);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_Compress(Texture2D self, bool highQuality);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Rect[] PackTextures(Texture2D[] textures, int padding, int maximumAtlasSize, bool makeNoLongerReadable);

		public Rect[] PackTextures(Texture2D[] textures, int padding, int maximumAtlasSize)
		{
			bool makeNoLongerReadable = false;
			return PackTextures(textures, padding, maximumAtlasSize, makeNoLongerReadable);
		}

		public Rect[] PackTextures(Texture2D[] textures, int padding)
		{
			bool makeNoLongerReadable = false;
			int maximumAtlasSize = 2048;
			return PackTextures(textures, padding, maximumAtlasSize, makeNoLongerReadable);
		}

		public void ReadPixels(Rect source, int destX, int destY, bool recalculateMipMaps)
		{
			INTERNAL_CALL_ReadPixels(this, ref source, destX, destY, recalculateMipMaps);
		}

		public void ReadPixels(Rect source, int destX, int destY)
		{
			bool recalculateMipMaps = true;
			INTERNAL_CALL_ReadPixels(this, ref source, destX, destY, recalculateMipMaps);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_ReadPixels(Texture2D self, ref Rect source, int destX, int destY, bool recalculateMipMaps);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern byte[] EncodeToPNG();
	}
}
