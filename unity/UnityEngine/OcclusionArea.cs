using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class OcclusionArea : Component
	{
		public Vector3 center
		{
			get
			{
				INTERNAL_get_center(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_center(ref value);
			}
		}

		public Vector3 size
		{
			get
			{
				INTERNAL_get_size(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_size(ref value);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_center(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_center(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_size(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_size(ref Vector3 value);
	}
}
