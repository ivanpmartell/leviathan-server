using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class ConfigurableJoint : Joint
	{
		public Vector3 secondaryAxis
		{
			get
			{
				INTERNAL_get_secondaryAxis(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_secondaryAxis(ref value);
			}
		}

		public ConfigurableJointMotion xMotion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public ConfigurableJointMotion yMotion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public ConfigurableJointMotion zMotion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public ConfigurableJointMotion angularXMotion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public ConfigurableJointMotion angularYMotion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public ConfigurableJointMotion angularZMotion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public SoftJointLimit linearLimit
		{
			get
			{
				INTERNAL_get_linearLimit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_linearLimit(ref value);
			}
		}

		public SoftJointLimit lowAngularXLimit
		{
			get
			{
				INTERNAL_get_lowAngularXLimit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_lowAngularXLimit(ref value);
			}
		}

		public SoftJointLimit highAngularXLimit
		{
			get
			{
				INTERNAL_get_highAngularXLimit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_highAngularXLimit(ref value);
			}
		}

		public SoftJointLimit angularYLimit
		{
			get
			{
				INTERNAL_get_angularYLimit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_angularYLimit(ref value);
			}
		}

		public SoftJointLimit angularZLimit
		{
			get
			{
				INTERNAL_get_angularZLimit(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_angularZLimit(ref value);
			}
		}

		public Vector3 targetPosition
		{
			get
			{
				INTERNAL_get_targetPosition(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_targetPosition(ref value);
			}
		}

		public Vector3 targetVelocity
		{
			get
			{
				INTERNAL_get_targetVelocity(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_targetVelocity(ref value);
			}
		}

		public JointDrive xDrive
		{
			get
			{
				INTERNAL_get_xDrive(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_xDrive(ref value);
			}
		}

		public JointDrive yDrive
		{
			get
			{
				INTERNAL_get_yDrive(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_yDrive(ref value);
			}
		}

		public JointDrive zDrive
		{
			get
			{
				INTERNAL_get_zDrive(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_zDrive(ref value);
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

		public RotationDriveMode rotationDriveMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public JointDrive angularXDrive
		{
			get
			{
				INTERNAL_get_angularXDrive(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_angularXDrive(ref value);
			}
		}

		public JointDrive angularYZDrive
		{
			get
			{
				INTERNAL_get_angularYZDrive(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_angularYZDrive(ref value);
			}
		}

		public JointDrive slerpDrive
		{
			get
			{
				INTERNAL_get_slerpDrive(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_slerpDrive(ref value);
			}
		}

		public JointProjectionMode projectionMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float projectionDistance
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float projectionAngle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool configuredInWorldSpace
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool swapBodies
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
		private extern void INTERNAL_get_secondaryAxis(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_secondaryAxis(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_linearLimit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_linearLimit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_lowAngularXLimit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_lowAngularXLimit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_highAngularXLimit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_highAngularXLimit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_angularYLimit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_angularYLimit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_angularZLimit(out SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_angularZLimit(ref SoftJointLimit value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_targetPosition(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_targetPosition(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_targetVelocity(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_targetVelocity(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_xDrive(out JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_xDrive(ref JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_yDrive(out JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_yDrive(ref JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_zDrive(out JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_zDrive(ref JointDrive value);

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
		private extern void INTERNAL_get_angularXDrive(out JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_angularXDrive(ref JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_angularYZDrive(out JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_angularYZDrive(ref JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_slerpDrive(out JointDrive value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_slerpDrive(ref JointDrive value);
	}
}
