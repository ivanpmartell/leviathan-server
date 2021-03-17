using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class CharacterJoint : Joint
	{
		public Vector3 swingAxis
		{
			get
			{
				INTERNAL_get_swingAxis(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_swingAxis(ref value);
			}
		}

		public SoftJointLimit lowTwistLimit
		{
			get
			{
				INTERNAL_get_lowTwistLimit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_lowTwistLimit(ref value);
			}
		}

		public SoftJointLimit highTwistLimit
		{
			get
			{
				INTERNAL_get_highTwistLimit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_highTwistLimit(ref value);
			}
		}

		public SoftJointLimit swing1Limit
		{
			get
			{
				INTERNAL_get_swing1Limit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_swing1Limit(ref value);
			}
		}

		public SoftJointLimit swing2Limit
		{
			get
			{
				INTERNAL_get_swing2Limit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_swing2Limit(ref value);
			}
		}

		public Quaternion targetRotation
		{
			get
			{
				INTERNAL_get_targetRotation(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_targetRotation(ref value);
			}
		}

		public Vector3 targetAngularVelocity
		{
			get
			{
				INTERNAL_get_targetAngularVelocity(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_targetAngularVelocity(ref value);
			}
		}

		public JointDrive rotationDrive
		{
			get
			{
				INTERNAL_get_rotationDrive(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_rotationDrive(ref value);
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_swingAxis(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_swingAxis(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_lowTwistLimit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_lowTwistLimit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_highTwistLimit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_highTwistLimit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_swing1Limit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_swing1Limit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_swing2Limit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_swing2Limit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_targetRotation(out Quaternion value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_targetRotation(ref Quaternion value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_targetAngularVelocity(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_targetAngularVelocity(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_rotationDrive(out JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_rotationDrive(ref JointDrive value);
	}
}
