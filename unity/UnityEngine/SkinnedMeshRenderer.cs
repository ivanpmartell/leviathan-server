using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class SkinnedMeshRenderer : Renderer
	{
		public Transform[] bones
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public SkinQuality quality
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Mesh sharedMesh
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[Obsolete("Has no effect.")]
		public bool skinNormals
		{
			get
			{
				return true;
			}
			set
			{
			}
		}

		public bool updateWhenOffscreen
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Bounds localBounds
		{
			get
			{
				INTERNAL_get_localBounds(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_localBounds(ref value);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_localBounds(out Bounds value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_localBounds(ref Bounds value);
	}
}
