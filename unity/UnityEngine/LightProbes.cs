using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class LightProbes : Object
	{
		public Vector3[] positions
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public float[] coefficients
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}

		public int count
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public int cellCount
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
		}

		public void GetInterpolatedLightProbe(Vector3 position, Renderer renderer, float[] coefficients)
		{
			INTERNAL_CALL_GetInterpolatedLightProbe(this, ref position, renderer, coefficients);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		[WrapperlessIcall]
		private static extern void INTERNAL_CALL_GetInterpolatedLightProbe(LightProbes self, ref Vector3 position, Renderer renderer, float[] coefficients);
	}
}
