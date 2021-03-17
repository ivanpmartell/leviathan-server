using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class Cubemap : Texture
	{
		public TextureFormat format
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public Cubemap(int size, TextureFormat format, bool mipmap)
		{
			Internal_Create(this, size, format, mipmap);
		}

		public void SetPixel(CubemapFace face, int x, int y, Color color)
		{
			INTERNAL_CALL_SetPixel(this, face, x, y, ref color);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_SetPixel(Cubemap self, CubemapFace face, int x, int y, ref Color color);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color GetPixel(CubemapFace face, int x, int y);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern Color[] GetPixels(CubemapFace face, int miplevel);

		public Color[] GetPixels(CubemapFace face)
		{
			int miplevel = 0;
			return GetPixels(face, miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void SetPixels(Color[] colors, CubemapFace face, int miplevel);

		public void SetPixels(Color[] colors, CubemapFace face)
		{
			int miplevel = 0;
			SetPixels(colors, face, miplevel);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern void Apply(bool updateMipmaps);

		public void Apply()
		{
			bool updateMipmaps = true;
			Apply(updateMipmaps);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_Create(Cubemap mono, int size, TextureFormat format, bool mipmap);
	}
}
