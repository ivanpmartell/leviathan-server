using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class HingeJoint : Joint
	{
		public JointMotor motor
		{
			get
			{
				INTERNAL_get_motor(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_motor(ref value);
			}
		}

		public JointLimits limits
		{
			get
			{
				INTERNAL_get_limits(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_limits(ref value);
			}
		}

		public JointSpring spring
		{
			get
			{
				INTERNAL_get_spring(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_spring(ref value);
			}
		}

		public bool useMotor
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool useLimits
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool useSpring
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float velocity
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float angle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_motor(out JointMotor value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_motor(ref JointMotor value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_limits(out JointLimits value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_limits(ref JointLimits value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_spring(out JointSpring value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_spring(ref JointSpring value);
	}
}
