using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class WheelCollider : Collider
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

		public float radius
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float suspensionDistance
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public JointSpring suspensionSpring
		{
			get
			{
				INTERNAL_get_suspensionSpring(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_suspensionSpring(ref value);
			}
		}

		public float mass
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public WheelFrictionCurve forwardFriction
		{
			get
			{
				INTERNAL_get_forwardFriction(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_forwardFriction(ref value);
			}
		}

		public WheelFrictionCurve sidewaysFriction
		{
			get
			{
				INTERNAL_get_sidewaysFriction(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_sidewaysFriction(ref value);
			}
		}

		public float motorTorque
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float brakeTorque
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float steerAngle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool isGrounded
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float rpm
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_center(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_center(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_suspensionSpring(out JointSpring value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_suspensionSpring(ref JointSpring value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_forwardFriction(out WheelFrictionCurve value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_forwardFriction(ref WheelFrictionCurve value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_sidewaysFriction(out WheelFrictionCurve value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_sidewaysFriction(ref WheelFrictionCurve value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		public extern bool GetGroundHit(out WheelHit hit);
	}
}
