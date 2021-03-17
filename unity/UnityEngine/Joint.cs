using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public class Joint : Component
	{
		public Rigidbody connectedBody
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector3 axis
		{
			get
			{
				INTERNAL_get_axis(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_axis(ref value);
			}
		}

		public Vector3 anchor
		{
			get
			{
				INTERNAL_get_anchor(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_anchor(ref value);
			}
		}

		public float breakForce
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float breakTorque
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_axis(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_axis(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_anchor(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_anchor(ref Vector3 value);
	}
}
