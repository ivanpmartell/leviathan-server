using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class SkinnedCloth : Cloth
	{
		public ClothSkinningCoefficient[] coefficients
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float worldVelocityScale
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public float worldAccelerationScale
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
		public extern void SetEnabledFading(bool enabled, float interpolationTime);

		public void SetEnabledFading(bool enabled)
		{
			float interpolationTime = 0.5f;
			SetEnabledFading(enabled, interpolationTime);
		}
	}
}
