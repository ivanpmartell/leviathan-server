using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class ParticleAnimator : Component
	{
		public bool doesAnimateColor
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector3 worldRotationAxis
		{
			get
			{
				INTERNAL_get_worldRotationAxis(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_worldRotationAxis(ref value);
			}
		}

		public Vector3 localRotationAxis
		{
			get
			{
				INTERNAL_get_localRotationAxis(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_localRotationAxis(ref value);
			}
		}

		public float sizeGrow
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Vector3 rndForce
		{
			get
			{
				INTERNAL_get_rndForce(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_rndForce(ref value);
			}
		}

		public Vector3 force
		{
			get
			{
				INTERNAL_get_force(out var value);
				return value;
			}
			set
			{
				INTERNAL_set_force(ref value);
			}
		}

		public float damping
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public bool autodestruct
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public Color[] colorAnimation
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
		private extern void INTERNAL_get_worldRotationAxis(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_worldRotationAxis(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_localRotationAxis(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_localRotationAxis(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_rndForce(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_rndForce(ref Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_get_force(out Vector3 value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private extern void INTERNAL_set_force(ref Vector3 value);
	}
}
