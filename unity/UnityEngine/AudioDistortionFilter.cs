using System.Runtime.CompilerServices;

namespace UnityEngine
{
	public sealed class AudioDistortionFilter : Behaviour
	{
		public float distortionLevel
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			[WrapperlessIcall]
			set;
		}
	}
}
