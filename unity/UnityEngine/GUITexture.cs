using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class GUITexture : GUIElement
	{
		public Color color
		{
			get
			{
				INTERNAL_get_color(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_color(ref value);
			}
		}

		public Texture texture
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Rect pixelInset
		{
			get
			{
				INTERNAL_get_pixelInset(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_pixelInset(ref value);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_color(out Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_color(ref Color value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_pixelInset(out Rect value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_pixelInset(ref Rect value);
	}
}
