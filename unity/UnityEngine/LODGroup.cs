using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class LODGroup : Component
	{
		public Vector3 localReferencePoint
		{
			get
			{
				INTERNAL_get_localReferencePoint(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_localReferencePoint(ref value);
			}
		}

		public float size
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int lodCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_localReferencePoint(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_localReferencePoint(ref Vector3 value);
	}
}
