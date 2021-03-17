using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class Texture : Object
	{
		public static int masterTextureLimit
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public static AnisotropicFiltering anisotropicFiltering
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public virtual int width
		{
			get
			{
				return Internal_GetWidth(this);
			}
			set
			{
				throw new Exception("not implemented");
			}
		}

		public virtual int height
		{
			get
			{
				return Internal_GetHeight(this);
			}
			set
			{
				throw new Exception("not implemented");
			}
		}

		public FilterMode filterMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int anisoLevel
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public TextureWrapMode wrapMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float mipMapBias
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector2 texelSize
		{
			get
			{
				Internal_GetTexelSize(this, out var output);
				return output;
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern int Internal_GetWidth(Texture mono);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern int Internal_GetHeight(Texture mono);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void Internal_GetTexelSize(Texture tex, out Vector2 output);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern int GetNativeTextureID();
	}
}
